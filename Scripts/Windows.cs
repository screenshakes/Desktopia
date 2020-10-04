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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System;
using UnityEngine;

namespace Desktopia
{
    public static class Windows
    {
        #region Windows API
        enum DwmWindowAttribute
        {
            NcRenderingEnabled = 1,
            NcRenderingPolicy,
            TransitionsForcedisabled,
            AllowNcPaint,
            CaptionButtonBounds,
            NonclientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation,
            Last
        }

        delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr window, ref Window.NativeRect rectangle);

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr window, int dwAttribute, out bool pvAttribute, int cbAttribute);
        #endregion

        public static Window Main;
        static Dictionary<IntPtr, Window> dictionary;
        static List<IntPtr> removedWindows;
        static List<Action<Window>> onWindowOpened, onWindowClosed;

        internal static void GetMain()
        {
            var mainHandle = GetActiveWindow();
            Main = new Window(mainHandle, GetStyle(mainHandle), GetTitle(mainHandle), GetRect(mainHandle));
        }

        internal static void Initialize(Core core)
        {
            core.SubscribeModule(Update);

            dictionary     = new Dictionary<IntPtr, Window>();
            onWindowOpened = new List<Action<Window>>();
            onWindowClosed = new List<Action<Window>>();
            removedWindows = new List<IntPtr>();
        }

        static void Update()
        {
            IntPtr shellWindow = GetShellWindow();

            foreach(var ptr in dictionary.Keys)
            removedWindows.Add(ptr);

            EnumDesktopWindows(IntPtr.Zero, delegate(IntPtr handle, int lParam)
            {
                if (handle == shellWindow) return true;
                if (!IsWindowVisible(handle)) return true;

                int length = GetWindowTextLength(handle);
                if (length == 0) return true;

                DwmGetWindowAttribute(handle, (int)DwmWindowAttribute.Cloaked, out bool isCloacked, Marshal.SizeOf(typeof(bool)));
                if (isCloacked) return true;    

                if(dictionary.ContainsKey(handle))
                {
                    var window = dictionary[handle];
                    window.Title = GetTitle(handle, length);
                    window.Rect  = GetRect(handle);
                    removedWindows.Remove(handle);
                }
                else
                {
                    var window = new Window(handle, GetWindowLong(handle, Window.GWL_STYLE), GetTitle(handle, length), GetRect(handle));
                    dictionary.Add(handle, window);
                    
                    foreach(var a in onWindowOpened)
                        a.Invoke(window);
                }
                
                return true;
            }, IntPtr.Zero);

            foreach(var ptr in removedWindows)
            {
                foreach(var a in onWindowClosed)
                    a.Invoke(dictionary[ptr]);

                dictionary.Remove(ptr);
            }

            removedWindows.Clear();
        }

        #region Helpers
        static string GetTitle(IntPtr handle, int length = 128)
        {
            StringBuilder builder = new StringBuilder(length);
            GetWindowText(handle, builder, length + 1);
            return builder.ToString();
        }

        static uint GetStyle(IntPtr handle)
        {
            return GetWindowLong(handle, Window.GWL_STYLE);
        }

        internal static uint GetEXStyle(IntPtr handle)
        {
            return GetWindowLong(handle, Window.GWL_EXSTYLE);
        }

        static Rect GetRect(IntPtr handle)
        {
            Window.NativeRect rect = new Window.NativeRect();
            GetWindowRect(handle, ref rect);     
            return new UnityEngine.Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }
        #endregion

        #region Publics
        /// <summary>
        /// Returns the desktop windows list.
        /// </summary>
        public static Dictionary<IntPtr, Window>.ValueCollection List { get { return dictionary.Values; } }

        /// <summary>
        /// Adds an action that is executed when a new window is opened
        /// </summary>
        public static Action<Window> AddOnWindowOpened(Action<Window> action)
        {
            onWindowOpened.Add(action);
            return action;
        }

         /// <summary>
        /// Removes an action that is executed when a new window is opened
        /// </summary>
        public static Action<Window> RemoveOnWindowOpened(Action<Window> action)
        {
            onWindowOpened.Remove(action);
            return action;
        }

        /// <summary>
        /// Adds an action that is executed when a window is closed
        /// </summary>
        public static Action<Window> AddOnWindowClosed(Action<Window> action)
        {
            onWindowClosed.Add(action);
            return action;
        }

         /// <summary>
        /// Removes an action that is executed when a window is closed
        /// </summary>
        public static Action<Window> RemoveOnWindowClosed(Action<Window> action)
        {
            onWindowClosed.Remove(action);
            return action;
        }
        #endregion
    }
}