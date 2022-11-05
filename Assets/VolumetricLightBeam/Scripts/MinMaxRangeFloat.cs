using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB
{
    [Serializable]
    public struct MinMaxRangeFloat
    {
        public float minValue { get { return m_MinValue; } }
        public float maxValue { get { return m_MaxValue; } }

        public float randomValue { get { return UnityEngine.Random.Range(minValue, maxValue); } }
        public Vector2 asVector2 { get { return new Vector2(minValue, maxValue); } }

        public float GetLerpedValue(float lerp01) { return Mathf.Lerp(minValue, maxValue, lerp01); }

        public MinMaxRangeFloat(float min, float max) { m_MinValue = min; m_MaxValue = max; Debug.Assert(min <= max); }

        [SerializeField] float m_MinValue;
        [SerializeField] float m_MaxValue;
    }

    public class MinMaxRangeAttribute : System.Attribute
    {
        public float minValue { get; private set; }
        public float maxValue { get; private set; }

        public MinMaxRangeAttribute(float min, float max) { minValue = min; maxValue = max; Debug.Assert(min <= max); }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MinMaxRangeFloat), true)]
    public class MinMaxRangeFloatPropertyDrawer : PropertyDrawer
    {
        static float RoundFloat(float f) { return (float)Math.Round(f * 100f) / 100f; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, label);

                var propMin = property.FindPropertyRelative("m_MinValue");
                Debug.AssertFormat(propMin != null, "Failed to find property 'MinMaxRangeFloat.m_MinValue'");
                var propMax = property.FindPropertyRelative("m_MaxValue");
                Debug.AssertFormat(propMax != null, "Failed to find property 'MinMaxRangeFloat.m_MaxValue'");

                float valueMin = RoundFloat(propMin.floatValue);
                float valueMax = RoundFloat(propMax.floatValue);

                float rangeMin = 0.0f, rangeMax = 1.0f;

                var ranges = (MinMaxRangeAttribute[])fieldInfo.GetCustomAttributes(typeof(MinMaxRangeAttribute), true);
                if (ranges.Length > 0)
                {
                    rangeMin = ranges[0].minValue;
                    rangeMax = ranges[0].maxValue;
                }

                const float kBoundsFieldWidth = 40.0f;
                const float kWidthOffset = 5.0f;

                EditorGUI.showMixedValue = propMin.hasMultipleDifferentValues || propMax.hasMultipleDifferentValues;
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        var rectMinValue = new Rect(position);
                        rectMinValue.width = kBoundsFieldWidth;
                        valueMin = EditorGUI.FloatField(rectMinValue, valueMin);

                        position.xMin += kBoundsFieldWidth + kWidthOffset;

                        var reactMaxValue = new Rect(position);
                        reactMaxValue.xMin = reactMaxValue.xMax - kBoundsFieldWidth;
                        valueMax = EditorGUI.FloatField(reactMaxValue, valueMax);

                        position.xMax -= kBoundsFieldWidth + kWidthOffset;

                        EditorGUI.MinMaxSlider(position, ref valueMin, ref valueMax, rangeMin, rangeMax);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        propMin.floatValue = valueMin;
                        propMax.floatValue = valueMax;
                    }
                }
                EditorGUI.showMixedValue = false;
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}
