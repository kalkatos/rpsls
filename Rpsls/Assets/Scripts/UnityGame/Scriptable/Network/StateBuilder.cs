using Kalkatos.Network.Model;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.UnityGame.Scriptable.Network
{
	[CreateAssetMenu(fileName = "StateBuilder", menuName = "Network/State Builder")]
	public class StateBuilder : ScriptableObject
	{
		public List<SignalState> PublicStateSignals;
		public List<SignalState> PrivateStateSignals;

		private StateInfo lastState = null;

		public void Initialize ()
		{
			Logger.Log("  [[[  StateBuilder  ]]]  Initializing state builder.");
			foreach (var item in PublicStateSignals)
				item.Value = "";
			foreach (var item in PrivateStateSignals)
				item.Value = "";
		}

		public StateInfo BuildChangedPieces (StateInfo origin)
		{
			StateInfo result = new StateInfo { PublicProperties = new Dictionary<string, string>(), PrivateProperties = new Dictionary<string, string>() };
			foreach (var item in PublicStateSignals)
			{
				if (origin != null && origin.PublicProperties.ContainsKey(item.Key) && origin.PublicProperties[item.Key] == item.Value)
					continue;
				result.PublicProperties.Add(item.Key, item.Value);
			}
			foreach (var item in PrivateStateSignals)
			{
				if (origin != null && origin.PrivateProperties.ContainsKey(item.Key) && origin.PrivateProperties[item.Key] == item.Value)
					continue;
				result.PrivateProperties.Add(item.Key, item.Value);
			}
			return result;
		}

		public void ReceiveState (StateInfo stateInfo)
		{
			foreach (var item in PublicStateSignals)
				if (stateInfo.PublicProperties.ContainsKey(item.Key)
					&& (lastState == null || stateInfo.PublicProperties[item.Key] != lastState.PublicProperties[item.Key]))
					item.EmitWithParam(stateInfo.PublicProperties[item.Key]);
			foreach (var item in PrivateStateSignals)
				if (stateInfo.PrivateProperties.ContainsKey(item.Key)
					&& (lastState == null || stateInfo.PrivateProperties[item.Key] != lastState.PrivateProperties[item.Key]))
					item.EmitWithParam(stateInfo.PrivateProperties[item.Key]);
			lastState = stateInfo.Clone();
		}
	}
}