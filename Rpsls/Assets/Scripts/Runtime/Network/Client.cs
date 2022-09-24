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

        protected PlayerInfo info;
        protected string currentOpponentId;
        protected ClientState currentState = ClientState.Undefined;
        protected RoundInfo currentRound;
        protected MatchInfo currentMatch;

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
            this.Log("Event received in client: " + key);
            Dictionary<string, object> paramDict = parameters.ToDictionary();
            switch (key)
            {
                case Keys.RoundReceivedEvt:
                    if (paramDict.TryGetValue(Keys.TournamentIdKey, out object value))
                    {
                        this.Log($"Round received:     {value}");
                        RoundInfo round = JsonConvert.DeserializeObject<RoundInfo>(value.ToString());
                        SetRound(round);
                    }
                    else
                        this.LogWarning("Didn't receive the key " + Keys.TournamentIdKey);
                    break;
                case Keys.TurnUpdateEvt:

                    break;
            }
        }

        protected void SetState (ClientState state)
        {
            currentState = state;
            //NetworkManager.Instance.SendCustomData($"{Keys.PlayerStatusKey}-{Id}", (int)currentState);
            NetworkManager.Instance.UpdateMyCustomData(Keys.PlayerStatusKey, (int)currentState);
        }

        public virtual void SetInfo (PlayerInfo info)
        {
            this.info = info;
            Id = info.Id;
        }

        public virtual void SetRound (RoundInfo roundInfo)
        {
            currentRound = roundInfo;
            foreach (var item in currentRound.Matches)
            {
                if (item.Player1.Id == Id)
                {
                    currentMatch = item;
                    currentOpponentId = item.Player2?.Id ?? "";
                }
                else if (item.Player2 != null && item.Player2.Id == Id)
                {
                    currentMatch = item;
                    currentOpponentId = item.Player1.Id;
                }
            }
        }

        public void SetReadyInGame ()
        {
            SetState(ClientState.GameReady);
        }

        public void SetReadyToStartMatch ()
        {
            SetState(ClientState.MatchReady);
        }
    }
}
