using System;
using Unity.Netcode;

public class PlayerWarn : NetworkBehaviour
{

	public event Action OnWarnPlayer;
	public event Action OnStopWarnPlayer;

	private bool dead = false;

	private void Start() {
		WarnPlayerUI.Instance.Initialize(this);
		TabletOverlayUI.Instance.OnOpenCamsAfterDeath += Instance_OnOpenCamsAfterDeath;
		GameStateManager.Instance.OnGameOver += Instance_OnGameOver;
	}

	private void Instance_OnGameOver() {
		dead = true;
		OnStopWarnPlayer?.Invoke();
	}

	private void Instance_OnOpenCamsAfterDeath() {
		dead = true;
		OnStopWarnPlayer?.Invoke();
	}

	public void WarnPlayer(float distance) {
		if (!IsOwner || dead) return;
		if(distance > 4.5f) {
			OnStopWarnPlayer?.Invoke();
		} else {
			OnWarnPlayer?.Invoke();
		}
	}
}
