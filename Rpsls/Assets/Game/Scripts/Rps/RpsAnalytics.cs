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
        [SerializeField] private SignalString playerNickname;
        [SerializeField] private SignalInt playerAvatarSignal;
        [SerializeField] private SignalInt rockCardSignal;
        [SerializeField] private SignalInt paperCardSignal;
        [SerializeField] private SignalInt scissorsCardSignal;
        [SerializeField] private Signal onPlayButtonClicked;
        [SerializeField] private Signal onReconnectButtonClicked;
        [SerializeField] private Signal onCancelButtonClicked;
        [SerializeField] private SignalBool onMatchFoundSuccess;
        [SerializeField] private Signal onGameSceneLoaded;
        [SerializeField] private SignalBool onWaitingOpponent;
        [SerializeField] private SignalBool onOpponentLeft;
        [SerializeField] private SignalBool onMatchStarted;
        [SerializeField] private SignalBool onMatchEnded;
        [SerializeField] private SignalState matchWinner;
        [SerializeField] private Signal onSendButtonClicked;
        [SerializeField] private SignalState myMove;
        [SerializeField] private Signal onLeaveButtonClicked;
        [SerializeField] private SignalState myScore;
        [SerializeField] private SignalState opponentScore;

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

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy ()
        {
            UnsubscribeToSignals();
        }

        private void SubscribeToSignals ()
        {
            onConnectionSuccessSignal.OnSignalEmitted.AddListener(HandleConnectionSuccess);
            onConnectionFailureSignal.OnSignalEmitted.AddListener(HandleConnectionFailure);
            onChangeAvatarScreenOpened.OnSignalEmittedWithParam.AddListener(HandleChangeAvatarScreenOpened);
            onChangeRockScreenOpened.OnSignalEmittedWithParam.AddListener(HandleChangeRockScreenOpened);
            onChangePaperScreenOpened.OnSignalEmittedWithParam.AddListener(HandleChangePaperScreenOpened);
            onChangeScissorsScreenOpened.OnSignalEmittedWithParam.AddListener(HandleChangeScissorsScreenOpened);
            onChangeNicknameSignal.OnSignalEmitted.AddListener(HandleChangeNickname);
            playerAvatarSignal.OnSignalEmittedWithParam.AddListener(HandleAvatarChanged);
            rockCardSignal.OnSignalEmittedWithParam.AddListener(HandleRockCardChanged);
            paperCardSignal.OnSignalEmittedWithParam.AddListener(HandlePaperCardChanged);
            scissorsCardSignal.OnSignalEmittedWithParam.AddListener(HandleScissorsCardChanged);
            onPlayButtonClicked.OnSignalEmitted.AddListener(HandlePlayButtonClicked);
            onReconnectButtonClicked.OnSignalEmitted.AddListener(HandleReconnectButtonClicked);
            onCancelButtonClicked.OnSignalEmitted.AddListener(HandleCancelButtonClicked);
            onMatchFoundSuccess.OnSignalEmittedWithParam.AddListener(HandleMatchFound);
            onGameSceneLoaded.OnSignalEmitted.AddListener(HandleGameSceneLoaded);
            onWaitingOpponent.OnSignalEmittedWithParam.AddListener(HandleWaitingOpponent);
            onOpponentLeft.OnSignalEmittedWithParam.AddListener(HandleOpponentLeft);
            onMatchStarted.OnSignalEmittedWithParam.AddListener(HandleMatchStarted);
            onMatchEnded.OnSignalEmittedWithParam.AddListener(HandleMatchEnded);
            onSendButtonClicked.OnSignalEmitted.AddListener(HandleSendButtonClicked);
            onLeaveButtonClicked.OnSignalEmitted.AddListener(HandleLeaveButtonClicked);
        }
        private void UnsubscribeToSignals ()
        {
            onConnectionSuccessSignal.OnSignalEmitted.RemoveListener(HandleConnectionSuccess);
            onConnectionFailureSignal.OnSignalEmitted.RemoveListener(HandleConnectionFailure);
            onChangeAvatarScreenOpened.OnSignalEmittedWithParam.RemoveListener(HandleChangeAvatarScreenOpened);
            onChangeRockScreenOpened.OnSignalEmittedWithParam.RemoveListener(HandleChangeRockScreenOpened);
            onChangePaperScreenOpened.OnSignalEmittedWithParam.RemoveListener(HandleChangePaperScreenOpened);
            onChangeScissorsScreenOpened.OnSignalEmittedWithParam.RemoveListener(HandleChangeScissorsScreenOpened);
            onChangeNicknameSignal.OnSignalEmitted.RemoveListener(HandleChangeNickname);
            playerAvatarSignal.OnSignalEmittedWithParam.RemoveListener(HandleAvatarChanged);
            rockCardSignal.OnSignalEmittedWithParam.RemoveListener(HandleRockCardChanged);
            paperCardSignal.OnSignalEmittedWithParam.RemoveListener(HandlePaperCardChanged);
            scissorsCardSignal.OnSignalEmittedWithParam.RemoveListener(HandleScissorsCardChanged);
            onPlayButtonClicked.OnSignalEmitted.RemoveListener(HandlePlayButtonClicked);
            onReconnectButtonClicked.OnSignalEmitted.RemoveListener(HandleReconnectButtonClicked);
            onCancelButtonClicked.OnSignalEmitted.RemoveListener(HandleCancelButtonClicked);
            onMatchFoundSuccess.OnSignalEmittedWithParam.RemoveListener(HandleMatchFound);
            onGameSceneLoaded.OnSignalEmitted.RemoveListener(HandleGameSceneLoaded);
            onWaitingOpponent.OnSignalEmittedWithParam.RemoveListener(HandleWaitingOpponent);
            onOpponentLeft.OnSignalEmittedWithParam.RemoveListener(HandleOpponentLeft);
            onMatchStarted.OnSignalEmittedWithParam.RemoveListener(HandleMatchStarted);
            onMatchEnded.OnSignalEmittedWithParam.RemoveListener(HandleMatchEnded);
            onSendButtonClicked.OnSignalEmitted.RemoveListener(HandleSendButtonClicked);
            onLeaveButtonClicked.OnSignalEmitted.RemoveListener(HandleLeaveButtonClicked);
        }

        private void HandleConnectionSuccess ()
        {
            AnalyticsController.SendEvent(CONNECTION_SUCCESS);
        }

        private void HandleConnectionFailure ()
        {
            AnalyticsController.SendEvent(CONNECTION_FAILURE);
        }

        private void HandleChangeAvatarScreenOpened (bool isOpen)
        {
            AnalyticsController.SendUniqueEvent(BTN_CHANGE_AVATAR);
        }

        private void HandleChangeRockScreenOpened (bool isOpen)
        {
            AnalyticsController.SendUniqueEvent(BTN_CHANGE_ROCK);
        }

        private void HandleChangePaperScreenOpened (bool isOpen)
        {
            AnalyticsController.SendUniqueEvent(BTN_CHANGE_PAPER);
        }

        private void HandleChangeScissorsScreenOpened (bool isOpen)
        {
            AnalyticsController.SendUniqueEvent(BTN_CHANGE_SCISSORS);
        }

        private void HandleChangeNickname ()
        {
            AnalyticsController.SendEventWithString(BTN_CHANGE_NICKNAME, playerNickname.Value);
        }

        private void HandleAvatarChanged (int index)
        {
            if (onChangeAvatarScreenOpened.Value)
                AnalyticsController.SendEventWithNumber(SELECTED_AVATAR, index);
        }

        private void HandleRockCardChanged (int index)
        {
            if (onChangeRockScreenOpened.Value)
                AnalyticsController.SendEventWithNumber(SELECTED_ROCK, index);
        }

        private void HandlePaperCardChanged (int index)
        {
            if (onChangePaperScreenOpened.Value)
                AnalyticsController.SendEventWithNumber(SELECTED_PAPER, index);
        }

        private void HandleScissorsCardChanged (int index)
        {
            if (onChangeScissorsScreenOpened.Value)
                AnalyticsController.SendEventWithNumber(SELECTED_SCISSORS, index);
        }

        private void HandlePlayButtonClicked ()
        {
            int totalPlayButtonClicks = Storage.Load(TOTAL_CLICKS_PLAY, 0) + 1;
            Storage.Save(TOTAL_CLICKS_PLAY, totalPlayButtonClicks);
            AnalyticsController.SendEventWithNumber(BTN_PLAY, totalPlayButtonClicks);
            int sessionPlayButtonClicks = Storage.Load(SESSION_CLICKS_PLAY, 0) + 1;
            Storage.Save(SESSION_CLICKS_PLAY, sessionPlayButtonClicks);
            AnalyticsController.SendEventWithNumber(BTN_PLAY_SESSION, sessionPlayButtonClicks);
        }

        private void HandleReconnectButtonClicked ()
        {
            AnalyticsController.SendEvent(BTN_RECONNECT);
        }

        private void HandleCancelButtonClicked ()
        {
            AnalyticsController.SendEvent(BTN_CANCEL);
        }

        private void HandleMatchFound (bool active)
        {
            if (active)
                AnalyticsController.SendEvent(MATCH_FOUND);
        }

        private void HandleGameSceneLoaded ()
        {
            AnalyticsController.SendEvent(GAME_SCENE_LOADED);
        }

        private void HandleWaitingOpponent (bool active)
        {
            if (active)
                AnalyticsController.SendEvent(WAITING_PLAYER);
        }

        private void HandleOpponentLeft (bool active)
        {
            if (active)
                AnalyticsController.SendEvent(OPPONENT_LEFT);
        }

        private void HandleMatchStarted (bool active)
        {
            if (active)
            {
                int totalMatches = Storage.Load(TOTAL_MATCHES, 0) + 1;
                Storage.Save(TOTAL_MATCHES, totalMatches);
                AnalyticsController.SendEventWithNumber(MATCH_STARTED, totalMatches);
            }
        }

        private void HandleMatchEnded (bool active)
        {
            if (active)
                AnalyticsController.SendEventWithString(MATCH_ENDED, matchWinner.Value);
        }

        private void HandleSendButtonClicked ()
        {
            AnalyticsController.SendEventWithString(BTN_SEND, myMove.Value);
        }

        private void HandleLeaveButtonClicked ()
        {
            AnalyticsController.SendEventWithString(BTN_LEAVE, $"{myScore.Value}-{opponentScore.Value}");
        }
    }
}