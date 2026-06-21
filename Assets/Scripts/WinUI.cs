using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUI : MonoBehaviour
{
	[SerializeField] private GameObject container;
	[SerializeField] private AudioSource winSFX;


	private void Start() {
		GameStateManager.Instance.OnWin += Instance_OnWin;	
	}

	private void Instance_OnWin() {
		ShowUI();
		AudioManager.Instance.MuteMixer();
		winSFX.Play();
	}

	private void ShowUI() {
		container.SetActive(true);
		//Invoke(nameof(ShowGameLobbyScene), 11f);
	}

	private void ShowGameLobbyScene() {
		//load game lobby scene
		Debug.Log("Implement show lobby here... For now, reload scene.");
		//refactor
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

	}
}
