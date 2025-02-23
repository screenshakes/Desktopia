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

using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace Desktopia
{
    public static class Taskbar
    {
        #region Windows API
        [DllImport("shell32.dll")]
        static extern IntPtr SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

        [StructLayout(LayoutKind.Sequential)]
        struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left, top, right, bottom;
        }

        const int ABM_GETTASKBARPOS = 5;

        public enum TaskbarPosition
        {
            Unknown = -1,
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        public struct TaskbarInfos
        {
            public TaskbarPosition Position;
            public Rect Rectangle;
        }

        #endregion

        #region Publics
        
        /// <summary>
        /// Returns the position and rectangle of the taskbar
        /// </summary>
        public static TaskbarInfos GetInfos()
        {
            var abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(typeof(APPBARDATA));

            var result = SHAppBarMessage(ABM_GETTASKBARPOS, ref abd);

            if (result != IntPtr.Zero)
            {
                return new TaskbarInfos
                {
                    Position = (TaskbarPosition) abd.uEdge,
                    Rectangle = new Rect(abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top)
                };
            }
            else
            {
                return new TaskbarInfos() { Position = TaskbarPosition.Unknown };
            }
        }
        #endregion
    }
}