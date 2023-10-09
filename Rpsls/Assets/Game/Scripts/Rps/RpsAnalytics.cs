using Kalkatos.Analytics;
using Kalkatos.Analytics.Unity;
using Kalkatos.UnityGame.Scriptable;
using System;
using UnityEngine;

namespace Kalkatos.UnityGame.Rps
{
    [DefaultExecutionOrder(-1)]
    public class RpsAnalytics : MonoBehaviour
    {
        [SerializeField] private Signal onConnectionSuccessSignal;
        [SerializeField] private Signal onConnectionFailureSignal;
        [SerializeField] private SignalBool onChangeAvatarScreenOpened;
        [SerializeField] private SignalBool onChangeRockScreenOpened;
        [SerializeField] private SignalBool onChangePaperScreenOpened;
        [SerializeField] private SignalBool onChangeScissorsScreenOpened;
        [SerializeField] private Signal onChangeNicknameSignal;

        // Analytics event handles
        private const string SESSION_START = "session_start";
        private const string COUNT_NEW_DAY = "count_new_day";
		private const string CONNECTION_FAILURE = "connection_failure";
		private const string CONNECTION_SUCCESS = "connection_success";
		private const string BTN_CHANGE_AVATAR = "btn_change_avatar";
		private const string BTN_CHANGE_ROCK = "btn_change_rock";
		private const string BTN_CHANGE_PAPER = "btn_change_paper";
		private const string BTN_CHANGE_SCISSORS = "btn_change_scissors";
		private const string BTN_CHANGE_NICKNAME = "btn_change_nickname";
		private const string SELECTED_AVATAR = "selected_avatar";
		private const string SELECTED_ROCK = "selected_rock";
		private const string SELECTED_PAPER = "selected_paper";
		private const string SELECTED_SCISSORS = "selected_scissors";
		private const string BTN_PLAY = "btn_play";
		private const string BTN_PLAY_SESSION = "btn_play_session";
		private const string BTN_RECONNECT = "btn_reconnect";
		private const string BTN_CANCEL = "btn_cancel";
		private const string MATCH_FOUND = "match_found";
		private const string GAME_SCENE_LOADED = "game_scene_loaded";
		private const string WAITING_PLAYER = "waiting_player";
		private const string OPPONENT_LEFT = "opponent_left";
		private const string MATCH_STARTED = "match_started";
		private const string MATCH_ENDED = "match_ended";
		private const string BTN_SEND = "btn_send";
		private const string BTN_LEAVE = "btn_leave";
        // Storage save handles
        private const string LAST_PLAYED_DAY = "LastPlayedDay";
        private const string PLAYED_DAYS_COUNT = "PlayedDaysCount";
        private const string TOTAL_CLICKS_PLAY = "TotalClicksPlay";
        private const string SESSION_CLICKS_PLAY = "SessionClicksPlay";
        private const string TOTAL_MATCHES = "TotalMatches";

        private void Awake ()
        {
            AnalyticsController.Initialize(new GameAnalyticsSender());
            AnalyticsController.SendEvent(SESSION_START);
            long currentDay = DateTime.Today.Ticks;
            long savedDay = long.Parse(Storage.Load(LAST_PLAYED_DAY, "0"));
            int dayCount = Storage.Load(PLAYED_DAYS_COUNT, -1);
            bool isNewDay = currentDay != savedDay;
            if (isNewDay)
            {
                AnalyticsController.SendEventWithNumber(COUNT_NEW_DAY, ++dayCount);
                Storage.Save(LAST_PLAYED_DAY, currentDay.ToString());
                Storage.Save(PLAYED_DAYS_COUNT, dayCount);
                Storage.Delete(SESSION_CLICKS_PLAY);
            }
            SubscribeToSignals();
        }

        private void SubscribeToSignals ()
        {
            onConnectionSuccessSignal?.OnSignalEmitted.AddListener(HandleConnectionSuccess);
            onConnectionFailureSignal?.OnSignalEmitted.AddListener(HandleConnectionFailure);
            onChangeAvatarScreenOpened?.OnSignalEmittedWithParam.AddListener(HandleChangeAvatarScreenOpened);
            onChangeRockScreenOpened?.OnSignalEmittedWithParam.AddListener(HandleChangeRockScreenOpened);
            onChangePaperScreenOpened?.OnSignalEmittedWithParam.AddListener(HandleChangePaperScreenOpened);
            onChangeScissorsScreenOpened?.OnSignalEmittedWithParam.AddListener(HandleChangeScissorsScreenOpened);
            onChangeNicknameSignal?.OnSignalEmitted.AddListener(HandleChangeNickname);
        }

        private void UnsubscribeToSignals ()
        {
            onConnectionSuccessSignal?.OnSignalEmitted.RemoveListener(HandleConnectionSuccess);
            onConnectionFailureSignal?.OnSignalEmitted.RemoveListener(HandleConnectionFailure);
            onChangeAvatarScreenOpened?.OnSignalEmittedWithParam.RemoveListener(HandleChangeAvatarScreenOpened);
            onChangeRockScreenOpened?.OnSignalEmittedWithParam.RemoveListener(HandleChangeRockScreenOpened);
            onChangePaperScreenOpened?.OnSignalEmittedWithParam.RemoveListener(HandleChangePaperScreenOpened);
            onChangeScissorsScreenOpened?.OnSignalEmittedWithParam.RemoveListener(HandleChangeScissorsScreenOpened);
            onChangeNicknameSignal?.OnSignalEmitted.RemoveListener(HandleChangeNickname);
        }

        private void HandleConnectionSuccess ()
        {
            
        }

        private void HandleConnectionFailure ()
        {
            
        }

        private void HandleChangeAvatarScreenOpened (bool isOpen)
        {
            
        }

        private void HandleChangeRockScreenOpened (bool isOpen)
        {
            
        }

        private void HandleChangePaperScreenOpened (bool isOpen)
        {
            
        }

        private void HandleChangeScissorsScreenOpened (bool isOpen)
        {
            
        }

        private void HandleChangeNickname ()
        {
            
        }
    }
}