using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Kalkatos
{
    [CreateAssetMenu(menuName = "Save", fileName = "SaveScriptable")]
    public class SaveScriptable : SingletonScriptableObject<SaveScriptable>
    {
        [TextArea]
        public string Data;

        [Button]
        public void Clear ()
        {
            Data = string.Empty;
        }
    }
}
