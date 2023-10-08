// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

#if KALKATOS_NETWORK

using Sirenix.OdinInspector;
using UnityEngine;

namespace Kalkatos.UnityGame.Scriptable.Network
{
	[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Network/Player Data")]
	public class PlayerDataScriptable : ScriptableObject
	{
		[InlineProperty, HideLabel] public PlayerData Data;
	}
}

#endif