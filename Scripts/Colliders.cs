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
using System.Collections.Generic;
using System.Linq;

namespace Desktopia
{
    public static class Colliders
    {
        static Dictionary<Window, Transform> dictionary;
        static Vector2 screenToWorld;
        static Transform prefab;

        internal static void Initialize(Core core, Transform prefab)
        {
            Colliders.prefab = prefab;
            core.SubscribeModule(Update);
            Windows.AddOnWindowOpened(AddCollider);
            Windows.AddOnWindowClosed(RemoveCollider);

            dictionary = new Dictionary<Window, Transform>();
            ComputeScreenToWorld();
        }

        static void AddCollider(Window window)
        {
            var c = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            c.name = window.Title;
            dictionary.Add(window, c);
        }

        static void RemoveCollider(Window window)
        {
            var transform = dictionary[window];
            dictionary.Remove(window);
            GameObject.Destroy(transform.gameObject);
        }

        static void Update()
        {
            foreach(var p in dictionary)
            {
                var window = p.Key;
                var collider = p.Value;
                p.Value.transform.position = ScreenToWorldPosition(window.Rect.position);

                if(Mathf.Abs(window.Rect.size.x) >= Screen.width || window.Rect.size.x == 0)
                     collider.localScale = Vector3.zero;
                else collider.localScale = ScreenToWorldSize(window.Rect.size);
            }
        }

        #region Helpers
        /// <summary>
        /// Transforms a screen position in pixel to a world position.
        /// </summary>
        public static Vector2 ScreenToWorldPosition(Vector2 screen)
        {
            return new Vector2(
                Camera.main.transform.position.x + screen.x / Screen.width * screenToWorld.x - screenToWorld.x * .5f,
                Camera.main.transform.position.y + (1 - (screen.y / Screen.height)) * screenToWorld.y - screenToWorld.y * .5f
            );
        }

        /// <summary>
        /// Transforms a screen size in pixel to a world size.
        /// </summary>
        public static Vector3 ScreenToWorldSize(Vector2 screen)
        {
            return new Vector3(screen.x / Screen.width * screenToWorld.x, screen.y / Screen.height * screenToWorld.y, 1);
        }

        static void ComputeScreenToWorld()
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            Camera.main.transform.position = Vector3.zero;
            screenToWorld = -Camera.main.ScreenToWorldPoint(new Vector3(1,1)) * 2;
            Camera.main.transform.position = cameraPosition;
        }
        #endregion

        #region Publics
        /// <summary>
        /// Returns the windows collider list.
        /// </summary>
        public static Dictionary<Window, Transform>.ValueCollection List { get { return dictionary.Values; } }

        /// <summary>
        /// Forces to recalculate the screen to world ratio.
        /// </summary>
        public static void RecalculateScreenToWorld()
        {
            ComputeScreenToWorld();
        }
        #endregion
    }
}
