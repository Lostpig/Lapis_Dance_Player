#if UNITY_EDITOR
#if UNITY_2019_1_OR_NEWER
#define UI_USE_FOLDOUT_HEADER_2019
#endif

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;

namespace VLB
{
    public class EditorCommon : Editor
    {
        protected virtual void OnEnable()
        {
            FoldableHeader.OnEnable();
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

        protected SerializedProperty FindProperty<T, TValue>(Expression<Func<T, TValue>> expr)
        {
            Debug.Assert(serializedObject != null);
            var prop = serializedObject.FindProperty(ReflectionUtils.GetFieldPath(expr));
            Debug.Assert(prop != null, string.Format("Failed to find SerializedProperty for expression '{0}'", expr));
            return prop;
        }

        protected void ButtonOpenConfig(bool miniButton = true)
        {
            bool buttonClicked = false;
            if (miniButton) buttonClicked = GUILayout.Button(EditorStrings.Common.ButtonOpenGlobalConfig, EditorStyles.miniButton);
            else            buttonClicked = GUILayout.Button(EditorStrings.Common.ButtonOpenGlobalConfig);

            if (buttonClicked)
                Config.EditorSelectInstance();
        }

        protected bool GlobalToggle(ref bool boolean, GUIContent content, string saveString)
        {
            EditorGUI.BeginChangeCheck();
            boolean = EditorGUILayout.Toggle(content, boolean);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(saveString, boolean);
                SceneView.RepaintAll();
                return true;
            }
            return false;
        }
    }
}
#endif // UNITY_EDITOR

