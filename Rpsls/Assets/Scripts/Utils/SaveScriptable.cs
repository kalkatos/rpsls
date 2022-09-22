using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Kalkatos
{
    [CreateAssetMenu(menuName = "Save", fileName = "SaveScriptable")]
    public class SaveScriptable : SingletonScriptableObject<SaveScriptable>
    {
        public string DefaultText;
        [TextArea(70, 70)]
        public string Data;

        [Button]
        public void Clear ()
        {
            Data = DefaultText;
        }

        [Button]
        public void PrettyPrint ()
        {
            Data = Data.Replace("\\n", "\n");
            Data = Data.Replace("\\r", "\r");
            Data = Data.Replace("\\\"", "\"");
            Data = Data.Replace("\"{", "{");
            Data = Data.Replace("}\"", "}");
            Data = Data.Replace("\"[", "[");
            Data = Data.Replace("]\"", "]");
            Data = Data.Replace("\\", "");
            Debug.Log(Data);
        }
    }
}
