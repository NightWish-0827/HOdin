using UnityEditor;
using UnityEngine;

public partial class GameComponentEditor
{
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
}