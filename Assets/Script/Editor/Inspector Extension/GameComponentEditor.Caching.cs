using System.Collections.Generic;
using UnityEngine;

public partial class GameComponentEditor
{
    private static readonly Dictionary<string, GUIStyle> styleCache = new Dictionary<string, GUIStyle>();
    private static readonly Dictionary<string, string> upperCaseCache = new Dictionary<string, string>();
    private static readonly Dictionary<string, bool> headerAttributeCache = new Dictionary<string, bool>();
    private static readonly Dictionary<string, string> headerTextCache = new Dictionary<string, string>();
    private readonly Dictionary<string, string> customHeaderTexts = new Dictionary<string, string>();

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
}