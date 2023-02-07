using Kalkatos.Network.Model;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.UnityGame.Scriptable.Network
{
	[CreateAssetMenu(fileName = "StateBuilder", menuName = "Network/State Builder")]
	public class StateBuilder : ScriptableObject
	{
		public List<StateSignal> PublicStatePieces;
		public List<StateSignal> PrivateStatePieces;

		public StateInfo BuildChangedPieces (StateInfo origin)
		{
			StateInfo result = new StateInfo { PublicProperties = new Dictionary<string, string>(), PrivateProperties = new Dictionary<string, string>() };
			foreach (var item in PublicStatePieces)
			{
				if (origin != null && origin.PublicProperties.ContainsKey(item.Key) && origin.PublicProperties[item.Key] == item.Value)
					continue;
				result.PublicProperties.Add(item.Key, item.Value);
			}
			foreach (var item in PrivateStatePieces)
			{
				if (origin != null && origin.PrivateProperties.ContainsKey(item.Key) && origin.PrivateProperties[item.Key] == item.Value)
					continue;
				result.PrivateProperties.Add(item.Key, item.Value);
			}
			return result;
		}

		public void ReceiveState (StateInfo stateInfo)
		{
			foreach (var item in PublicStatePieces)
				if (stateInfo.PublicProperties.ContainsKey(item.Key)) 
					item.EmitWithParam(stateInfo.PublicProperties[item.Key]);
			foreach (var item in PrivateStatePieces)
				if (stateInfo.PrivateProperties.ContainsKey(item.Key))
					item.EmitWithParam(stateInfo.PrivateProperties[item.Key]);
		}
	}
}