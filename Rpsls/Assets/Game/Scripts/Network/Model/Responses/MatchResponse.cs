namespace Kalkatos.Network.Model
{
	public class MatchResponse : Response
	{
		public string MatchId;
		public PlayerInfo[] Players;
		public bool IsOver;
	}
}
