using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace NKStudio
{
    public static class FrameRateToolbar
    {
        private static float deltaTime = 0.0f;
        private static bool isPlaying = false;

        static FrameRateToolbar()
        {
            // 씬이 변경될 때 이벤트를 구독하여 FPS 측정 초기화
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                isPlaying = true;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                isPlaying = false;
                deltaTime = 0.0f;
            }
        }

        private static void OnActiveSceneChanged(Scene current, Scene next)
        {
            deltaTime = 0.0f;
        }

        public static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            if (isPlaying)
            {
                deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
                float fps = 1.0f / deltaTime;

                string label = string.Format("FPS: {0:0.}", fps);
                string tooltip = "Current Frame Rate";

                GUILayout.Label(new GUIContent(label, tooltip), ToolbarStyles.CommandButtonStyle);
            }
            else
            {
                GUILayout.Label(new GUIContent("FPS: --", "Current Frame Rate"), ToolbarStyles.CommandButtonStyle);
            }
        }
    }
}
