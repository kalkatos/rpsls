namespace Kalkatos.Network.Model
{
	public class LoginResponse : Response
	{
        public bool IsAuthenticated;
        public bool MustRunLocally;
        public string PlayerId;
        public PlayerInfo MyInfo;
    }
}
