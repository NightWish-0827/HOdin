using UnityEditor;
using UnityToolbarExtender;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NKStudio
{
    [InitializeOnLoad]
    public class ToolbarRegister
    {
        static ToolbarRegister()
        {
           // ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUILeft);
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUIRight);

            // Add play mode state change callback
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

/*        static void OnToolbarGUILeft()
        {
            // 가장 좌측에 TimeScale 배치
            //TimeScaleToolbar.OnToolbarGUI();
            //GUILayout.Space(950); // 간격 조정
            
            //GUILayout.FlexibleSpace();
            
            // PlayMode 버튼 좌측에 Frame 관련 기능들 배치
            GUILayout.Space(-615);
            TargetFrameToolbar.OnToolbarGUI(); // 간격 조정
            GUILayout.Space(-612); // 간격 조정
            FrameRateToolbar.OnToolbarGUI();
        }
*/
        static void OnToolbarGUIRight()
        {
            // PlayMode 버튼 우측에 EnterPlayMode 토글 배치
            GUILayout.Space(-5); // 간격 조정
            EnterPlayModeOptionToolbars.OnToolbarGUI();
#if USE_FMOD
            FMODDebugToolbars.OnToolbarGUI();
#endif
            
            GUILayout.FlexibleSpace();
            
            // 가장 우측에 Scene 관련 기능들 배치
            RestartSceneToolbar.OnToolbarGUI();
            GUILayout.Space(-497); // 간격 조정
            SceneSwitchRightButton.OnToolbarGUI();
            GUILayout.Space(-495); // 간격 조정
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Get the current active scene
                Scene currentScene = SceneManager.GetActiveScene();
                string sceneName = currentScene.name;

                // Display a colored debug message in the console
                Debug.Log($"<color=yellow> H.MOD </color> <color=white> : </color> <color=green> 해당 < </color> <color=white> {sceneName} </color> <color=green> > 시작 되었습니다. </color>");
            }
        }
    }
}