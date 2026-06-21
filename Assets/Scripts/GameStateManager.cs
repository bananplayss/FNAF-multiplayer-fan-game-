using QFSW.QC;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : NetworkBehaviour
{
    public event Action OnElectricityOut;
	public event Action OnGameOver;
	public event Action OnWin;

	public event Action OnHideUI;
    public static GameStateManager Instance { get; private set; }

	private int gameOverCount = 0;

	public enum GameState {
        Normal,
        ElectricityOutage,
        GameOver,
		WinGame
    }

	public static GameState CurrentState { get; private set; } = GameState.Normal;

	private void Awake() {
		Instance = this;
	}

	private void Start() {
		NetworkManager.OnPreShutdown += NetworkManager_OnPreShutdown;
	}

	private void NetworkManager_OnPreShutdown() {
		SceneManager.LoadScene(0);
	}

	public void ElectricityOut() {
		if(CurrentState == GameState.ElectricityOutage) {
			return;
		}
		AudioManager.Instance.PlaySound(Sound.PowerOut, .6f);
		AudioManager.Instance.PlaySound(Sound.FreddyTune, .6f);
		CurrentState = GameState.ElectricityOutage;
		OnElectricityOut?.Invoke();
		OnHideUI?.Invoke();
	}

	public void GameOver() {
		if (CurrentState == GameState.GameOver) {
			return;
		}
		CurrentState = GameState.GameOver;
		OnHideUI?.Invoke();
		OnGameOver?.Invoke();
		AudioManager.Instance.MuteMixer();
	}

	[ServerRpc(RequireOwnership = false)]
	public void PlayerDieServerRpc() {
		gameOverCount++;
		if (gameOverCount >= NetworkManager.ConnectedClients.Count) {
			Invoke(nameof(GameOverClientRpc),1f);
			Invoke(nameof(ShutdownServer), 5f);
		}
	}


	[ClientRpc]
	private void GameOverClientRpc() {
		GameOver();
	}

	private void ShutdownServer() {
		NetworkManager.Shutdown();
	}

	private void WinGame() {
		CurrentState = GameState.WinGame;
		OnWin?.Invoke();
		OnHideUI?.Invoke();
		Invoke(nameof(ShutdownServer), 5f);
	}

	[ClientRpc]
	public void SendWinGameClientRpc() {
		WinGame();
	}
}
