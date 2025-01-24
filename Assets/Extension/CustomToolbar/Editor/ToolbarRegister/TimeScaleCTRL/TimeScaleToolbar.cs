using UnityEditor;
using UnityToolbarExtender;
using UnityEngine;

namespace NKStudio
{
    public static class TimeScaleToolbar
    {
        public static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            GUILayout.Label("배속", GUILayout.Width(30));
            Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0, 5, GUILayout.Width(100));
            GUILayout.Label(Time.timeScale.ToString("0.00"), GUILayout.Width(35));
        }
    }
}