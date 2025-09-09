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
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam,
            uint fuFlags, uint uTimeout, out IntPtr lpdwResult);
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetDesktopWindow();

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        #endregion

        #region Constants
        internal const int GWL_STYLE   = -16;
        internal const int GWL_EXSTYLE = -20;
        
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        const int SW_MAXIMIZE = 3;
        const int SW_MINIMIZE = 6;

        const uint WS_POPUP          = 0x80000000;
        const uint WS_EX_LAYERED     = 0x00080000;
        const uint WS_EX_TRANSPARENT = 0x00000020;
        
        const uint WS_CHILD       = 0x40000000;
        const uint WS_VISIBLE     = 0x10000000;
        const uint WS_CLIPSIBLINGS= 0x04000000;
        const uint WS_CLIPCHILDREN= 0x02000000;

        const uint WS_EX_NOACTIVATE   = 0x08000000;
        const uint WS_EX_TOOLWINDOW   = 0x00000080;

        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_SHOWWINDOW = 0x0040;

        const int WM_SETTEXT = 0x000C;
        
        const int HWND_TOPMOST   = -1;
        const int HWND_NOTOPMOST = -2;

        const int WM_SPAWN_WORKER = 0x052C;
        const int SMTO_BLOCK = 0x0001;
        const int LWA_ALPHA = 0x0002;
        const int SWP_SHOWWINDOW_AND_FRAMECHANGED = 0x0060;
        #endregion

        #region Data
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion
        
        internal IntPtr Handle;
        uint baseStyle;
        
        static IntPtr wallpaperClickWindow1 = IntPtr.Zero;
        static IntPtr wallpaperClickWindow2 = IntPtr.Zero;

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
        /// Sets wether or not the window is placed in front of all the other non-topmost windows.
        /// </summary>
        public void SetTopMost(bool topMost)
        {
            SetWindowPos(Handle, topMost ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
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
        
        /// <summary>
        ///  Sets the window as wallpaper.
        /// </summary>
        public void SetAsWallpaper()
        {
            var targetRect = Rect;

            var progman = FindWindow("Progman", "Program Manager");
            
            if (progman == IntPtr.Zero)
            {
                progman = GetDesktopWindow();
                if (progman == IntPtr.Zero)
                {
                    return;
                }
            }

            IntPtr result;
            SendMessageTimeout(progman, WM_SPAWN_WORKER, IntPtr.Zero, IntPtr.Zero, SMTO_BLOCK, 1000U, out result);

            SendMessageTimeout(progman, WM_SPAWN_WORKER, new IntPtr(13), new IntPtr(1), SMTO_BLOCK, 1000U, out result);

            var workerw = IntPtr.Zero;
            var wallpaperWorker = IntPtr.Zero;

            while (true)
            {
                workerw = FindWindowEx(IntPtr.Zero, workerw, "WorkerW", null);
                if (workerw == IntPtr.Zero) break;

                var parent = GetParent(workerw);
                if (parent == progman)
                {
                    wallpaperWorker = workerw;
                    break;
                }
            }

            if (wallpaperWorker == IntPtr.Zero)
            {
                EnumWindows((topHandle, lParam) =>
                {
                    var defView = FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (defView != IntPtr.Zero)
                    {
                        var foundWorker = FindWindowEx(topHandle, defView, "WorkerW", null);
                        if (foundWorker != IntPtr.Zero)
                        {
                            wallpaperWorker = foundWorker;
                            wallpaperClickWindow1 = defView;
                            wallpaperClickWindow2 = FindWindowEx(defView, IntPtr.Zero, "SysListView32", null);
                            return false;
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }

            if (wallpaperWorker == IntPtr.Zero)
            {
                wallpaperWorker = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "WorkerW", null);
                if (wallpaperWorker == IntPtr.Zero)
                {
                    return;
                }
            }

            SetParent(Handle, wallpaperWorker);
            SetWindowLong(Handle, -16, 1342177280U);
            SetWindowLong(Handle, -20, 134742016U);
            SetLayeredWindowAttributes(Handle, 0U, byte.MaxValue, LWA_ALPHA);

            var bgRect = default(RECT);
            if (GetWindowRect(wallpaperWorker, ref bgRect))
            {
                var offsetX = (int)targetRect.x - bgRect.Left;
                var offsetY = (int)targetRect.y - bgRect.Top;
                var width   = (int)targetRect.width;
                var height  = (int)targetRect.height;

                SetWindowPos(Handle, 0, offsetX, offsetY, width, height, SWP_SHOWWINDOW_AND_FRAMECHANGED);
            }
            else
            {
                SetWindowPos(Handle, 0, (int)targetRect.x, (int)targetRect.y, (int)targetRect.width, (int)targetRect.height, SWP_SHOWWINDOW_AND_FRAMECHANGED);
            }
        }
        #endregion
    }
}