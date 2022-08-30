using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.Rpsls
{
    public class TestData : SingletonScriptableObject<TestData>
    {
        public Dictionary<string, object> Data = new Dictionary<string, object>();
    }
}
