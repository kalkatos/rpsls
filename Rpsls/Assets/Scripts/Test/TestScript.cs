using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network.Model;
using System.Linq;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestScript : MonoBehaviour
{
#if UNITY_EDITOR
	[MenuItem("Test/Test")]
    public static void Test ()
    {
		StateInfo stateInfo = new StateInfo
		{
			PrivateProperties = new Dictionary<string, string> { { "Valae", "1" } },
			PublicProperties = new Dictionary<string, string> { { "Chimp", "Seux" }, { "Champ", "Doue" } },
		};
		HashState(stateInfo);
		Debug.Log(stateInfo.Hash);

		void HashState (StateInfo stateInfo)
		{
			unchecked
			{
				stateInfo.Hash = 23;
				foreach (var item in stateInfo.PublicProperties)
				{
					foreach (char c in item.Key)
						stateInfo.Hash = stateInfo.Hash * 31 + c;
					foreach (char c in item.Value)
						stateInfo.Hash = stateInfo.Hash * 31 + c;
				}
				foreach (var item in stateInfo.PrivateProperties)
				{
					foreach (char c in item.Key)
						stateInfo.Hash = stateInfo.Hash * 31 + c;
					foreach (char c in item.Value)
						stateInfo.Hash = stateInfo.Hash * 31 + c;
				}
			}
		}
	}

#endif
}
