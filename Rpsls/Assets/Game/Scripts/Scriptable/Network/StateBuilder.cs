// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

#if KALKATOS_NETWORK

using Kalkatos.Network.Model;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.UnityGame.Scriptable.Network
{
	[CreateAssetMenu(fileName = "StateBuilder", menuName = "Network/State Builder")]
	public class StateBuilder : ScriptableObject
	{
		[Header("Receiving")]
		public List<SignalState> PublicStateSignals;
		public List<SignalState> PrivateStateSignals;
		[Header("Sending")]
		public List<SignalState> PrivateStateSignalsToSend;

		public void Initialize ()
		{
			Logger.Log("[StateBuilder] Initializing state builder.");
			foreach (var item in PublicStateSignals)
				item.Value = "";
			foreach (var item in PrivateStateSignals)
				item.Value = "";
		}

		public ActionInfo BuildChangedPieces (StateInfo origin)
		{
			ActionInfo result = new ActionInfo();
			foreach (var item in PrivateStateSignalsToSend)
			{
				if (origin != null && origin.PrivateProperties.ContainsKey(item.Key) && origin.PrivateProperties[item.Key] == item.Value)
					continue;
				result.PrivateChanges[item.Key] = item.Value;
			}
			return result;
		}

		public void ReceiveState (StateInfo stateInfo)
		{
			if (stateInfo == null) 
				return;
			foreach (var item in PublicStateSignals)
				if (stateInfo.PublicProperties.ContainsKey(item.Key) && stateInfo.PublicProperties[item.Key] != item.Value)
					item.EmitWithParam(stateInfo.PublicProperties[item.Key]);
			foreach (var item in PrivateStateSignals)
				if (stateInfo.PrivateProperties.ContainsKey(item.Key) && stateInfo.PrivateProperties[item.Key] != item.Value)
					item.EmitWithParam(stateInfo.PrivateProperties[item.Key]);
		}
	}
}

#endif