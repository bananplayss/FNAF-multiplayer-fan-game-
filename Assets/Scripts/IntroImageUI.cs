
using UnityEngine;

public class IntroImageUI : MonoBehaviour
{
	[SerializeField] private GameObject container;
	[SerializeField] private CanvasGroup group;
	[SerializeField] private LobbyUI lobbyui;

	private void Start() {
		lobbyui.OnStartLobby += Lobbyui_OnStartLobby;
	}

	private void Lobbyui_OnStartLobby() {
		container.SetActive(true);
		LeanTween.alphaCanvas(group, 1, 3f).setOnComplete(LeanBack);
	}

	private void LeanBack() {
		Invoke(nameof(DelayLeanBack), 3f);
	}

	private void DelayLeanBack() {
		LeanTween.alphaCanvas(group, 0, 4f);
	}
}
