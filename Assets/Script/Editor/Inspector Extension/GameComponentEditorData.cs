using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HeaderEntry
{
    public string propertyName;
    public string headerText;

    public HeaderEntry(string propertyName, string headerText)
    {
        this.propertyName = propertyName;
        this.headerText = headerText;
    }
}

[Serializable]
public class ComponentEditorData
{
    public string componentTypeName;
    public string colorHtml;
    public List<HeaderEntry> headerEntries = new List<HeaderEntry>();

    public Dictionary<string, string> GetHeaderDictionary()
    {
        var dict = new Dictionary<string, string>();
        foreach (var entry in headerEntries)
        {
            dict[entry.propertyName] = entry.headerText;
        }
        return dict;
    }

    public void SetHeaderDictionary(Dictionary<string, string> headers)
    {
        headerEntries.Clear();
        foreach (var kvp in headers)
        {
            headerEntries.Add(new HeaderEntry(kvp.Key, kvp.Value));
        }
    }
}

[Serializable]
public class EditorSettingsData
{
    public List<ComponentEditorData> components = new List<ComponentEditorData>();
}

