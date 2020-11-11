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

using System.Runtime.InteropServices;
using UnityEngine;

namespace Desktopia
{
    public static class Cursor
    {
        #region Windows API
        struct Point { public int x, y; }

        [DllImport("User32.Dll")]
        static extern long SetCursorPos(int x, int y);

        [DllImport("User32.Dll")]
        static extern long GetCursorPos(out Point point);
        #endregion

        static Vector2 position;

        internal static void Initialize(Core core)
        {
            core.SubscribeModule(Update);
            GetPoint();
        }

        static void Update()
        {
            GetPoint();
        }

        static void GetPoint()
        {
            Point point;
            GetCursorPos(out point);
            position.x = point.x;
            position.y = point.y;
        }

        #region Publics
        /// <summary>
        /// Sets the cursor position, in pixel.
        /// </summary>
        public static void SetPosition(int x, int y)
        {
            SetCursorPos(x,y);
        }

        /// <summary>
        /// Sets the cursor position, in pixel.
        /// </summary>
        public static void SetPosition(Vector2 position)
        {
            SetCursorPos((int) position.x, (int) position.y);
        }

        /// <summary>
        /// Moves the cursor of a given distance, in pixel.
        /// </summary>
        public static void Move(int x, int y)
        {
            SetCursorPos((int) position.x + x, (int) position.y + y);
        }
        
        /// <summary>
        /// Moves the cursor of a given distance, in pixel.
        /// </summary>
        public static void Move(Vector2 position)
        {
            SetCursorPos((int) (position.x + Position.x), (int) (position.y + Position.y));
        }

        /// <summary>
        /// The position of the cursor, in pixel.
        /// </summary>
        public static Vector2 Position { get { return position; } }
        #endregion
    }
}