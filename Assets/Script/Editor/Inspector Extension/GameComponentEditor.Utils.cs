using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class GameComponentEditor
{
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
        SaveSettingsToJson();
        Repaint();
    }

    private void SaveSettingsToJson()
    {
        var data = new ComponentEditorData
        {
            componentTypeName = target.GetType().FullName,
            colorHtml = "#" + ColorUtility.ToHtmlStringRGBA(headerColor)
        };
        data.SetHeaderDictionary(customHeaderTexts);
        GameComponentEditorManager.UpdateComponentData(data);
    }
}