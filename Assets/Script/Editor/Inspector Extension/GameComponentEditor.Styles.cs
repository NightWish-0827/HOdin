using UnityEditor;
using UnityEngine;

public partial class GameComponentEditor
{
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
}