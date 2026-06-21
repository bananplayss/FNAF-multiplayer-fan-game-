using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject container;


	private void Start() {
		GameStateManager.Instance.OnGameOver += GameStateManager_OnGameOver;
	}

	private void GameStateManager_OnGameOver() {
		CameraViewManagerUI.Instance.ShowNoise(3);
		AudioManager.Instance.PlaySound(Sound.GameOverStatic, .8f);
		Invoke(nameof(ShowUI),2f);
	}

	private void ShowUI() {
		container.SetActive(true);
		AudioManager.Instance.MuteMixer();
	}
}
