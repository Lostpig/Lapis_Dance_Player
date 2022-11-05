#if UNITY_EDITOR
#if UNITY_2019_1_OR_NEWER
#define UI_USE_FOLDOUT_HEADER_2019
#endif

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace VLB
{
    public class EditorCommon : Editor
    {
        protected virtual void OnEnable()
        {
            FoldableHeader.OnEnable();
            RetrieveSerializedProperties("_");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
    #if UNITY_2019_3_OR_NEWER
            // no vertical space in 2019.3 looks better
    #else
            EditorGUILayout.Separator();
    #endif
        }

        public static void DrawLineSeparator()
        {
            DrawLineSeparator(Color.grey, 1, 10);
        }

        static void DrawLineSeparator(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));

            r.x = 0;
            r.width = EditorGUIUtility.currentViewWidth;

            r.y += padding / 2;
            r.height = thickness;

            EditorGUI.DrawRect(r, color);
        }

        protected void ButtonOpenConfig(bool miniButton = true)
        {
            bool buttonClicked = false;
            if (miniButton) buttonClicked = GUILayout.Button(EditorStrings.Common.ButtonOpenGlobalConfig, EditorStyles.miniButton);
            else            buttonClicked = GUILayout.Button(EditorStrings.Common.ButtonOpenGlobalConfig);

            if (buttonClicked)
                Config.EditorSelectInstance();
        }

        protected bool GlobalToggleButton(ref bool boolean, GUIContent content, string saveString, float maxWidth = 999.0f)
        {
            EditorGUI.BeginChangeCheck();
            boolean = GUILayout.Toggle(boolean, content, EditorStyles.miniButton, GUILayout.MaxWidth(maxWidth));
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(saveString, boolean);
                SceneView.RepaintAll();
                return true;
            }
            return false;
        }

        // SERIALIZED PROPERTY RETRIEVAL
        string GetThisNamespaceAsString() { return GetType().Namespace; }

        SerializedProperty FindProperty(string prefix, string name)
        {
            Debug.Assert(serializedObject != null);
            var prop = serializedObject.FindProperty(name);
            if (prop == null)
            {
                name = string.Format("{0}{1}{2}", prefix, char.ToUpperInvariant(name[0]), name.Substring(1)); // get '_Name' from 'name'
                prop = serializedObject.FindProperty(name);
            }
            return prop;
        }

        void RetrieveSerializedProperties(string prefix, Type t)
        {
            if (t == null) return;
            if (t.Namespace != GetThisNamespaceAsString()) return;

            var allEditorFields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var field in allEditorFields)
            {
                if (field.FieldType == typeof(SerializedProperty))
                {
                    var runtimeFieldName = field.Name;
                    var serializedProp = FindProperty(prefix, runtimeFieldName);
                    Debug.AssertFormat(serializedProp != null, "Fail to find serialized field '{0}' in object {1}", runtimeFieldName, serializedObject.targetObject);
                    field.SetValue(this, serializedProp);
                }
            }

            RetrieveSerializedProperties(prefix, t.BaseType);
        }

        protected void RetrieveSerializedProperties(string prefix)
        {
            RetrieveSerializedProperties(prefix, GetType());
        }

        public abstract class EditorGUIWidth : System.IDisposable
        {
            protected abstract void ApplyWidth(float width);
            public EditorGUIWidth(float width) { ApplyWidth(width); }
            public void Dispose() { ApplyWidth(0.0f); }
        }

        public class LabelWidth : EditorGUIWidth
        {
            public LabelWidth(float width) : base(width) { }
            protected override void ApplyWidth(float width) { EditorGUIUtility.labelWidth = width; }
        }

        public class FieldWidth : EditorGUIWidth
        {
            public FieldWidth(float width) : base(width) { }
            protected override void ApplyWidth(float width) { EditorGUIUtility.fieldWidth = width; }
        }

        private static ArcHandle ms_HandleRadius = null;

        // HANDLES
        protected static ArcHandle handleRadius
        {
            get
            {
                if (ms_HandleRadius == null)
                {
                    ms_HandleRadius = new ArcHandle();
                    ms_HandleRadius.SetColorWithRadiusHandle(Color.white, 0f);
                    ms_HandleRadius.angle = 360f;
                    ms_HandleRadius.angleHandleSizeFunction = null;
                    ms_HandleRadius.angleHandleColor = Color.clear;
                }
                return ms_HandleRadius;
            }
        }
    }
}
#endif // UNITY_EDITOR

