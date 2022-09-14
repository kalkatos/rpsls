using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Kalkatos.Tournament
{
    public class GameScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject timerBarObj;
        [SerializeField] private RectTransform timerBar;
        [SerializeField, ChildGameObjectsOnly] private Button exitButton;
        [Header("Positions")]
        [SerializeField] private Transform[] playmatPositions;
        [Header("Config")]
        [SerializeField] private TournamentGameSettings settings;

        private void Awake ()
        {
            TournamentScreen.OnTournamentIntroFinished += HandleTournamentIntroFinished;
            exitButton.onClick.AddListener(OnExitButtonClicked);
            settings = TournamentGameSettings.Instance;
        }

        private void OnDestroy ()
        {
            TournamentScreen.OnTournamentIntroFinished -= HandleTournamentIntroFinished;
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        private void HandleTournamentIntroFinished ()
        {
            Debug.Log("Tournament Intro Finished");
            StartCoroutine(GameSetupAnimations());
        }

        private IEnumerator GameSetupAnimations ()
        {
            yield return new WaitForSeconds(0.5f);
            //TODO Create and place playmats
            //TODO Create and place the cards

            // Send ready event Action
            GameManagerClient.Instance.SetReadyToStartMatch();
            // Wait for other player ready
            StartCoroutine(TurnLoop());
        }

        private IEnumerator  TurnLoop ()
        {
            yield return new WaitForSeconds(0.5f);
            //TODO Do while no one has won:
            //TODO      Wait for turn info (hand)
            //TODO      Setup and move correct cards to hand
            //TODO      Wait signal of the start of the turn
            //TODO      Start the clock
            //TODO      On clock ends block input
            //TODO      Wait for turn resolution
            //TODO      Present the result
            //TODO      Wait for other player confirmation
        }

        private void OnExitButtonClicked ()
        {
            GameManagerClient.ExitRoom();
        }
    }
}
