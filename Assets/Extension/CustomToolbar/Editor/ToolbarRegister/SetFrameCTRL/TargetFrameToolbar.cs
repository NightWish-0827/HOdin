using UnityEditor;
using UnityToolbarExtender;
using UnityEngine;

namespace NKStudio
{
    public static class TargetFrameToolbar
    {
        public static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            GUILayout.Label("Frame", GUILayout.Width(40));
            Application.targetFrameRate = EditorGUILayout.IntField(Application.targetFrameRate, GUILayout.Width(30));
        }
    }
}