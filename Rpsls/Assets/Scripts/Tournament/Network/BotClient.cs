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

        protected override void SetState (string state, string addInfo = "")
        {
            base.SetState(state, addInfo);
            PlayerInfo info = NetworkManager.Instance.GetPlayer(Id);
            if (info != null)
            {
                info.CustomData = info.CustomData.CloneWithUpdateOrAdd(Keys.ClientStateKey, currentState);
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
            if (roundInfo.IsOver)
                this.Wait(1f, SetStateAsBetweenRounds);
            else
                this.Wait(1f, SetStateAsInTurn);
        }
    }
}
