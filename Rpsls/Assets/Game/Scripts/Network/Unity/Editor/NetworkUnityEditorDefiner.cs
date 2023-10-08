// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

#if UNITY_2018_1_OR_NEWER

using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Kalkatos.Network.Unity
{
    [InitializeOnLoad]
    public class NetworkUnityEditorDefiner : Editor
    {
        public static readonly string[] Symbols = new string[] { "KALKATOS_NETWORK" };

        static NetworkUnityEditorDefiner ()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(Symbols.Except(allDefines));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }
    }
}

#endif