using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
public class GameComponentEditor : Editor

{
    private const string HeaderColorKey = "GameComponentEditor_HeaderColor";
    private const string HeaderTextKey = "CustomHeader_";

    // 캐시 시스템
    private static readonly Dictionary<string, GUIStyle> styleCache = new Dictionary<string, GUIStyle>();
    private static readonly Dictionary<string, string> upperCaseCache = new Dictionary<string, string>();
    private static readonly Dictionary<string, bool> headerAttributeCache = new Dictionary<string, bool>();
    private static readonly Dictionary<string, string> headerTextCache = new Dictionary<string, string>();
    private readonly Dictionary<string, string> customHeaderTexts = new Dictionary<string, string>();

    private Color headerColor;
    private SerializedProperty cachedIterator;
    private StringBuilder stringBuilder;

    #region 초기화 및 정리
    private void OnEnable()
    {
        LoadColor();
        LoadCustomHeaders();
        stringBuilder = new StringBuilder();
    }

    private void OnDisable()
    {
        ClearCaches();
    }

    private void ClearCaches()
    {
        if (cachedIterator != null)
        {
            cachedIterator.Dispose();
            cachedIterator = null;
        }

        styleCache.Clear();
        upperCaseCache.Clear();
        headerAttributeCache.Clear();
        headerTextCache.Clear();
        customHeaderTexts.Clear();
    }
    #endregion

    #region 스타일 및 색상 관리
    private GUIStyle GetHeaderStyle(string key)
    {
        if (!styleCache.TryGetValue(key, out var style))
        {
            style = new GUIStyle
            {
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 5, 5),
                fontSize = 12,
                richText = true
            };
            styleCache[key] = style;
        }
        return style;
    }

    private void LoadColor()
    {
        string colorString = EditorPrefs.GetString(HeaderColorKey, "");
        if (string.IsNullOrEmpty(colorString))
        {
            headerColor = new Color(0.2f, 0.3f, 0.7f);
        }
        else
        {
            ColorUtility.TryParseHtmlString(colorString, out headerColor);
        }
    }
    #endregion

    #region 헤더 관리
    private void LoadCustomHeaders()
    {
        var targetType = target.GetType();
        var fields = targetType.GetFields(System.Reflection.BindingFlags.Instance |
                                       System.Reflection.BindingFlags.Public |
                                       System.Reflection.BindingFlags.NonPublic);

        string typePrefix = $"{HeaderTextKey}{targetType.Name}_";
        foreach (var field in fields)
        {
            string savedText = EditorPrefs.GetString($"{typePrefix}{field.Name}", "");
            if (!string.IsNullOrEmpty(savedText))
            {
                customHeaderTexts[field.Name] = savedText;
            }
        }
    }

    private bool HasHeaderAttribute(SerializedProperty property, out string headerText)
    {
        string key = $"{property.serializedObject.targetObject.GetType().FullName}_{property.name}";

        if (headerAttributeCache.TryGetValue(key, out bool hasHeader))
        {
            headerText = headerTextCache.GetValueOrDefault(key, string.Empty);
            return hasHeader;
        }

        headerText = string.Empty;
        var field = property.serializedObject.targetObject.GetType()
            .GetField(property.name, System.Reflection.BindingFlags.Instance |
                                   System.Reflection.BindingFlags.Public |
                                   System.Reflection.BindingFlags.NonPublic);

        if (field != null)
        {
            var headerAttribute = field.GetCustomAttributes(typeof(HeaderAttribute), false)
                .FirstOrDefault() as HeaderAttribute;

            if (headerAttribute != null)
            {
                headerText = headerAttribute.header;
                headerAttributeCache[key] = true;
                headerTextCache[key] = headerText;
                return true;
            }
        }

        headerAttributeCache[key] = false;
        headerTextCache[key] = string.Empty;
        return false;
    }

    private string GetUpperCase(string text)
    {
        if (!upperCaseCache.TryGetValue(text, out string upper))
        {
            upper = text.ToUpper();
            upperCaseCache[text] = upper;
        }
        return upper;
    }
    #endregion

    #region GUI 렌더링
    public override void OnInspectorGUI()
    {
        float fullWidth = EditorGUIUtility.currentViewWidth;
        DrawMainHeader(fullWidth);
        EditorGUILayout.Space(5);
        DrawProperties(fullWidth);
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
    {
        // 확장 가능한 필드인 경우 추가 여백 계산
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

    // 확장된 프로퍼티의 실제 높이를 계산
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

        return height + 10f; // 추가 여백
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
    #endregion

    #region 컨텍스트 메뉴
    private void HandleMainHeaderContextMenu(Rect headerRect)
    {
        if (Event.current.type == EventType.MouseDown &&
            Event.current.button == 1 &&
            headerRect.Contains(Event.current.mousePosition))
        {
            ShowHeaderContextMenu();
            Event.current.Use();
        }
    }

    private void HandlePropertyContextMenu(Rect headerRect, SerializedProperty property, string displayText)
    {
        if (Event.current.type == EventType.MouseDown &&
            Event.current.button == 1 &&
            headerRect.Contains(Event.current.mousePosition))
        {
            ShowHeaderContextMenu(property, displayText);
            Event.current.Use();
        }
    }

    private void ShowHeaderContextMenu(SerializedProperty property, string currentHeaderText)
    {
        string propertyName = property.name;
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("헤더 텍스트 변경"), false, () =>
        {
            var window = EditorWindow.GetWindow<HeaderTextEditor>(true, "헤더 텍스트 편집");
            window.Initialize(this, propertyName, currentHeaderText);
        });
        menu.AddItem(new GUIContent("기본값으로 초기화"), false, () =>
        {
            string key = $"{HeaderTextKey}{target.GetType().Name}_{propertyName}";
            EditorPrefs.DeleteKey(key);
            customHeaderTexts.Remove(propertyName);
            Repaint();
        });
        menu.ShowAsContext();
    }

    private void ShowHeaderContextMenu()
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("헤더 색상 변경"), false, () =>
        {
            var colorPicker = EditorWindow.GetWindow<ColorPicker>(true, "헤더 색상 선택");
            colorPicker.color = headerColor;
            colorPicker.editor = this;
            colorPicker.Show();
        });
        menu.AddItem(new GUIContent("기본 색상으로 초기화"), false, () =>
        {
            headerColor = new Color(0.2f, 0.3f, 0.7f);
            EditorPrefs.DeleteKey(HeaderColorKey);
            Repaint();
        });
        menu.ShowAsContext();
    }
    #endregion

    #region 유틸리티
    private bool ShouldDrawHeader(SerializedProperty property) =>
        property.isArray ||
        property.propertyType == SerializedPropertyType.Integer ||
        property.propertyType == SerializedPropertyType.Float ||
        property.propertyType == SerializedPropertyType.String ||
        property.propertyType == SerializedPropertyType.Boolean ||
        property.propertyType == SerializedPropertyType.Enum ||
        property.propertyType == SerializedPropertyType.ObjectReference;

    private bool IsSpaceAttribute(SerializedProperty property) =>
        property.propertyType == SerializedPropertyType.Generic &&
        property.type.Contains("Space");

    public void UpdateColor(Color newColor)
    {
        headerColor = newColor;
        EditorPrefs.SetString(HeaderColorKey, "#" + ColorUtility.ToHtmlStringRGBA(newColor));
        Repaint();
    }
    #endregion

    #region 내부 클래스
    public class ColorPicker : EditorWindow
    {
        public Color color;
        public GameComponentEditor editor;

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            color = EditorGUILayout.ColorField("색상", color);
            if (EditorGUI.EndChangeCheck() && editor != null)
            {
                editor.UpdateColor(color);
            }
        }
    }

    public class HeaderTextEditor : EditorWindow
    {
        private GameComponentEditor editor;
        private string propertyName;
        private string headerText;
        private string targetTypeName;

        public void Initialize(GameComponentEditor editor, string propertyName, string currentText)
        {
            this.editor = editor;
            this.propertyName = propertyName;
            this.targetTypeName = editor.target.GetType().Name;
            this.headerText = currentText;
            this.minSize = new Vector2(300, 100);
            this.maxSize = new Vector2(300, 100);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("헤더 텍스트 입력");
            headerText = EditorGUILayout.TextField(headerText);

            if (GUILayout.Button("저장"))
            {
                string key = $"{HeaderTextKey}{targetTypeName}_{propertyName}";
                EditorPrefs.SetString(key, headerText);
                editor.customHeaderTexts[propertyName] = headerText;
                editor.Repaint();
                Close();
            }
        }
    }
    #endregion
}