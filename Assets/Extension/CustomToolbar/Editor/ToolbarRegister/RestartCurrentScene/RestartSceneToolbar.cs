using UnityEditor;
using UnityToolbarExtender;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NKStudio
{
    public static class RestartSceneToolbar
    {
        public static void OnToolbarGUI()
        {
             EditorGUIUtility.labelWidth = 60;

            if (GUILayout.Button("재시작", GUILayout.Width(50)))
            {
                RestartCurrentScene();
            }

            GUILayout.FlexibleSpace();
        }

        private static void RestartCurrentScene()
        {
            // Check if the application is in Play Mode
            if (!Application.isPlaying)
            {
                // Display a warning message in the console
               // Debug.LogWarning("<color=yellow> H.MOD </color> <color=white> : </color> <color=red> 현재  PlayMode  가 아닙니다. </color>");
                return;
            }

            // Get the current active scene
            Scene currentScene = SceneManager.GetActiveScene();
            string sceneName = currentScene.name;

            // Reload the current scene
            SceneManager.LoadScene(sceneName);

            // Display a colored debug message in the console
            //Debug.Log($"<color=yellow> H.MOD </color> <color=white> : </color> <color=red> 해당 < </color> <color=white> {sceneName} </color> <color=red> > 재 시작 되었습니다. </color>");
        }
    }
}