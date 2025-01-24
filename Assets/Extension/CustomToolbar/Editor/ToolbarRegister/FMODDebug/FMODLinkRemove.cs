using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
namespace NKStudio
{
    public static class FMODLinkRemove
    {
        [MenuItem("Tools/Toolbar/FMOD/Define Remove")]
        private static void RemoveDefine()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
#if UNITY_2023
            // FMOD 폴더가 없으면 USE_FMOD 심볼을 제거합니다.
            string symbols = PlayerSettings.GetScriptingDefineSymbols(
                NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));

            if (symbols.Contains("USE_FMOD"))
            {
                symbols = symbols.Replace("USE_FMOD", "");
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), symbols);
            }
            else
                Debug.LogWarning("USE_FMOD 심볼이 존재하지 않습니다.");
#else
            // FMOD 폴더가 없으면 USE_FMOD 심볼을 제거합니다.
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                BuildTargetGroup.Standalone);

            if (symbols.Contains("USE_FMOD"))
            {
                symbols = symbols.Replace("USE_FMOD", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    BuildTargetGroup.Standalone, symbols);
            }
            else
                Debug.LogWarning("USE_FMOD 심볼이 존재하지 않습니다.");
#endif
        }
    }
}
