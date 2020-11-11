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
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Desktopia
{
    public static partial class Files
    {
        public static class DragDrop
        {
            #region Windows API
            struct Point { public int x, y; }
            struct Message
            {
                public IntPtr hwnd;
                public uint message;
                public IntPtr wParam;
                public IntPtr lParam;
                public ushort time;
                public Point pt;
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            static extern IntPtr SetWindowsHookEx(int idHook, DragDropProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref Message lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            static extern uint DragQueryFile(IntPtr hDrop, uint iFile, System.Text.StringBuilder lpszFile, uint cch);

            [DllImport("shell32.dll")]
            static extern void DragFinish(IntPtr hDrop);

            [DllImport("shell32.dll")]
            static extern void DragAcceptFiles(IntPtr hWnd, bool fAccept);

            [DllImport("kernel32.dll")]
            static extern uint GetCurrentThreadId();

            [DllImport("shell32.dll")]
            static extern void DragQueryPoint(IntPtr hDrop, out Point pos);

            delegate IntPtr DragDropProc(int nCode, IntPtr wParam, ref Message lParam);
            #endregion

            #region Constants
            const int WM_DROPFILES = 0x233;
            const int WH_GETMESSAGE = 3;
            #endregion

            static IntPtr hook;
            static List<Action<string[], Vector2>> onDrop;

            internal static void Enable()
            {
                hook = CreateHook();
                onDrop = new List<Action<string[], Vector2>>();
            }
            
            internal static void Disable()
            {
                UnhookWindowsHookEx(hook);
                DragAcceptFiles(Windows.Main.Handle, false);
            }

            static IntPtr CreateHook()
            {
                uint threadId = GetCurrentThreadId();

                DragAcceptFiles(Windows.Main.Handle, true);
                return SetWindowsHookEx(
                    WH_GETMESSAGE,
                    Callback,
                    GetModuleHandle(null),
                    threadId
                );
            }

            static IntPtr Callback(int nCode, IntPtr wParam, ref Message lParam)
            {
                if (nCode == 0 && lParam.message == WM_DROPFILES)
                {
                    Point pt;
                    DragQueryPoint(lParam.wParam, out pt);
                    var position = new Vector2(pt.x, pt.y);

                    uint fileCount = DragQueryFile(lParam.wParam, 0xFFFFFFFF, null, 0);
                    StringBuilder builder = new StringBuilder(1024);
                    String[] files = new string[fileCount];

                    for(uint i = 0; i < fileCount; ++i)
                    {
                        int length = (int) DragQueryFile(lParam.wParam, i, builder, 1024);
                        files[i] = builder.ToString(0, length);
                        builder.Length = 0;
                    }

                    DragFinish(lParam.wParam);
                        
                    foreach(var a in onDrop) a.Invoke(files, position);
                }

                return CallNextHookEx(hook, nCode, wParam, ref lParam);
            }

            /// <summary>
            /// Adds a function that is called when a file is dropped in the window.
            /// </summary>
            public static Action<string[], Vector2> AddCallback(Action<string[], Vector2> callback)
            {
                onDrop.Add(callback);
                return callback;
            }

            /// <summary>
            /// Removes a callback.
            /// </summary>
            public static void RemoveCallback(Action<string[], Vector2> callback)
            {
                onDrop.Remove(callback);
            }
        }
    }
}