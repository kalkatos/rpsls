using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network;
using Newtonsoft.Json;
using System;

namespace Kalkatos.Tournament
{
    public abstract class Client : MonoBehaviour
    {
        [SerializeField] protected string Id;

        protected string currentOpponentId;
        protected string currentState = ClientState.Undefined;
        protected RoundInfo currentRound;
        protected MatchInfo currentMatch;
        protected MatchState currentMatchState;

        private void Awake ()
        {
            NetworkManager.OnEventReceived += HandleEventReceived;
            NetworkManager.OnExecuteFunctionSuccess += HandleExecuteFunctionSuccess;
            OnAwake();
        }

        private void OnDestroy ()
        {
            NetworkManager.OnEventReceived -= HandleEventReceived;
            NetworkManager.OnExecuteFunctionSuccess -= HandleExecuteFunctionSuccess;
            UponDestroy();
        }

        private void Update ()
        {
            
        }

        protected virtual void OnAwake () { }

        protected virtual void UponDestroy () { }

        private void HandleExecuteFunctionSuccess (object[] parameters)
        {
            
        }

        protected void HandleEventReceived (string key, object[] parameters)
        {
            //this.Log("Event received in client: " + key);
            Dictionary<string, object> paramDict = parameters.ToDictionary();
            object value = null;
            switch (key)
            {
                case Keys.RoundReceivedEvt:
                    if (paramDict.TryGetValue(Keys.RoundKey, out value))
                    {
                        //this.Log($"Round received:     {value}");
                        RoundInfo round = JsonConvert.DeserializeObject<RoundInfo>(value.ToString());
                        SetRound(round);
                    }
                    else
                        this.LogWarning("Didn't receive the key " + Keys.RoundKey);
                    break;
                case Keys.HandReceivedEvt:
                    // TODO Change to the real tratative of the hand
                    this.Wait(UnityEngine.Random.Range(1f, 2f), () =>
                    {
                        SetHand();
                    });
                    break;
                case Keys.TurnEndedEvt:
                    if (paramDict.TryGetValue(Keys.RoundKey, out value))
                    {
                        //this.Log($"Round received at turn End:     {value}");
                        RoundInfo round = JsonConvert.DeserializeObject<RoundInfo>(value.ToString());
                        HandleTurnResult(round);
                    }
                    else
                        this.LogWarning("Didn't receive the key " + Keys.RoundKey);
                    break;
            }
        }

        public virtual void SetId (string id)
        {
            Id = id;
        }

        protected virtual void SetState (string state, string addInfo = "")
        {
            currentState = NextState(currentState, addInfo);
            if (state != currentState)
                this.LogWarning($"Expected state {state} is different from correct next state {currentState}");
            else
                this.Log($"Set state: {currentState}");
            NetworkManager.Instance.SendCustomData($"{Keys.ClientStateKey}-{Id}", currentState);
            //if (info.IsBot)
            //{
            //    GameManager.UpdatePlayers();
            //    info = GameManager.GetPlayer(info.Id);
            //    info.CustomData = info.CustomData.CloneWithUpdateOrAdd(Keys.PlayerStatusKey, currentState);
            //    NetworkManager.Instance.UpdateBotData(info);
            //}
            //else
            //    NetworkManager.Instance.UpdateMyCustomData(Keys.PlayerStatusKey, currentState);
        }

        public virtual void SetRound (RoundInfo roundInfo)
        {
            currentRound = roundInfo;
            foreach (var item in currentRound.Matches)
            {
                if (item.Player1 == Id)
                {
                    currentMatch = item;
                    currentOpponentId = item.Player2;
                }
                else if (item.Player2 == Id)
                {
                    currentMatch = item;
                    currentOpponentId = item.Player1;
                }
            }
        }

        public virtual void SetHand ()
        {

        }

        public virtual void HandleTurnResult (RoundInfo roundInfo)
        {

        }

        public void SetStateAsInGame ()
        {
            SetState(ClientState.InGame);
        }

        public void SetStateAsInMatch ()
        {
            SetState(ClientState.InMatch);
        }

        public void SetStateAsInTurn ()
        {
            SetState(ClientState.InTurn);
        }

        public void SetStateAsHandReceived ()
        {
            SetState(ClientState.HandReceived);
        }

        public void SetStateAsWaitingForResult ()
        {
            SetState(ClientState.WaitingTurnResult);
        }

        public void SetStateAsBetweenRounds ()
        {
            SetState(ClientState.BetweenRounds, ClientState.BetweenRounds);
        }

        protected string NextState (string currentState, string addInfo = "")
        {
            switch (currentState)
            {
                case ClientState.Undefined:
                case "":
                    return ClientState.InGame;
                case ClientState.InGame:
                case ClientState.BetweenRounds:
                    return ClientState.InMatch;
                case ClientState.InMatch:
                    return ClientState.InTurn;
                case ClientState.InTurn:
                    return ClientState.HandReceived;
                case ClientState.HandReceived:
                    return ClientState.WaitingTurnResult;
                case ClientState.WaitingTurnResult:
                    if (string.IsNullOrEmpty(addInfo))
                        return ClientState.InTurn;
                    else if (addInfo == ClientState.BetweenRounds)
                        return ClientState.BetweenRounds;
                    else
                        return ClientState.GameOver;
            }
            return ClientState.Undefined;
        }
    }
}
