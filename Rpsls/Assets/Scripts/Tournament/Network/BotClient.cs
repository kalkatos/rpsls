using Kalkatos.Network;

namespace Kalkatos.Tournament
{
    public class BotClient : Client
    {
        public override void SetInfo (PlayerInfo info)
        {
            base.SetInfo(info);
            SetStateAsInGame();
        }

        public override void SetRound (RoundInfo roundInfo)
        {
            base.SetRound(roundInfo);
            SetStateAsInMatch();
            this.Wait(1f, () => SetStateAsInTurn());
        }

        public override void SetHand ()
        {
            base.SetHand();
            SetStateAsWaitingForResult();
        }

        public override void HandleTurnResult (RoundInfo roundInfo)
        {
            SetStateAsInMatch();
        }
    }
}
