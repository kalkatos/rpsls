﻿using Kalkatos.Network.Model;
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
			StateInfo result = new StateInfo();
			foreach (var item in PrivateStateSignalsToSend)
			{
				if (origin != null && origin.PrivateProperties.ContainsKey(item.Key) && origin.PrivateProperties[item.Key] == item.Value)
					continue;
				result.PrivateProperties[item.Key] = item.Value;
			}
			return result;
		}

		public void ReceiveState (StateInfo stateInfo)
		{
			foreach (var item in PublicStateSignals)
				if (stateInfo.PublicProperties.ContainsKey(item.Key)
					&& (lastState == null || !lastState.PublicProperties.ContainsKey(item.Key) 
					|| stateInfo.PublicProperties[item.Key] != lastState.PublicProperties[item.Key]))
					item.EmitWithParam(stateInfo.PublicProperties[item.Key]);
			foreach (var item in PrivateStateSignals)
				if (stateInfo.PrivateProperties.ContainsKey(item.Key)
					&& (lastState == null || !lastState.PrivateProperties.ContainsKey(item.Key)
					|| stateInfo.PrivateProperties[item.Key] != lastState.PrivateProperties[item.Key]))
					item.EmitWithParam(stateInfo.PrivateProperties[item.Key]);
			lastState = stateInfo.Clone();
		}
	}
}