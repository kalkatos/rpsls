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

        public override void SetTournament (TournamentInfo tournamentInfo)
        {
            base.SetTournament(tournamentInfo);
            SetReadyToStartMatch();
        }
    }
}
