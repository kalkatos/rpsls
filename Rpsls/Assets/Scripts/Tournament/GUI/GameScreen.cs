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
        [Header("Config")]
        [SerializeField] private TournamentGameSettings settings;

        private bool isHandReceived;
        private bool isMatchOver;

        private void Awake ()
        {
            TournamentScreen.OnTournamentIntroFinished += HandleTournamentIntroFinished;
            GameManager.OnHandReceived += HandleHandReceived;
            exitButton.onClick.AddListener(OnExitButtonClicked);
            settings = TournamentGameSettings.Instance;
        }

        private void OnDestroy ()
        {
            TournamentScreen.OnTournamentIntroFinished -= HandleTournamentIntroFinished;
            GameManager.OnHandReceived -= HandleHandReceived;
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        private void HandleTournamentIntroFinished ()
        {
            Debug.Log("Tournament Intro Finished");
            StartCoroutine(GameSetupAnimations());
        }

        private void HandleHandReceived ()
        {
            isHandReceived = true;
        }

        private IEnumerator GameSetupAnimations ()
        {
            GameManager.Instance.SetStateAsInMatch();
            
            yield return new WaitForSeconds(1f);

            //TODO Create and place the cards

            // Send ready event Action
            // Wait for other player ready
            StartCoroutine(TurnLoop());
        }

        private IEnumerator  TurnLoop ()
        {
            yield return new WaitForSeconds(0.5f);
            GameManager.Instance.SetStateAsInTurn();
            // Do while no one has won:
            while (!isMatchOver)
            {
                // Wait for turn info (hand)
                while (!isHandReceived)
                    yield return null;

                //TODO      Setup and move correct cards to hand

                // Start the clock
                float time = settings.TurnDuration;
                float startTime = Time.time;
                timerBarObj.SetActive(true);
                timerBar.anchorMax.Set(1, 1);
                float elapsed = 0;
                while (elapsed < time)
                {
                    elapsed = Time.time - startTime;
                    timerBar.anchorMax.Set(1 - elapsed / time, 1);
                    yield return null;
                }
                timerBarObj.SetActive(false);

                //TODO      On clock ends block input

                // Wait for turn resolution
                GameManager.Instance.SetStateAsWaitingForResult();


                //TODO      Present the result
                //TODO      Wait for other player confirmation
                isHandReceived = false;
                yield return null;
            }
        }

        private void OnExitButtonClicked ()
        {
            GameManager.ExitRoom();
        }
    }
}
