/*
Desktopia - Made by Constantin Liétard - https://twitter.com/screenshakes

MIT License

Copyright (c) 2020 Constantin Liétard

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Desktopia
{
    public class Window
    {
        #region Windows API
        internal struct Margins { public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight; }
        internal struct NativeRect { public int left, top, right, bottom; }

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        static extern int SetWindowPos(IntPtr hWnd, int windowInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
		static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

        [DllImport("User32.dll")]
		static extern Int32 SendMessage(IntPtr hWnd, int Msg, int wParam, string lParam);

        [DllImport("dwmapi.dll")]
        static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins margins);
        #endregion

        #region Constants
        internal const int GWL_STYLE   = -16;
        internal const int GWL_EXSTYLE = -20;

        const int SW_MAXIMIZE = 3;
        const int SW_MINIMIZE = 6;

        const uint WS_POPUP          = 0x80000000;
        const uint WS_VISIBLE        = 0x10000000;
        const uint WS_EX_LAYERED     = 0x00080000;
        const uint WS_EX_TRANSPARENT = 0x00000020;

        const int WM_SETTEXT = 0x000C;
        
        const int HWND_TOPMOST   = -1;
        const int HWND_NOTOPMOST = -2;
        #endregion

        internal IntPtr Handle;
        uint baseStyle;

        public Rect Rect         { get; internal set; }
        public string Title      { get; internal set; }
        public bool Transparent  { get; internal set; }
        public bool ClickThrough { get; internal set; }

        internal Window(IntPtr handle, uint baseStyle, string title, Rect rect)
        {
            this.baseStyle = baseStyle;
            Handle = handle;

            Title = title;
            Rect  = rect;
        }

        #region Publics
        /// <summary>
        /// Returns the windows status.
        /// </summary>
        public string Status()
        {
            return "[Desktopia/Window] Handle:" + Handle + " Title:" + Title + " Rect:" + Rect;
        }

        /// <summary>
        /// Moves the window of a given distance, in pixel.
        /// </summary>
        public void Move(int x, int y)
        {
            MoveWindow(
                Handle,
                (int) Rect.x + x,
                (int) Rect.y + y,
                (int) Rect.width,
                (int) Rect.height,
                true
            );

            Rect.Set(Rect.x + x, Rect.y + y, Rect.width, Rect.height);
        }

        /// <summary>
        /// Moves the window of a given distance, in pixel.
        /// </summary>
        public void Move(Vector2 distance)
        {
            Move((int) distance.x, (int) distance.y);
        }

        /// <summary>
        /// Sets the position of the window, in pixel;
        /// </summary>
        public void SetPosition(int x, int y)
        {
            MoveWindow(
                Handle,
                x,
                y,
                (int) Rect.width,
                (int) Rect.height,
                true
            );

            Rect.Set(x, y, Rect.width, Rect.height);
        }

        /// <summary>
        /// Sets the position of the window, in pixel;
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            SetPosition((int) position.x, (int) position.y);
        }

        /// <summary>
        /// Sets the size of the window, in pixel;
        /// </summary>
        public void SetSize(int width, int height)
        {
            MoveWindow(
                Handle,
                (int) Rect.x,
                (int) Rect.y,
                width,
                height,
                true
            );

            Rect.Set(Rect.x, Rect.y, width, height);
        }

        /// <summary>
        /// Sets the size of the window, in pixel.
        /// </summary>
        public void SetSize(Vector2 size)
        {
            SetSize((int) size.x, (int) size.y);
        }

        /// <summary>
        /// Sets wether or not the window is place in front of all the other non-topmost windows.
        /// </summary>
        public void SetTopMost(bool topMost)
        {
            SetWindowPos(Handle, topMost ? HWND_TOPMOST : HWND_NOTOPMOST, (int) Rect.x, (int) Rect.y, (int) Rect.width, (int) Rect.height, 32 | 64);
        }

        /// <summary>
        /// Sets wether or not the window is transparent.
        /// </summary>
        public void SetTransparent(bool transparent)
        {
            Transparent = transparent;
            SetWindowLong(Handle, GWL_STYLE, transparent ? WS_POPUP | WS_VISIBLE : baseStyle);

            int marginValue = transparent ? -1 : 0;
            Margins margins = new Margins() { cxLeftWidth = marginValue, cxRightWidth = marginValue, cyTopHeight = marginValue, cyBottomHeight = marginValue };
            DwmExtendFrameIntoClientArea(Handle, ref margins);
        }

        /// <summary>
        /// Sets wether or not the window ignores clicks.
        /// </summary>
        public void SetClickThrough(bool clickThrough)
        {
            ClickThrough = clickThrough;
            uint currentStyle = Windows.GetEXStyle(Handle);
            SetWindowLong(Handle, GWL_EXSTYLE, clickThrough ? currentStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT : currentStyle & ~(WS_EX_LAYERED | WS_EX_TRANSPARENT));
        }

        /// <summary>
        /// Brings the window to the foreground.
        /// </summary>
        public void Focus()
        {
            SetForegroundWindow(Handle);
        }

        /// <summary>
        /// Minimizes the window.
        /// </summary>
        public void Minimize()
        {
            ShowWindow(Handle, SW_MINIMIZE);
        }

        /// <summary>
        /// Maximizes the window.
        /// </summary>
        public void Maximize()
        {
            ShowWindow(Handle, SW_MAXIMIZE);
        }

        /// <summary>
        /// Changes the title of the window.
        /// </summary>
        public void SetTitle(string title)
        {
            SetWindowText(Handle, title);
            Title = title;
        }

        /// <summary>
        /// Sends key down message to the window.
        /// </summary>
        public void SendKeyDown(KeyCode keyCode)
        {
            PostMessage(Handle, Inputs.WM_KEYDOWN, (uint) Inputs.KeyCodeToVKCode[keyCode], 0);
        }

        /// <summary>
        /// Sends key pressed message to the window.
        /// </summary>
        public void SendKey(KeyCode keyCode)
        {
            PostMessage(Handle, Inputs.WM_KEYDOWN, (uint) Inputs.KeyCodeToVKCode[keyCode], (uint) 1 << 30);
        }

        /// <summary>
        /// Sends key up message to the window.
        /// </summary>
        public void SendKeyUp(KeyCode keyCode)
        {
           PostMessage(Handle, Inputs.WM_KEYUP, (uint) Inputs.KeyCodeToVKCode[keyCode], (uint) 1 << 30 | (uint) 1 << 31);
        }

        /// <summary>
        /// Sends text message to the window.
        /// </summary>
        public void SendText(string text)
        {
            SendMessage(Handle, WM_SETTEXT, 0, text);
        }
        #endregion
    }
}