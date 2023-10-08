using System.Collections.Generic;

namespace Kalkatos.Network.Model
{
	public class SetPlayerDataRequest
	{
		public string PlayerId;
		public Dictionary<string, string> Data;
	}
}
