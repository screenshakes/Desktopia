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
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Desktopia
{
    public static class Inputs
    {
        #region Windows API
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        #endregion

        #region Constants
        const int WH_KEYBOARD_LL = 13;
        const int WH_MOUSE_LL = 14;

        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP   = 0x0101;
        
        const int WM_MOUSEMOVE = 512;
        const int WM_LBUTTONDOWN = 513;
        const int WM_LBUTTONUP = 514;
        const int WM_MBUTTONDOWN = 519;
        const int WM_MBUTTONUP = 520;
        const int WM_RBUTTONDOWN = 516;
        const int WM_RBUTTONUP = 517;

        readonly static Dictionary<int, (int, bool)> MouseCodesToButtons = new Dictionary<int, (int, bool)>()
        {
            { WM_LBUTTONDOWN, (0, true)},
            { WM_LBUTTONUP,   (0, false)},
            { WM_RBUTTONDOWN, (1, true)},
            { WM_RBUTTONUP,   (1, false)},
            { WM_MBUTTONDOWN, (2, true)},
            { WM_MBUTTONUP,   (2, false)},
        };

        readonly static Dictionary<int, KeyCode> VKCodeToKeyCode = new Dictionary<int, KeyCode>()
        {
            { 0x08, KeyCode.Backspace },
            { 0x09, KeyCode.Tab },
            { 0x0C, KeyCode.Clear },
            { 0x0D, KeyCode.Return },
            { 0x13, KeyCode.Pause },
            { 0x1B, KeyCode.Escape },
            { 0x20, KeyCode.Space },
            { 0xBB, KeyCode.Plus },
            { 0xBC, KeyCode.Comma },
            { 0xBD, KeyCode.Minus },
            { 0xBE, KeyCode.Period },
            { 0x2F, KeyCode.Help },
            { 0x30, KeyCode.Alpha0 },
            { 0x31, KeyCode.Alpha1 },
            { 0x32, KeyCode.Alpha2 },
            { 0x33, KeyCode.Alpha3 },
            { 0x34, KeyCode.Alpha4 },
            { 0x35, KeyCode.Alpha5 },
            { 0x36, KeyCode.Alpha6 },
            { 0x37, KeyCode.Alpha7 },
            { 0x38, KeyCode.Alpha8 },
            { 0x39, KeyCode.Alpha9 },
            { 0x2C, KeyCode.Colon },
            { 0x3C, KeyCode.Less },
            { 0x3D, KeyCode.Equals },
            { 0x3E, KeyCode.Greater },
            { 0x3F, KeyCode.Question },
            { 0x40, KeyCode.At },
            { 0x5E, KeyCode.Caret },
            { 0x5F, KeyCode.Underscore },
            { 0x41, KeyCode.A },
            { 0x42, KeyCode.B },
            { 0x43, KeyCode.C },
            { 0x44, KeyCode.D },
            { 0x45, KeyCode.E },
            { 0x46, KeyCode.F },
            { 0x47, KeyCode.G },
            { 0x48, KeyCode.H },
            { 0x49, KeyCode.I },
            { 0x4A, KeyCode.J },
            { 0x4B, KeyCode.K },
            { 0x4C, KeyCode.L },
            { 0x4D, KeyCode.M },
            { 0x4E, KeyCode.N },
            { 0x4F, KeyCode.O },
            { 0x50, KeyCode.P },
            { 0x51, KeyCode.Q },
            { 0x52, KeyCode.R },
            { 0x53, KeyCode.S },
            { 0x54, KeyCode.T },
            { 0x55, KeyCode.U },
            { 0x56, KeyCode.V },
            { 0x57, KeyCode.W },
            { 0x58, KeyCode.X },
            { 0x59, KeyCode.Y },
            { 0x5A, KeyCode.Z },
            { 0x2E, KeyCode.Delete },
            { 0x26, KeyCode.UpArrow },
            { 0x28, KeyCode.DownArrow },
            { 0x27, KeyCode.RightArrow },
            { 0x25, KeyCode.LeftArrow },
            { 0x2D, KeyCode.Insert },
            { 0x24, KeyCode.Home },
            { 0x23, KeyCode.End },
            { 0x21, KeyCode.PageUp },
            { 0x22, KeyCode.PageDown },
            { 0x70, KeyCode.F1 },
            { 0x71, KeyCode.F2 },
            { 0x72, KeyCode.F3 },
            { 0x73, KeyCode.F4 },
            { 0x74, KeyCode.F5 },
            { 0x75, KeyCode.F6 },
            { 0x76, KeyCode.F7 },
            { 0x77, KeyCode.F8 },
            { 0x78, KeyCode.F9 },
            { 0x79, KeyCode.F10 },
            { 0x7A, KeyCode.F11 },
            { 0x7B, KeyCode.F12 },
            { 0x7C, KeyCode.F13 },
            { 0x7D, KeyCode.F14 },
            { 0x7E, KeyCode.F15 },
            { 0x90, KeyCode.Numlock },
            { 0x14, KeyCode.CapsLock },
            { 0x91, KeyCode.ScrollLock },
            { 0xA0, KeyCode.LeftShift },
            { 0xA1, KeyCode.RightShift },
            { 0xA2, KeyCode.LeftControl },
            { 0xA3, KeyCode.RightControl },
            { 0x5B, KeyCode.LeftWindows },
            { 0x5C, KeyCode.RightWindows },
            { 0x2A, KeyCode.Print },
            { 0x03, KeyCode.Break },
            { 0x01, KeyCode.Mouse0 },
            { 0x02, KeyCode.Mouse1 },
            { 0x04, KeyCode.Mouse2 },
            { 0x05, KeyCode.Mouse3 },
            { 0x06, KeyCode.Mouse4 },
            { 0xBF, KeyCode.Slash },
            { 0xDE, KeyCode.Quote },
            { 0xC0, KeyCode.BackQuote },            
            { 0xA4, KeyCode.LeftAlt },
            { 0xA5, KeyCode.RightAlt },
            { 0xDC, KeyCode.Backslash },
            { 0xDF, KeyCode.Exclaim },
            { 0x6A, KeyCode.Asterisk },
            { 0xBA, KeyCode.Semicolon },
            { 0xDB, KeyCode.LeftBracket},
            { 0xDD, KeyCode.RightBracket}
        };

        readonly internal static Dictionary<KeyCode, int> KeyCodeToVKCode = VKCodeToKeyCode.ToDictionary((i) => i.Value, (i) => i.Key);
        #endregion

        static IntPtr keyboardHook = IntPtr.Zero, mouseHook = IntPtr.Zero;
        static HashSet<KeyCode> down, up, hold;
        static HashSet<int> mouseDown, mouseUp, mouseHold;
        static Dictionary<KeyCode, List<Action>> onKeyDown, onKeyUp;
        static Dictionary<int, List<Action>> onMouseDown, onMouseUp;
        static List<Action> onMouseMove;

        #region Hooking
        internal static void Initialize(Core core)
        {
            down    = new HashSet<KeyCode>();
            up      = new HashSet<KeyCode>();
            hold = new HashSet<KeyCode>();

            onKeyDown = new Dictionary<KeyCode, List<Action>>();
            onKeyUp   = new Dictionary<KeyCode, List<Action>>();

            mouseDown    = new HashSet<int>();
            mouseUp      = new HashSet<int>();
            mouseHold = new HashSet<int>();

            onMouseMove = new List<Action>();
            onMouseDown = new Dictionary<int, List<Action>>();
            onMouseUp   = new Dictionary<int, List<Action>>();

            keyboardHook = CreateKeyboardHooks();
            mouseHook = CreateMouseHooks();
            
            core.SubscribeModule(Update);
        }

        internal static void Disable()
        {
            UnhookWindowsHookEx(keyboardHook);
            UnhookWindowsHookEx(mouseHook);
        }

        static IntPtr CreateKeyboardHooks()
        {
            using(Process process = Process.GetCurrentProcess())
            using(ProcessModule module = process.MainModule)
            return SetWindowsHookEx(
                WH_KEYBOARD_LL,
                KeyboardCallback,
                GetModuleHandle(module.ModuleName),
                0
            );
        }

        static IntPtr CreateMouseHooks()
        {
            using(Process process = Process.GetCurrentProcess())
            using(ProcessModule module = process.MainModule)
            return SetWindowsHookEx(
                WH_MOUSE_LL,
                MouseCallback,
                GetModuleHandle(module.ModuleName),
                0
            );
        }
        #endregion

        #region Update
        static void Update()
        {
            foreach(var key in down)
                hold.Add(key);
            
            down.Clear();
            up.Clear();

            foreach(var button in mouseDown)
                mouseHold.Add(button);
            
            mouseDown.Clear();
            mouseUp.Clear();
        }

        static IntPtr KeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int code = Marshal.ReadInt32(lParam);

                if(VKCodeToKeyCode.ContainsKey(code))
                {
                    KeyCode keyCode = VKCodeToKeyCode[Marshal.ReadInt32(lParam)];
                    
                    if(wParam == (IntPtr) WM_KEYDOWN)
                    {
                        if(down.Contains(keyCode))
                            KeyCallback(keyCode, hold);
                        else if(!hold.Contains(keyCode))
                            KeyCallback(keyCode, down, onKeyDown);
                    }
                    else if(wParam == (IntPtr) WM_KEYUP)
                    {
                        KeyCallback(keyCode, up, onKeyUp);
                        hold.Remove(keyCode);
                    }
                }
                else UnityEngine.Debug.LogError("[Desktopia/Inputs] VKCode " + code + " doesn't exist in database, try adding it!");
            }
            
            return CallNextHookEx(keyboardHook, nCode, wParam, lParam);
        }

        static IntPtr MouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int code = (int) wParam;
                if(MouseCodesToButtons.ContainsKey(code))
                {   
                    int button = MouseCodesToButtons[code].Item1;
                    bool down  = MouseCodesToButtons[code].Item2;

                    if(down)
                    {
                        if(mouseDown.Contains(button))
                            MouseButtonCallback(button, mouseHold);
                        else if(!mouseHold.Contains(button))
                            MouseButtonCallback(button, mouseDown, onMouseDown); 
                    }
                    else
                    {
                        MouseButtonCallback(button, mouseUp, onMouseUp);
                        mouseHold.Remove(button);
                    }
                }
            }

            return CallNextHookEx(mouseHook, nCode, wParam, lParam);
        }
        #endregion

        #region Helpers
        static void KeyCallback(KeyCode keyCode, HashSet<KeyCode> set, Dictionary<KeyCode, List<Action>> callbacks = null)
        {
            set.Add(keyCode);

            if(callbacks?.Count > 0 && callbacks.ContainsKey(keyCode))
                foreach(var c in callbacks[keyCode])
                    c.Invoke();
        }

        static void MouseButtonCallback(int button, HashSet<int> set, Dictionary<int, List<Action>> callbacks = null)
        {
            set.Add(button);

            if(callbacks?.Count > 0 && callbacks.ContainsKey(button))
                foreach(var c in callbacks[button])
                    c.Invoke();
        }

        static void AddCallback(KeyCode keyCode, Action callback, Dictionary<KeyCode, List<Action>> dictionary)
        {
            if(dictionary.Count == 0 || !dictionary.ContainsKey(keyCode))
                dictionary.Add(keyCode, new List<Action>());
            dictionary[keyCode].Add(callback);
        }
        
        static void RemoveCallback(KeyCode keyCode, Action callback, Dictionary<KeyCode, List<Action>> dictionary)
        {
            if(dictionary.Count > 0 && dictionary.ContainsKey(keyCode))
                dictionary[keyCode].Remove(callback);
        }
        #endregion

        #region Publics
        #region Keyboard
        /// <summary>
        /// Returns true while the key is hold.
        /// </summary>
        public static bool GetKey(KeyCode keyCode)
        {
            return hold.Contains(keyCode);
        }

        /// <summary>
        /// Returns true the first frame the key is pressed.
        /// </summary>
        public static bool GetKeyDown(KeyCode keyCode)
        {
            return down.Contains(keyCode);
        }

        /// <summary>
        /// Returns true the during frame the key is released.
        /// </summary>
        public static bool GetKeyUp(KeyCode keyCode)
        {
            return up.Contains(keyCode);
        }

        /// <summary>
        /// Returns true while any key is hold.
        /// </summary>
        public static bool GetAnyKey()
        {
            return hold.Count > 0;
        }

        /// <summary>
        /// Returns true if any key was pressed during the frame.
        /// </summary>
        public static bool GetAnyKeyDown()
        {
            return down.Count > 0;
        }

        /// <summary>
        /// Returns true if any key was released during the frame.
        /// </summary>
        public static bool GetAnyKeyUp()
        {
            return up.Count > 0;
        }

        /// <summary>
        /// Returns all the keys that were pressed during the frame.
        /// </summary>
        public static HashSet<KeyCode> PressedKeys
        {
            get { return down; }
        }

        /// <summary>
        /// Returns all the keys that were hold during the frame.
        /// </summary>
        public static HashSet<KeyCode> HoldKeys
        {
            get { return hold; }
        }

        /// <summary>
        /// Returns all the keys that were released during the frame.
        /// </summary>
        public static HashSet<KeyCode> ReleasedKeys
        {
            get { return down; }
        }

        /// <summary>
        /// Adds a function that is called the first time the key is pressed.
        /// </summary>
        public static Action AddOnKeyDown(KeyCode keyCode, Action callback)
        {
            AddCallback(keyCode, callback, onKeyDown);
            return callback;
        }

        /// <summary>
        /// Adds a function that is called when the key is released.
        /// </summary>
        public static Action AddOnKeyUp(KeyCode keyCode, Action callback)
        {
            AddCallback(keyCode, callback, onKeyUp);
            return callback;
        }

        /// <summary>
        /// Removes a key down callback for a given key.
        /// </summary>
        public static void RemoveOnKeyDown(KeyCode keyCode, Action callback)
        {
            RemoveCallback(keyCode, callback, onKeyDown);
        }

        /// <summary>
        /// Removes a key up callback for a given key.
        /// </summary>
        public static void RemoveOnKeyUp(KeyCode keyCode, Action callback)
        {
            RemoveCallback(keyCode, callback, onKeyUp);
        }
        #endregion
        #region Mouse
        /// <summary>
        /// Returns true while the mouse button is hold.
        /// </summary>
        public static bool GetMouseButton(int button)
        {
            return mouseHold.Contains(button);
        }

        /// <summary>
        /// Returns true the first frame the mouse button is pressed.
        /// </summary>
        public static bool GetMouseButtonDown(int button)
        {
            return mouseDown.Contains(button);
        }

        /// <summary>
        /// Returns true the during frame the mouse button is released.
        /// </summary>
        public static bool GetMouseButtonUp(int button)
        {
            return mouseUp.Contains(button);
        }

        /// <summary>
        /// Adds a function that is called when the mouse moves.
        /// </summary>
        public static Action AddOnMouseMove(Action callback)
        {
            onMouseMove.Add(callback);
            return callback;
        }

        /// <summary>
        /// Removes a mouse move callback.
        /// </summary>
        public static void RemoveOnMouseMove(Action callback)
        {
            onMouseMove.Remove(callback);
        }

        /// <summary>
        /// Adds a function that is called the first time a given mouse button is pressed.
        /// </summary>
        public static Action AddOnMouseDown(int button, Action callback)
        {
            if(!onMouseDown.ContainsKey(button)) onMouseDown.Add(button, new List<Action>());
            onMouseDown[button].Add(callback);
            return callback;
        }

        /// <summary>
        /// Removes a mouse down callback.
        /// </summary>
        public static void RemoveOnMouseDown(int button, Action callback)
        {
            if(onMouseDown.Count > 0 && onMouseDown.ContainsKey(button))
                onMouseDown[button].Remove(callback);
        }

        /// <summary>
        /// Adds a function that is called when a given mouse button is released.
        /// </summary>
        public static Action AddOnMouseUp(int button, Action callback)
        {
            if(!onMouseUp.ContainsKey(button)) onMouseUp.Add(button, new List<Action>());
            onMouseUp[button].Add(callback);
            return callback;
        }

        /// <summary>
        /// Removes a mouse up callback.
        /// </summary>
        public static void RemoveOnMouseUp(int button, Action callback)
        {
            if(onMouseUp.Count > 0 && onMouseUp.ContainsKey(button))
                onMouseUp[button].Remove(callback);
        }
        #endregion
        #endregion
    }
}
