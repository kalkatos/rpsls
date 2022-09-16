using Kalkatos.Network;

namespace Kalkatos.Tournament
{
    public class BotClient : Client
    {
        public override void SetInfo (PlayerInfo info)
        {
            base.SetInfo(info);
            SetReadyInGame();
        }

        public override void SetRound (RoundInfo tournamentInfo)
        {
            base.SetRound(tournamentInfo);
            SetReadyToStartMatch();
        }
    }
}
