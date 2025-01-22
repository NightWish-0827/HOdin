using UnityEditor;
using UnityEngine;

public partial class GameComponentEditor
{
    private void DrawProperties(float fullWidth)
    {
        serializedObject.Update();
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        SerializedProperty prevProperty = null;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (iterator.name.Equals("m_Script")) continue;

            DrawProperty(iterator, prevProperty, fullWidth);
            prevProperty = iterator.Copy();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawProperty(SerializedProperty property, SerializedProperty prevProperty, float fullWidth)
    {
        string headerText;
        bool hasHeader = HasHeaderAttribute(property, out headerText);

        if (customHeaderTexts.TryGetValue(property.name, out string customText))
        {
            headerText = customText;
            hasHeader = true;
        }

        if (hasHeader || (ShouldDrawHeader(property) && (prevProperty == null || !IsSpaceAttribute(prevProperty))))
        {
            DrawPropertyWithHeader(property, headerText, hasHeader, fullWidth);
        }
        else
        {
            EditorGUILayout.PropertyField(property, true);
        }
    }

    private void DrawPropertyWithHeader(SerializedProperty property, string headerText, bool hasHeader, float fullWidth)
    { // 확장 가능한 필드인 경우 추가 여백 계산
        bool isExpandable = property.isArray || property.hasVisibleChildren;
        float extraSpace = isExpandable && property.isExpanded ? GetExpandedPropertyHeight(property) : 0f;

        Rect headerRect = EditorGUILayout.GetControlRect(false, 22);
        headerRect.x = 0;
        headerRect.width = fullWidth;

        Color propertyColor = new Color(headerColor.r, headerColor.g, headerColor.b, 0.5f);
        EditorGUI.DrawRect(headerRect, propertyColor);

        string displayText = hasHeader ? headerText : GetUpperCase(property.displayName);
        EditorGUI.LabelField(headerRect, displayText, GetHeaderStyle("PropertyHeader"));

        HandlePropertyContextMenu(headerRect, property, displayText);
        DrawPropertyControl(property, fullWidth);

        // 확장된 상태일 때 추가 여백 적용
        if (extraSpace > 0)
        {
            EditorGUILayout.Space(extraSpace);
        }
    }

    private void DrawPropertyControl(SerializedProperty property, float fullWidth)
    {
        Rect controlRect = EditorGUILayout.GetControlRect(true);
        float originalLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 0;

        float padding = fullWidth * 0.1f;
        controlRect.x = padding;
        controlRect.width = fullWidth - (padding * 2);

        EditorGUI.PropertyField(controlRect, property, GUIContent.none, true);
        EditorGUIUtility.labelWidth = originalLabelWidth;
    }

    private float GetExpandedPropertyHeight(SerializedProperty property)
    {
        float height = 0;

        if (property.isArray)
        {
            // 배열/리스트의 경우
            int arraySize = property.arraySize;
            height += EditorGUI.GetPropertyHeight(property, true) - 20f; // 기본 헤더 높이 제외
        }
        else if (property.hasVisibleChildren)
        {
            // 중첩된 프로퍼티의 경우
            var iterator = property.Copy();
            var endProperty = property.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                height += EditorGUI.GetPropertyHeight(iterator, true) + 2f;
                enterChildren = false;
            }
        }

        return height + 10f; // 추가 여백}
    }

    private void DrawMainHeader(float fullWidth)
    {
        Rect headerRect = EditorGUILayout.GetControlRect(false, 30);
        headerRect.x = 0;
        headerRect.width = fullWidth;

        EditorGUI.DrawRect(headerRect, headerColor);
        string className = GetUpperCase(target.GetType().Name);
        EditorGUI.LabelField(headerRect, $"<size=12>{className}</size>", GetHeaderStyle("MainHeader"));

        HandleMainHeaderContextMenu(headerRect);
    }
}