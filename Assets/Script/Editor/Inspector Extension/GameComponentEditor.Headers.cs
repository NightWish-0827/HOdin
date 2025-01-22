using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class GameComponentEditor
{
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
}