using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DifficultyInt))]
[CustomPropertyDrawer(typeof(DifficultyFloat))]
public sealed class DifficultyValueDrawer : PropertyDrawer
{
    private static readonly string[] FieldNames =
    {
        "normal",
        "hard",
        "lunatic"
    };

    private static readonly GUIContent[] ShortLabels =
    {
        new("N", "Normal"),
        new("H", "Hard"),
        new("L", "Lunatic")
    };

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    )
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect contentRect = EditorGUI.PrefixLabel(
            position,
            GUIUtility.GetControlID(FocusType.Passive),
            label
        );

        int previousIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        const float spacing = 4f;
        const float subLabelWidth = 13f;

        float columnWidth =
            (contentRect.width - spacing * (FieldNames.Length - 1)) /
            FieldNames.Length;

        for (int i = 0; i < FieldNames.Length; i++)
        {
            float x = contentRect.x + i * (columnWidth + spacing);

            Rect labelRect = new(
                x,
                contentRect.y,
                subLabelWidth,
                contentRect.height
            );

            Rect fieldRect = new(
                x + subLabelWidth,
                contentRect.y,
                Mathf.Max(10f, columnWidth - subLabelWidth),
                contentRect.height
            );

            EditorGUI.LabelField(labelRect, ShortLabels[i]);

            SerializedProperty subProperty =
                property.FindPropertyRelative(FieldNames[i]);

            EditorGUI.PropertyField(
                fieldRect,
                subProperty,
                GUIContent.none
            );
        }

        EditorGUI.indentLevel = previousIndent;
        EditorGUI.EndProperty();
    }
}