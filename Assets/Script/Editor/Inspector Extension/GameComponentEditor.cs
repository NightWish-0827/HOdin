using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
public partial class GameComponentEditor : Editor
{
    private const string HeaderColorKey = "GameComponentEditor_HeaderColor";
    private const string HeaderTextKey = "CustomHeader_";

    private Color headerColor;
    private SerializedProperty cachedIterator;
    private StringBuilder stringBuilder;

    private void OnEnable()
    {
        LoadColor();
        LoadCustomHeaders();
        stringBuilder = new StringBuilder();
    }

    private void OnDisable()
    {
        ClearCaches();
    }

    public override void OnInspectorGUI()
    {
        float fullWidth = EditorGUIUtility.currentViewWidth;
        DrawMainHeader(fullWidth);
        EditorGUILayout.Space(5);
        DrawProperties(fullWidth);
    }
}
