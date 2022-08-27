using System.Collections.Generic;

namespace Kalkatos.Rpsls
{
    public static class SessionData
    {
        public static string RoomName;
        public static bool IAmMasterClient;
        public static List<PlayerInfo> PlayerList = new List<PlayerInfo>();
    }
}
