using Kalkatos.UnityGame.Scriptable;
using System;
using System.Collections;
using UnityEngine;

public class AutoPlay : MonoBehaviour
{
    [SerializeField] private bool isActive = true;
    [SerializeField] private Signal playButtonClickSignal;
    [SerializeField] private Signal reconnectButtonClickSignal;
    [SerializeField] private ScreenSignal menuSignal;
    [SerializeField] private int secondsToWait = 3;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject reconnectButton;

    void Start()
    {
        if (!isActive)
            return;
        if (Input.GetKey(KeyCode.Escape))
        {
            isActive = false;
            return;
        }
        DateTime now = DateTime.UtcNow;
        float seconds = secondsToWait + (float)TimeSpan.FromTicks(secondsToWait * TimeSpan.TicksPerSecond - now.Ticks % (secondsToWait * TimeSpan.TicksPerSecond)).TotalSeconds;
		Debug.Log($"Waiting {seconds} seconds.");
        StartCoroutine(ClickPlayButton(seconds));
    }

	private void Update ()
	{
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (reconnectButton.activeSelf)
                reconnectButtonClickSignal.Emit();
            else
                playButtonClickSignal.Emit();
        }
	}

	private IEnumerator ClickPlayButton (float timeToWait)
    {
        yield return new WaitForSeconds(2f);
        if (reconnectButton.activeSelf)
        {
            reconnectButtonClickSignal.Emit();
            yield break;
        }
        yield return new WaitForSeconds(Mathf.Max(timeToWait - 2, 0.1f));
        playButtonClickSignal.Emit();
	}
}
