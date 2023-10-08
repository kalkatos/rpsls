using System.Collections.Generic;

namespace Kalkatos.Network.Model
{
	public class PlayerInfo
	{
		public string Alias;
		public string Nickname;
		public Dictionary<string ,string> CustomData;

		public PlayerInfo Clone ()
		{
			Dictionary<string, string> newCustomData = new Dictionary<string, string>();
			if (CustomData != null)
			{
				foreach (var item in CustomData)
					newCustomData.Add(item.Key, item.Value);
			}
			return new PlayerInfo
			{
				Alias = Alias,
				Nickname = Nickname,
				CustomData = newCustomData
			};
		}
	}
}