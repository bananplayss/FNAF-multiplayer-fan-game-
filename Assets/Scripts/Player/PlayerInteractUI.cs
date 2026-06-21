
using UnityEngine;

public class PlayerInteractUI : MonoBehaviour
{
	public static PlayerInteractUI Instance {  get; private set; }

    private PlayerInteract playerInteract;
	[SerializeField] private GameObject container;

	public void Initialize(PlayerInteract pi) {
		playerInteract = pi;
		playerInteract.ShowInteractUI += PlayerInteract_ShowInteractUI;
		playerInteract.HideInteractUI += PlayerInteract_HideInteractUI;
	}

	private void Awake() {
		Instance = this;
	}

	private void PlayerInteract_HideInteractUI() {
		container.SetActive(false);
	}

	private void PlayerInteract_ShowInteractUI() {
		container.SetActive(true);
	}
}
