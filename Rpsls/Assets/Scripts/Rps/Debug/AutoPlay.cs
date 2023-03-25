using Kalkatos.UnityGame.Scriptable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class AutoPlay : MonoBehaviour
{
    [SerializeField] private bool isActive = true;
    [SerializeField] private Signal playButtonClickSignal;
    [SerializeField] private int secondsToWait = 3;

    void Start()
    {
        if (!isActive)
            return;
        DateTime now = DateTime.UtcNow;
        float seconds = secondsToWait + (float)TimeSpan.FromTicks(secondsToWait * TimeSpan.TicksPerSecond - now.Ticks % (secondsToWait * TimeSpan.TicksPerSecond)).TotalSeconds;
		Debug.Log($"Waiting {seconds} seconds.");
        StartCoroutine(ClickPlayButton(seconds));
    }

    private IEnumerator ClickPlayButton (float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        playButtonClickSignal.Emit();
	}
}
