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

            DrawProperty(iterator.Copy(), prevProperty, fullWidth);
            prevProperty = iterator.Copy();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawProperty(SerializedProperty property, SerializedProperty prevProperty, float fullWidth)
    {
        string headerText = string.Empty;
        bool hasUnityHeader = HasHeaderAttribute(property, out string unityHeaderText);
        bool hasCustomHeader = customHeaderTexts.TryGetValue(property.name, out string customText);

        // 커스텀 헤더 또는 자동 헤더 처리
        if (hasCustomHeader || (ShouldDrawHeader(property) && (prevProperty == null || !IsSpaceAttribute(prevProperty))))
        {
            headerText = hasCustomHeader ? customText : GetCachedUpperCase(property.displayName);
            DrawPropertyWithHeader(property, headerText, true, fullWidth);
        }
        else
        {
            // Unity Header가 있는 경우 기본 PropertyField 사용
            EditorGUILayout.PropertyField(property, true);
        }
    }

    private void DrawPropertyWithHeader(SerializedProperty property, string headerText, bool hasHeader, float fullWidth)
    {
        bool isExpandable = property.isArray || property.hasVisibleChildren;
        float extraSpace = 0f;

        // 배열이 펼쳐져 있을 때의 동적 높이 계산
        if (isExpandable && property.isExpanded)
        {
            if (property.isArray)
            {
                // 배열 헤더 높이
                extraSpace += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                
                // 각 배열 요소의 높이 계산
                for (int i = 0; i < property.arraySize; i++)
                {
                    var element = property.GetArrayElementAtIndex(i);
                    extraSpace += EditorGUI.GetPropertyHeight(element, true) + EditorGUIUtility.standardVerticalSpacing;
                }
                
                // 배열 컨트롤 버튼 영역 높이
                extraSpace += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else if (property.hasVisibleChildren)
            {
                extraSpace = GetExpandedPropertyHeight(property);
            }
        }

        // 헤더 그리기
        EditorGUILayout.Space(2);
        float headerHeight = 24; // 헤더 높이 약간 증가
        Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight);
        headerRect.x = 4;
        headerRect.width = fullWidth - 8;

        // 헤더 배경에 그라데이션 효과 추가
        Color baseColor = new Color(headerColor.r, headerColor.g, headerColor.b, 0.4f);
        Color shadowColor = new Color(0, 0, 0, 0.1f);
        EditorGUI.DrawRect(headerRect, baseColor);
        
        // 하단 테두리 효과
        Rect borderRect = headerRect;
        borderRect.y = headerRect.y + headerRect.height - 1;
        borderRect.height = 1;
        EditorGUI.DrawRect(borderRect, shadowColor);

        // 헤더 텍스트
        headerRect.x += 10;
        headerRect.width -= 20;
        var style = GetHeaderStyle("PropertyHeader");
        style.alignment = TextAnchor.MiddleLeft;
        style.fontSize = 11;
        EditorGUI.LabelField(headerRect, $"<color=#E0E0E0>{headerText}</color>", style);

        HandlePropertyContextMenu(headerRect, property, headerText);
        EditorGUILayout.Space(2);

        // 프로퍼티 필드
        Rect propertyRect = EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(property, true));
        propertyRect.x += 8; // 들여쓰기 증가
        propertyRect.width -= 16;
        EditorGUI.PropertyField(propertyRect, property, true);

        if (extraSpace > 0)
        {
            EditorGUILayout.Space(extraSpace * 0.15f); // 간격 약간 증가
        }
    }

    private void DrawPropertyControl(SerializedProperty property, float fullWidth, GUIContent label)
    {
        Rect controlRect = EditorGUILayout.GetControlRect(true);
        float originalLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = label == GUIContent.none ? 0 : originalLabelWidth;

        float padding = fullWidth * 0.1f;
        controlRect.x = padding;
        controlRect.width = fullWidth - (padding * 2);

        EditorGUI.PropertyField(controlRect, property, label, true);
        EditorGUIUtility.labelWidth = originalLabelWidth;
    }

    private float GetExpandedPropertyHeight(SerializedProperty property)
    {
        float height = 0;
        var iterator = property.Copy();
        var endProperty = property.GetEndProperty();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
        {
            height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
            enterChildren = false;
        }

        return height + 4f; // 추가 여백
    }

    private void DrawMainHeader(float fullWidth)
    {
        EditorGUILayout.Space(2);
        Rect headerRect = EditorGUILayout.GetControlRect(false, 28); // 메인 헤더 높이 증가
        headerRect.x = 0;
        headerRect.width = fullWidth;

        // 그라데이션 효과를 위한 오버레이
        Color gradientStart = new Color(headerColor.r, headerColor.g, headerColor.b, 1f);
        Color gradientEnd = new Color(headerColor.r, headerColor.g, headerColor.b, 0.8f);
        EditorGUI.DrawRect(headerRect, gradientStart);
        
        string className = GetCachedUpperCase(target.GetType().Name);
        var style = GetHeaderStyle("MainHeader");
        style.fontSize = 12;
        style.alignment = TextAnchor.MiddleCenter; // 중앙 정렬
        EditorGUI.LabelField(headerRect, $"<b>{className}</b>", style);

        HandleMainHeaderContextMenu(headerRect);
        EditorGUILayout.Space(3);
    }
}