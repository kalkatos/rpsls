using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network;
using Newtonsoft.Json;

namespace Kalkatos.Tournament
{
    public abstract class Client : MonoBehaviour
    {
        [SerializeField] protected string Id;

        protected PlayerInfo Info;
        protected string CurrentOpponentId;
        protected ClientState CurrentState = ClientState.Undefined;
        protected RoundInfo Tournament;
        protected MatchInfo CurrentMatch;

        private void Awake ()
        {
            NetworkManager.OnEventReceived += HandleEventReceived;
            OnAwake();
        }

        private void OnDestroy ()
        {
            NetworkManager.OnEventReceived -= HandleEventReceived;
            UponDestroy();
        }

        protected virtual void OnAwake () { }

        protected virtual void UponDestroy () { }

        protected void HandleEventReceived (string key, object[] parameters)
        {
            this.Log("Event received in client: " + key);
            Dictionary<string, object> paramDict = parameters.ToDictionary();
            switch (key)
            {
                case Keys.TournamentUpdateEvt:
                    if (paramDict.TryGetValue(Keys.TournamentInfoKey, out object value))
                    {
                        this.Log($"Tournament received:     {value}");
                        RoundInfo tournament = JsonConvert.DeserializeObject<RoundInfo>(value.ToString());
                        SetRound(tournament);
                    }
                    else
                        this.LogWarning("Didn't receive the key " + Keys.TournamentInfoKey);
                    break;
                case Keys.TurnUpdateEvt:

                    break;
            }
        }

        public virtual void SetInfo (PlayerInfo info)
        {
            Info = info;
            Id = info.Id;
        }

        public virtual void SetRound (RoundInfo tournamentInfo)
        {
            Tournament = tournamentInfo;
            foreach (var item in Tournament.Matches)
            {
                if (item.Player1.Id == Id)
                {
                    CurrentMatch = item;
                    CurrentOpponentId = item.Player2?.Id ?? "";
                }
                else if (item.Player2 != null && item.Player2.Id == Id)
                {
                    CurrentMatch = item;
                    CurrentOpponentId = item.Player1.Id;
                }
            }
        }

        public void SetReadyInGame ()
        {
            CurrentState = ClientState.GameReady;
            NetworkManager.Instance.SendData($"{Keys.ClientIdKey}-{Id}", (int)CurrentState);
        }

        public void SetReadyToStartMatch ()
        {
            CurrentState = ClientState.MatchReady;
            NetworkManager.Instance.SendData($"{Keys.ClientIdKey}-{Id}", (int)CurrentState);
        }
    }
}
