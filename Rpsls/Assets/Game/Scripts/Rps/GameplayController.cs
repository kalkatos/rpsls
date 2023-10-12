using Kalkatos.Firecard.Unity;
using Kalkatos.Network.Model;
using Kalkatos.Network.Unity;
using Kalkatos.UnityGame.Scriptable;
using Kalkatos.UnityGame.Scriptable.Network;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kalkatos.UnityGame.Rps
{
	public class GameplayController : MonoBehaviour
	{
		[Header("Config")]
		[SerializeField] private int opponentTimeout = 20;
		[SerializeField] private int delayBeforeWaitScreenPopup = 4;
        [SerializeField] private Vector2 minMaxWaitTime = new(1, 2);
        [Header("References")]
		[SerializeField] private Signal onGameSceneLoaded;
		[SerializeField] private StateBuilder stateBuilder;
		[SerializeField] private ScreenSignal menuScreen;
		[SerializeField] private SignalBool waitingOpponentScreen;
		[SerializeField] private SignalString waitingOpponentCountdown;
		[SerializeField] private SignalBool waitingOpponentFailedScreen;
		[SerializeField] private Signal onSendButtonClicked;
		[SerializeField] private Signal returnAllToOrigin;
		[SerializeField] private SignalBool gameplayScreen;
		[SerializeField] private SignalFloat turnTimer;
		[SerializeField] private SignalBool turnTimerControl;
		[SerializeField] private SignalBool matchEndedScreen;
		[SerializeField] private SignalBool hasSentMove;
		[SerializeField] private SignalState myMove;
		[SerializeField] private Signal onLeaveMatchSuccess;
		[Header("TESTS")]
		[SerializeField] private bool autoPlay;
		[SerializeField] private bool fixedDelay;
		[SerializeField] private CardBehaviour rockCard;
		[SerializeField] private CardBehaviour paperCard;
		[SerializeField] private CardBehaviour scissorsCard;
		[SerializeField] private ScreenSignal menuScreenSignal;

		private string phase = "0";
		private StateInfo currentState = null;
		private Coroutine countdownCoroutine;
		private bool hasExecutedTurnResult;
		private float endPlayPhaseTime = float.MaxValue;
		private AutoPlaySettings settings;

		private void Awake ()
		{
			onSendButtonClicked?.OnSignalEmitted.AddListener(HandleSendButtonClicked);
			onLeaveMatchSuccess?.OnSignalEmitted.AddListener(HandleLeaveMatch);
		}

		private void Start ()
		{
			settings = AutoPlaySettings.Instance;
			autoPlay = settings?.AutoPlay ?? false;
			onGameSceneLoaded?.Emit();
			StartCoroutine(GameplayLoop());
		}

		private void OnDestroy ()
		{
			onSendButtonClicked?.OnSignalEmitted.RemoveListener(HandleSendButtonClicked);
			onLeaveMatchSuccess?.OnSignalEmitted.RemoveListener(HandleLeaveMatch);
		}

		private void Update ()
		{
			if (Input.GetKeyDown(KeyCode.M))
				menuScreenSignal?.EmitWithParam(true);
			if (Input.GetKeyDown(KeyCode.D))
				autoPlay = !autoPlay;

		}

		private IEnumerator GameplayLoop ()
		{
			yield return null;
			Logger.Log(" ========= Send Handshaking =========");
			ActionInfo handshakingAction = new ActionInfo { PrivateChanges = new Dictionary<string, string> { { "Handshaking", "1" } } };
			NetworkClient.SendAction(handshakingAction, null, null);
			yield return new WaitForSeconds(Random.Range(minMaxWaitTime.x, minMaxWaitTime.y));

			yield return WaitMatchStateSimple();

			if (currentState == null)
			{
                Logger.Log(" ========= Wait for opponent =========");
				countdownCoroutine = StartCoroutine(CountdownCoroutine(30));
				float handshakingWaitStart = Time.time;
				bool hasHandshakingFromBothPlayers = false;
				waitingOpponentScreen?.EmitWithParam(true);
				while (!hasHandshakingFromBothPlayers)
				{
					NetworkClient.GetMatchState(
						(success) =>
						{
							hasHandshakingFromBothPlayers = true;
							currentState = success;
							if (countdownCoroutine != null)
								StopCoroutine(countdownCoroutine);
							waitingOpponentCountdown?.EmitWithParam("0");
							stateBuilder.ReceiveState(currentState);
						}, null);
					if (Time.time - handshakingWaitStart >= 30)
					{
						waitingOpponentScreen?.EmitWithParam(false);
						waitingOpponentFailedScreen?.EmitWithParam(true);
						yield return new WaitForSeconds(4);
						menuScreen?.EmitWithParam(true);
						yield break;
					}
					else
                        yield return new WaitForSeconds(Random.Range(minMaxWaitTime.x, minMaxWaitTime.y));
                }
				waitingOpponentScreen?.EmitWithParam(false);
			}
			else
			{
				stateBuilder.ReceiveState(currentState);
				Logger.Log($" : : > >  State {JsonConvert.SerializeObject(currentState, Formatting.Indented)}");
			}

			gameplayScreen?.EmitWithParam(true);
			hasSentMove.EmitWithParam(currentState != null && currentState.PrivateProperties["MyMove"] != "");
			Logger.Log(" ========= Turn Logic =========");
			phase = currentState?.PublicProperties["Phase"] ?? "0";
			while (true)
			{
				endPlayPhaseTime = Time.time;
				switch (phase)
				{
					case "0":
					case "1":
						if (hasSentMove.Value)
							break;
						returnAllToOrigin?.Emit();
						// TODO Countdown before the bar
						yield return new WaitForSeconds(3);
						Logger.Log($"Phase: 1 | UTC: {DateTime.UtcNow.ToString("u")} | State: \n{JsonConvert.SerializeObject(currentState, Formatting.Indented)}");
						endPlayPhaseTime = Time.time + 10;
						turnTimerControl.EmitWithParam(true);
						StartCoroutine(TurnTimerCoroutine(10));

						// DEBUG
						if (autoPlay)
							StartCoroutine(RandomMove());

						while (!hasSentMove.Value && Time.time < endPlayPhaseTime)
							yield return null;
						hasExecutedTurnResult = false;
						yield return new WaitForSeconds(1f);
						if (!hasSentMove.Value)
						{
							myMove.EmitWithParam("NOTHING");
							onSendButtonClicked.Emit();
							turnTimerControl.EmitWithParam(false);
						}
						break;
					case "2":
					case "3":
						if (hasExecutedTurnResult)
							break;
						turnTimerControl.EmitWithParam(false);
						hasSentMove?.EmitWithParam(false);
						Logger.Log($"Phase: 2 | UTC: {DateTime.UtcNow.ToString("u")} | State: \n{JsonConvert.SerializeObject(currentState, Formatting.Indented)}");
						yield return new WaitForSeconds(3);
						hasExecutedTurnResult = true;
						break;
					default:
						break;
				}
				if (phase == "3")
					break;
				yield return WaitMatchState();
				phase = currentState.PublicProperties["Phase"];
			}
			// Match is over
			if (waitingOpponentFailedScreen.Value)
				waitingOpponentFailedScreen.EmitWithParam(false);
			matchEndedScreen?.EmitWithParam(true);
			yield return new WaitForSeconds(5);
			menuScreen.EmitWithParam(true);
		}

        private IEnumerator WaitMatchStateSimple ()
		{
			bool hasResponse = false;
            NetworkClient.GetMatchState(
                success =>
                {
                    currentState = success;
                    hasResponse = true;
                },
                failure =>
                {
                    hasResponse = true;
                    if (failure.Tag == NetworkErrorTag.NotFound)
                        menuScreen.EmitWithParam(true);
                });
			while (!hasResponse)
				yield return null;
		}


        private IEnumerator WaitMatchState ()
		{
			float timeForScreenPopup = endPlayPhaseTime + delayBeforeWaitScreenPopup;
			float timeForOpponentTimeout = timeForScreenPopup + opponentTimeout;
			while (true)
			{
				bool hasResponse = false;
				int lastStateHash = currentState?.Hash ?? 0;
				NetworkClient.GetMatchState(
					success =>
					{ 
						currentState = success;
						hasResponse = true; 
					}, 
					failure => 
					{ 
						hasResponse = true;
						if (failure.Tag == NetworkErrorTag.NotFound)
							menuScreen.EmitWithParam(true);
					});
				while (!hasResponse)
					yield return null;
				if (currentState != null && currentState.Hash != lastStateHash)
				{
					if (waitingOpponentScreen.Value)
					{
						waitingOpponentScreen.EmitWithParam(false);
						if (countdownCoroutine != null)
							StopCoroutine(countdownCoroutine);
					}
					break;
				}
				else if (Time.time > endPlayPhaseTime)
				{
					if (!waitingOpponentScreen.Value)
					{
						if (Time.time >= timeForScreenPopup)
						{
							if (turnTimerControl.Value)
								turnTimerControl.EmitWithParam(false);
							waitingOpponentScreen.EmitWithParam(true);
							countdownCoroutine = StartCoroutine(CountdownCoroutine(opponentTimeout));
						}
					}
					else if (Time.time >= timeForOpponentTimeout)
					{
						waitingOpponentScreen.EmitWithParam(false);
						if (countdownCoroutine != null)
							StopCoroutine(countdownCoroutine);
						waitingOpponentFailedScreen.EmitWithParam(true);
						yield return new WaitForSeconds(delayBeforeWaitScreenPopup);
						break;
					}
				}
                yield return new WaitForSeconds(Random.Range(minMaxWaitTime.x, minMaxWaitTime.y));
            }
			stateBuilder.ReceiveState(currentState);
		}

		private IEnumerator CountdownCoroutine (int time)
		{
			WaitForSeconds wait1s = new WaitForSeconds(1);
			waitingOpponentCountdown?.EmitWithParam(time.ToString());
			while (time > 0)
			{
				yield return wait1s;
				time--;
				waitingOpponentCountdown?.EmitWithParam(time.ToString());
			}
		}

		private IEnumerator TurnTimerCoroutine (float time)
		{
			turnTimer.EmitWithParam(1f);
			while (time > 0)
			{
				if (!turnTimerControl.Value)
					yield break;
				time = Mathf.Max(time - Time.deltaTime, 0);
				turnTimer.EmitWithParam(Mathf.Clamp01(time / 10));
				yield return null;
			}
		}

		private void HandleSendButtonClicked ()
		{
			hasSentMove?.EmitWithParam(true);
		}

		private void HandleLeaveMatch ()
		{
			StopAllCoroutines();
			StartCoroutine(LeaveMatch());
		}

		private IEnumerator LeaveMatch ()
		{
			yield return new WaitForSeconds(5);
			menuScreen.EmitWithParam(true);
		}

		// DEBUG
		private IEnumerator RandomMove ()
		{
			yield return new WaitForSeconds(fixedDelay ? 2f : Random.Range(1.5f, 3.5f));
			int move = Random.Range(0, 3);
			switch (move)
			{
				case 0:
					rockCard.Use();
					break;
				case 1:
					paperCard.Use();
					break;
				case 2:
					scissorsCard.Use();
					break;
			}
			yield return new WaitForSeconds(fixedDelay ? 1f : Random.Range(0.5f, 1.5f));
			onSendButtonClicked.Emit();
		}
	}
}