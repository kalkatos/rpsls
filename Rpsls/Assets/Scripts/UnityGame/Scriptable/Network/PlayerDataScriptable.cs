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
