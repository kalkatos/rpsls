// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Kalkatos.Network.Unity
{
    [InitializeOnLoad]
    public class ScriptableEditorDefiner : Editor
    {
        public static readonly string[] Symbols = new string[] { "KALKATOS_SCRIPTABLE" };

        static ScriptableEditorDefiner ()
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
