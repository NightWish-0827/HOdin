using UnityEditor;
using UnityEditor.Build;

namespace NKStudio
{
    [InitializeOnLoad]
    public class FMODLink
    {
        static FMODLink()
        {
            const string fmodFolder = "Assets/Plugins/FMOD";

            if (AssetDatabase.IsValidFolder(fmodFolder))
            {
                BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
#if UNITY_2023
                string symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));

                if (!symbols.Contains("USE_FMOD"))
                    PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), symbols + ";USE_FMOD");
#else
                  var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

                if (!symbols.Contains("USE_FMOD"))
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols + ";USE_FMOD");
#endif
            }
        }
    }
}
