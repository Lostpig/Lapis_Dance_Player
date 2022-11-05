using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB
{
    /// <summary>
    /// Highlight in red in inspector in not set
    /// </summary>
    public sealed class HighlightNullAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HighlightNullAttribute))]
    public class HighlightNullDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label.text, "Only valid for object references");
                return;
            }

            if (property.objectReferenceValue == null)
                EditorGUI.DrawRect(position, Color.red);

            EditorGUI.ObjectField(position, property, label);
        }
    }
#endif
}

