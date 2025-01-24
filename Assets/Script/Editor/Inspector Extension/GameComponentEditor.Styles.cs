using UnityEditor;
using UnityEngine;

public partial class GameComponentEditor
{
    private GUIStyle GetHeaderStyle(string key)
    {
        return GetCachedStyle(key);
    }

    private void LoadColor()
    {
        var componentData = GameComponentEditorManager.GetComponentData(target.GetType().FullName);
        if (componentData != null && !string.IsNullOrEmpty(componentData.colorHtml))
        {
            ColorUtility.TryParseHtmlString(componentData.colorHtml, out headerColor);
        }
        else
        {
            headerColor = new Color(0.2f, 0.3f, 0.7f); // 기본 색상
        }
    }
}