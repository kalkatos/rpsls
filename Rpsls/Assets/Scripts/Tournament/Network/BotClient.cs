using Kalkatos.Network;

namespace Kalkatos.Tournament
{
    public class BotClient : Client
    {
        protected override void OnAwake ()
        {
            base.OnAwake();
            this.Wait(1f, SetStateAsInGame);
        }

        protected override void SetState (string state)
        {
            base.SetState(state);
            PlayerInfo info = NetworkManager.Instance.GetPlayer(Id);
            if (info != null)
            {
                info.CustomData = info.CustomData.CloneWithUpdateOrAdd(Keys.PlayerStatusKey, currentState);
                NetworkManager.Instance.UpdateBotData(info);
            }
            else
                this.LogWarning("Bot not found: " + Id);
        }

        public override void SetRound (RoundInfo roundInfo)
        {
            base.SetRound(roundInfo);
            SetStateAsInMatch();
            this.Wait(1f, SetStateAsInTurn);
        }

        public override void SetHand ()
        {
            base.SetHand();
            SetStateAsHandReceived();
            this.Wait(2f, SetStateAsWaitingForResult);
        }

        public override void HandleTurnResult (RoundInfo roundInfo)
        {
            this.Wait(1f, SetStateAsInTurn);
        }
    }
}
