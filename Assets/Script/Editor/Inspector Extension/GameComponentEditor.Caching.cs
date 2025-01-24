using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public partial class GameComponentEditor
{
    // 타입별 캐시 컨테이너
    private class TypeCache
    {
        public readonly string TypeFullName;
        public readonly Dictionary<string, GUIStyle> Styles = new();
        public readonly Dictionary<string, string> UpperCaseTexts = new();
        public readonly Dictionary<string, bool> HeaderAttributes = new();
        public readonly Dictionary<string, string> HeaderTexts = new();
        public readonly Dictionary<string, string> CustomHeaders = new();

        public TypeCache(Type type)
        {
            TypeFullName = type.FullName;
        }

        public void Clear()
        {
            Styles.Clear();
            UpperCaseTexts.Clear();
            HeaderAttributes.Clear();
            HeaderTexts.Clear();
            CustomHeaders.Clear();
        }
    }

    // 타입별 캐시 저장소
    private static readonly Dictionary<Type, TypeCache> typeCaches = new();
    private TypeCache currentTypeCache;

    private void InitializeCache()
    {
        var targetType = target.GetType();
        if (!typeCaches.TryGetValue(targetType, out currentTypeCache))
        {
            currentTypeCache = new TypeCache(targetType);
            typeCaches[targetType] = currentTypeCache;
        }
    }

    private void ClearCaches()
    {
        if (currentTypeCache != null)
        {
            currentTypeCache.Clear();
        }

        // 미사용 캐시 정리
        CleanupUnusedCaches();
    }

    private static void CleanupUnusedCaches()
    {
        var unusedTypes = typeCaches.Keys
            .Where(type => !IsTypeInUse(type))
            .ToList();

        foreach (var type in unusedTypes)
        {
            typeCaches.Remove(type);
        }
    }

    private static bool IsTypeInUse(Type type)
    {
        return ActiveEditorTracker.sharedTracker.activeEditors
            .Any(editor => editor.target?.GetType() == type);
    }

    // 캐시 접근 메서드들
    private GUIStyle GetCachedStyle(string key)
    {
        if (!currentTypeCache.Styles.TryGetValue(key, out var style))
        {
            style = CreateStyle();
            currentTypeCache.Styles[key] = style;
        }
        return style;
    }

    private string GetCachedUpperCase(string text)
    {
        if (!currentTypeCache.UpperCaseTexts.TryGetValue(text, out var upper))
        {
            upper = text.ToUpper();
            currentTypeCache.UpperCaseTexts[text] = upper;
        }
        return upper;
    }

    private GUIStyle CreateStyle()
    {
        return new GUIStyle
        {
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(10, 10, 0, 0),
            fontSize = 12,
            richText = true,
            clipping = TextClipping.Clip,
            wordWrap = false
        };
    }
}