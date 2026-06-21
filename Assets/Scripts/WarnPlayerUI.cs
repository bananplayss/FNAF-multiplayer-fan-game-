using System;
using UnityEngine;

public class WarnPlayerUI : MonoBehaviour
{
	public static WarnPlayerUI Instance { get; private set; }

	private PlayerWarn playerWarn;
	[SerializeField] private CanvasGroup canvasGroup;

	private bool warningPlayer = false;

	public void Initialize(PlayerWarn warn) {
		playerWarn = warn;
		playerWarn.OnWarnPlayer += PlayerWarn_OnWarnPlayer;
		playerWarn.OnStopWarnPlayer += PlayerWarn_OnStopWarnPlayer;
	}

	private void Awake() {
		Instance = this;
	}

	private void PlayerWarn_OnStopWarnPlayer() {
		warningPlayer = false;
		LeanTween.alphaCanvas(canvasGroup, 0f, 0.6f);
		AudioManager.Instance.ResetWarn();
	}

	private void PlayerWarn_OnWarnPlayer() {
		if(warningPlayer) return;
		AudioManager.Instance.WarnPlayerVolume();
		warningPlayer = true;
		LeanTween.alphaCanvas(canvasGroup, 1f, 0.8f);
	}
}
 