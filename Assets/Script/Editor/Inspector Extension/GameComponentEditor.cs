using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
public partial class GameComponentEditor : Editor
{
    private const string HeaderColorKey = "GameComponentEditor_HeaderColor";

    private Color headerColor;
    private Dictionary<string, string> customHeaderTexts = new Dictionary<string, string>();

    private void OnEnable()
    {
        InitializeCache();
        LoadColor();
        LoadCustomHeaders();
        GameComponentEditorManager.OnSettingsChanged += OnSettingsChanged;
        serializedObject.Update();
    }

    private void OnDisable()
    {
        GameComponentEditorManager.OnSettingsChanged -= OnSettingsChanged;
        ClearCaches();
    }

    private void OnSettingsChanged()
    {
        LoadColor();
        LoadCustomHeaders();
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        try
        {
            float fullWidth = EditorGUIUtility.currentViewWidth;
            
            DrawMainHeader(fullWidth);
            EditorGUILayout.Space(5);
            DrawProperties(fullWidth);
        }
        catch (Exception e)
        {
            Debug.LogError($"Inspector GUI 오류: {e.Message}\n{e.StackTrace}");
            base.OnInspectorGUI(); // 폴백으로 기본 인스펙터 표시
        }
    }
}