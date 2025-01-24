using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class GameComponentEditor
{
    private void LoadCustomHeaders()
    {
        var targetType = target.GetType();
        var componentData = GameComponentEditorManager.GetComponentData(targetType.FullName);
        
        if (componentData != null)
        {
            customHeaderTexts = componentData.GetHeaderDictionary();
        }
        else
        {
            customHeaderTexts.Clear();
        }
    }

    private bool HasHeaderAttribute(SerializedProperty property, out string headerText)
    {
        if (!currentTypeCache.HeaderAttributes.TryGetValue(property.name, out bool hasHeader))
        {
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
                    currentTypeCache.HeaderAttributes[property.name] = true;
                    currentTypeCache.HeaderTexts[property.name] = headerText;
                    return true;
                }
            }

            currentTypeCache.HeaderAttributes[property.name] = false;
            currentTypeCache.HeaderTexts[property.name] = string.Empty;
            return false;
        }

        headerText = currentTypeCache.HeaderTexts.GetValueOrDefault(property.name, string.Empty);
        return hasHeader;
    }
}