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
        [SerializeField, ChildGameObjectsOnly] private Button secondExitButton;
        [Header("Config")]
        [SerializeField] private TournamentGameSettings settings;

        private bool isHandReceived;
        private bool isTurnResultReceived;
        private bool isMatchOver;

        private void Awake ()
        {
            TournamentScreen.OnTournamentIntroFinished += HandleTournamentIntroFinished;
            TournamentScreen.OnTournamentOutroFinished += HandleTournamentOutroFinished;
            GameManager.OnHandReceived += HandleHandReceived;
            GameManager.OnMyMatchResultReceived += HandleMyMatchResultReceived;
            exitButton.onClick.AddListener(OnExitButtonClicked);
            secondExitButton.onClick.AddListener(OnExitButtonClicked);
            settings = TournamentGameSettings.Instance;
            secondExitButton.gameObject.SetActive(false);
        }

        private void OnDestroy ()
        {
            TournamentScreen.OnTournamentIntroFinished -= HandleTournamentIntroFinished;
            TournamentScreen.OnTournamentOutroFinished -= HandleTournamentOutroFinished;
            GameManager.OnHandReceived -= HandleHandReceived;
            GameManager.OnMyMatchResultReceived -= HandleMyMatchResultReceived;
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
            secondExitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        private void HandleTournamentIntroFinished ()
        {
            StartCoroutine(GameSetupAnimations());
        }

        private void HandleHandReceived ()
        {
            isHandReceived = true;
        }

        private void HandleMyMatchResultReceived (MatchInfo matchInfo)
        {
            isTurnResultReceived = true;
            isMatchOver = matchInfo.IsOver;
        }

        private void HandleTournamentOutroFinished ()
        {
            exitButton.gameObject.SetActive(false);
            secondExitButton.gameObject.SetActive(true);
        }

        private IEnumerator GameSetupAnimations ()
        {
            GameManager.Instance.SetStateAsInMatch();
            
            yield return new WaitForSeconds(1f);

            //TODO Create and place the cards

            StartCoroutine(TurnLoop());
        }

        private IEnumerator  TurnLoop ()
        {
            yield return new WaitForSeconds(0.5f);
            // Do while no one has won:
            isMatchOver = false;
            while (!isMatchOver)
            {
                GameManager.Instance.SetStateAsInTurn();
                // Wait for turn info (hand)
                isHandReceived = false;
                while (!isHandReceived)
                    yield return null;
                yield return new WaitForSeconds(0.5f);
                GameManager.Instance.SetStateAsHandReceived();

                //TODO      Setup and move correct cards to hand

                // Start the clock
                float time = settings.TurnDuration;
                float startTime = Time.time;
                timerBarObj.SetActive(true);
                timerBar.anchorMax = new Vector2(1, 1);
                float elapsed = 0;
                while (elapsed < time)
                {
                    elapsed = Time.time - startTime;
                    timerBar.anchorMax = new Vector2(1 - elapsed / time, 1);
                    yield return null;
                }
                timerBarObj.SetActive(false);

                //TODO      On clock ends block input

                // Wait for turn resolution
                GameManager.Instance.SetStateAsWaitingForResult();
                isTurnResultReceived = false;
                while (!isTurnResultReceived)
                    yield return null;
                yield return new WaitForSeconds(0.5f);

                if (isMatchOver)
                {
                    // TODO End match
                    this.Log("Match ended for me.");
                }
                else
                {
                    //TODO      Present the result  <<<
                }

                //TODO      Wait for other player confirmation

                yield return null;
            }
        }

        private void OnExitButtonClicked ()
        {
            GameManager.ExitRoom();
        }
    }
}
