﻿using System.Collections.Generic;
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
        protected string currentState = ClientState.Undefined.ToString();
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
            this.Log("Event received in client: " + key);
            Dictionary<string, object> paramDict = parameters.ToDictionary();
            object value = null;
            switch (key)
            {
                case Keys.RoundReceivedEvt:
                    if (paramDict.TryGetValue(Keys.RoundKey, out value))
                    {
                        this.Log($"Round received:     {value}");
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
                        SetStateAsHandReceived();
                    });
                    break;
                case Keys.TurnEndedEvt:
                    if (paramDict.TryGetValue(Keys.RoundKey, out value))
                    {
                        this.Log($"Round received at turn End:     {value}");
                        RoundInfo round = JsonConvert.DeserializeObject<RoundInfo>(value.ToString());
                        HandleTurnResult(round);
                    }
                    else
                        this.LogWarning("Didn't receive the key " + Keys.RoundKey);
                    break;
            }
        }

        protected void SetState (string state)
        {
            currentState = state;
            //NetworkManager.Instance.SendCustomData($"{Keys.PlayerStatusKey}-{Id}", (int)currentState);
            NetworkManager.Instance.UpdateMyCustomData(Keys.PlayerStatusKey, currentState);
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
    }
}