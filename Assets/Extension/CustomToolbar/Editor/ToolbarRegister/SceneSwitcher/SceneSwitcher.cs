#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace NKStudio
{
    public static class ToolbarStyles
    {
        public static readonly GUIStyle CommandButtonStyle;

        static ToolbarStyles()
        {
            CommandButtonStyle = new GUIStyle("Button")
            {
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Normal,
            };
        }
    }

    public static class SceneSwitchLeftButton
    {
        public static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            string[] allLevelName = FillLevels();
            string moveScene = string.Empty;
            string firstLevelName;
            bool isOnDisable;

            if (allLevelName == null)
            {
                firstLevelName = "None";
                isOnDisable = true;
            }
            else
            {
                firstLevelName = Path.GetFileNameWithoutExtension(allLevelName[0]);
                moveScene = allLevelName[0];
                isOnDisable = false;
            }

            if (isOnDisable)
                EditorGUI.BeginDisabledGroup(true);

            string tooltip = Application.systemLanguage == SystemLanguage.Korean ? "최초 세팅 씬부터 게임을 진입합니다." : "Start from the first scene.";
            if (GUILayout.Button(new GUIContent($"Start '{firstLevelName}'", tooltip), ToolbarStyles.CommandButtonStyle))
                SceneHelper.StartScene(moveScene, true);

            if (isOnDisable)
                EditorGUI.EndDisabledGroup();
        }

        private static string[] FillLevels()
        {
            List<string> list = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                if (scene.enabled)
                    list.Add(scene.path);
            return list.Count == 0 ? null : list.ToArray();
        }
    }

    public static class SceneSwitchRightButton
    {
        private static GenericMenu _menu;
    
        public static void OnToolbarGUI()
        {
            EditorGUIUtility.labelWidth = 60;

            if (EditorGUILayout.DropdownButton(new GUIContent("씬 선택"), FocusType.Keyboard))
            {
                Dictionary<string, string> allScenes = SceneHelper.FindAllScenes();
                _menu = new GenericMenu();
                foreach ((string scenePath, string sceneAllPath) in allScenes)
                    _menu.AddItem(new GUIContent($"{scenePath}"), false, OnClickDropdown, sceneAllPath);
                _menu.ShowAsContext();
            }

            GUILayout.FlexibleSpace();
        }

        private static void OnClickDropdown(object parameter)
        {
            SceneHelper.StartScene((string)parameter, false);
        }
    }

    internal static class SceneHelper
    {
        private static string _sceneToOpen;
        private static bool _isAutoPlay;
        private const string TargetSceneFolderPath = "Assets/Scenes";
    
        public static void StartScene(string sceneName, bool isPlay)
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            _sceneToOpen = sceneName;
            _isAutoPlay = isPlay;
            EditorApplication.update += OnUpdate;
        }

        public static Dictionary<string, string> FindAllScenes()
        {
            Dictionary<string, string> result = new();
            string[] guids = AssetDatabase.FindAssets("t:scene ", new[] { TargetSceneFolderPath });
            foreach (string guid in guids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                string filterSceneName = scenePath.Replace(".unity", "");
                try
                {
                    string resultPath = filterSceneName.Replace($"{TargetSceneFolderPath}/", "");
                    string resultSceneName = scenePath;
                    result.Add(resultPath, resultSceneName);
                }
                catch (Exception)
                {
                }
            }
            return result;
        }

        private static void OnUpdate()
        {
            if (_sceneToOpen == null ||
                EditorApplication.isPlaying || EditorApplication.isPaused ||
                EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            EditorApplication.update -= OnUpdate;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(_sceneToOpen);
                EditorApplication.isPlaying = _isAutoPlay;
            }
            _isAutoPlay = false;
            _sceneToOpen = null;
        }
    }
}
#endif
