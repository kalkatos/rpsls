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
		[SerializeField] private StateBuilder stateBuilder;
		[Header("Handshaking")]
		[SerializeField] private ScreenSignal menuScreen;
		[SerializeField] private SignalBool waitingOpponentScreen;
		[SerializeField] private SignalString waitingOpponentCountdown;
		[SerializeField] private SignalBool waitingOpponentFailedScreen;
		[Header("Gameplay")]
		[SerializeField] private Signal onSendButtonClicked;
		[SerializeField] private SignalBool gameplayScreen;
		[SerializeField] private SignalFloat turnTimer;
		[SerializeField] private SignalBool turnTimerControl;
		[SerializeField] private SignalBool matchEndedScreen;
		[SerializeField] private SignalBool hasSentMove;
		[Header("DEBUG")]
		[SerializeField] private bool autoPlay;
		[SerializeField] private SignalState myMoveSignal;

		private string phase = "0";
		private StateInfo currentState = null;
		private Coroutine countdownCoroutine;

		private void Awake ()
		{
			onSendButtonClicked?.OnSignalEmitted.AddListener(HandleSendButtonClicked);
		}

		private void Start ()
		{
			Logger.Log("Gameplay Controller Starts");
			StartCoroutine(GameplayLoop());
		}

		private void OnDestroy ()
		{
			onSendButtonClicked?.OnSignalEmitted.RemoveListener(HandleSendButtonClicked);
		}

		private IEnumerator GameplayLoop ()
		{
			Logger.Log(" ========= Send Handshaking =========");
			waitingOpponentScreen?.EmitWithParam(true);
			NetworkClient.SendAction(
				new StateInfo { PrivateProperties = new Dictionary<string, string> { { "Handshaking", "1" } } },
				(success) => { currentState = success; },
				(failure) => {});
			while (currentState == null)
				yield return null;
			Logger.Log(" ========= Wait for opponent =========");
			countdownCoroutine = StartCoroutine(CountdownCoroutine(30));
			float handshakingWaitStart = Time.time;
			bool hasHandshakingFromBothPlayers = false;
			while (!hasHandshakingFromBothPlayers)
			{
				NetworkClient.GetMatchState(
					(success) => 
					{ 
						hasHandshakingFromBothPlayers = true; 
						currentState = success;
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
					yield return new WaitForSeconds(Random.Range(1f, 2f));
			}
			waitingOpponentScreen?.EmitWithParam(false);
			gameplayScreen?.EmitWithParam(true);
			Logger.Log(" ========= Turn Logic =========");
			while (phase != "3")
			{
				yield return WaitMatchState();
				phase = currentState.PublicProperties["Phase"];
				DateTime utcNow = DateTime.UtcNow;
				switch (phase)
				{
					case "0":
						hasSentMove?.EmitWithParam(false);
						Logger.Log($"Phase: 0 | UTC: {utcNow.ToString("u")} | State: \n{JsonConvert.SerializeObject(currentState, Formatting.Indented)}");
						DateTime startPlayPhaseTime = DateTime.Parse(currentState.PublicProperties["PlayPhaseStartTime"]).ToUniversalTime();
						turnTimerControl.EmitWithParam(false);
						if (utcNow < startPlayPhaseTime)
							yield return new WaitForSeconds((float)(startPlayPhaseTime - utcNow).TotalSeconds + Random.Range(0, 0.3f));
						else
							Logger.LogError("Error in phase 0, it's pass the start play phase time");
						break;
					case "1":
						if (hasSentMove.Value)
							break;
						Logger.Log($"Phase: 1 | UTC: {utcNow.ToString("u")} | State: \n{JsonConvert.SerializeObject(currentState, Formatting.Indented)}");
						DateTime endPlayPhaseTime = DateTime.Parse(currentState.PublicProperties["PlayPhaseEndTime"]).ToUniversalTime();
						turnTimerControl.EmitWithParam(true);
						if (utcNow < endPlayPhaseTime)
						{
							float timeRemaining = (float)(endPlayPhaseTime - utcNow).TotalSeconds;
							StartCoroutine(TurnTimerCoroutine(timeRemaining));
						}

						// DEBUG
						if (autoPlay)
							StartCoroutine(RandomMove());

						while (!hasSentMove.Value && DateTime.UtcNow < endPlayPhaseTime)
							yield return null;
						break;
					case "2":
						hasSentMove?.EmitWithParam(false);
						Logger.Log($"Phase: 2 | UTC: {utcNow.ToString("u")} | State: \n{JsonConvert.SerializeObject(currentState, Formatting.Indented)}");
						DateTime endTurnTime = DateTime.Parse(currentState.PublicProperties["TurnEndTime"]).ToUniversalTime();
						turnTimerControl.EmitWithParam(false);
						if (utcNow < endTurnTime)
						{
							yield return new WaitForSeconds((float)(endTurnTime - utcNow).TotalSeconds + Random.Range(0, 0.3f)); 
						}
						else
							Logger.LogError("Error in phase 2, it's pass the end turn time");
						break;
					default:
						break;
				}
			}
			// Match is over
			matchEndedScreen?.EmitWithParam(true);
			yield return new WaitForSeconds(5);
			menuScreen.EmitWithParam(true);
			Logger.Log("Execution Ended");
		}

		private IEnumerator WaitMatchState ()
		{
			while (true)
			{
				bool hasResponse = false;
				int lastStateHash = currentState.Hash;
				NetworkClient.GetMatchState(success => { hasResponse = true; currentState = success; }, failure => { hasResponse = true; });
				while (!hasResponse)
					yield return null;
				if (currentState.Hash != lastStateHash)
					break;
				else
					yield return new WaitForSeconds(Random.Range(1f, 2f));
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

		// DEBUG
		private IEnumerator RandomMove ()
		{
			yield return new WaitForSeconds(Random.Range(1.5f, 3.5f));
			int move = Random.Range(0, 3);
			switch (move)
			{
				case 0:
					myMoveSignal.EmitWithParam("ROCK");
					break;
				case 1:
					myMoveSignal.EmitWithParam("PAPER");
					break;
				case 2:
					myMoveSignal.EmitWithParam("SCISSORS");
					break;
			}
			yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
			onSendButtonClicked.Emit();
		}
	}
}