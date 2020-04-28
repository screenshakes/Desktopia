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
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Desktopia
{
    [AddComponentMenu("Desktopia")]
    public sealed class Core : MonoBehaviour
    {
        const string version = "0.1";
        HashSet<Action> modulesUpdate;
        Vector2 lastResolution;

        #region Modules
        [SerializeField] bool inputs;
        [SerializeField] bool cursor;
        [SerializeField] bool windows;
        [SerializeField] bool colliders;
        [SerializeField] Transform collider;
        [SerializeField] bool dragDrop;
        #endregion

        #region Main Window
        [SerializeField] bool transparent;
        [SerializeField] bool topMost;
        [SerializeField] bool clickThrough;
        #endregion

        void OnEnable()
        {
            PlayerPrefs.SetString("Desktopia/Version", version);

            modulesUpdate = new HashSet<Action>();

            Windows.GetMain();            
            
            if(inputs)    Inputs.Initialize(this);
            if(cursor)    Cursor.Initialize(this);
            if(windows)   Windows.Initialize(this);
            if(colliders) Colliders.Initialize(this, collider);
            if(dragDrop)  Files.DragDrop.Enable();
            
            Windows.Main.SetTransparent(transparent);
            Windows.Main.SetTopMost(topMost);
            Windows.Main.SetClickThrough(clickThrough);
        }

        void Update()
        {
            foreach(var u in modulesUpdate)
                u.Invoke();
        }

        void OnDisable()
        {
            if(inputs) Inputs.Disable();
            if(dragDrop) Files.DragDrop.Disable();
        }

        void OnApplicationQuit()
        {
            OnDisable();
        }

        internal void SubscribeModule(Action update)
        {
            modulesUpdate.Add(update);
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(Core))]
    class Inspector : Editor
    {
        static Texture2D desktopia;
        static GUIStyle title, header, box;

        static bool modules, main;

        static SerializedProperty inputs, cursor, windows, colliders, collider, dragDrop;
        static SerializedProperty transparent, clickThrough, topMost;

        void OnEnable()
        {
            inputs    = serializedObject.FindProperty("inputs");
            cursor    = serializedObject.FindProperty("cursor");
            windows   = serializedObject.FindProperty("windows");
            colliders = serializedObject.FindProperty("colliders");
            collider  = serializedObject.FindProperty("collider");
            dragDrop  = serializedObject.FindProperty("dragDrop");

            transparent  = serializedObject.FindProperty("transparent");
            clickThrough = serializedObject.FindProperty("clickThrough");
            topMost      = serializedObject.FindProperty("topMost");

            header = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };

            box = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(20, 10, 10, 10),
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            title = new GUIStyle()
            {
                fixedHeight = 200,
                alignment = TextAnchor.MiddleCenter
            };

            string path = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("desktopia t:texture2D", null)[0]);
            desktopia = (Texture2D) AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label(new GUIContent(desktopia), title);

            DrawMainWindow();
            DrawModules();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawModules()
        {
            EditorGUILayout.BeginVertical(box);
            modules = EditorGUILayout.Foldout(modules, "Modules", header);

            if(modules)
            {
                EditorGUILayout.PropertyField(inputs,    new GUIContent("Inputs"));
                EditorGUILayout.PropertyField(cursor,    new GUIContent("Cursor"));
                EditorGUILayout.PropertyField(windows,   new GUIContent("Windows"));

                if(windows.boolValue)
                {
                    EditorGUILayout.PropertyField(colliders, new GUIContent("Colliders"));
                    if(colliders.boolValue)
                    {
                        ++EditorGUI.indentLevel;
                        EditorGUILayout.PropertyField(collider, new GUIContent("Prefab"));
                        --EditorGUI.indentLevel;
                    }
                }
                
                EditorGUILayout.PropertyField(dragDrop,  new GUIContent("Drag & Drop files"));
            }
            EditorGUILayout.EndVertical();
        }

        void DrawMainWindow()
        {
            EditorGUILayout.BeginVertical(box);
            main = EditorGUILayout.Foldout(main, "Main Window", header);

            if(main)
            {
                EditorGUILayout.PropertyField(transparent,  new GUIContent("Transparent"));
                EditorGUILayout.PropertyField(clickThrough, new GUIContent("Click Through"));
                EditorGUILayout.PropertyField(topMost,      new GUIContent("Top Most"));
            }
            EditorGUILayout.EndVertical();
        }
    }
    #endif
}