using bananplayss;
using Unity.Netcode;
using UnityEngine;

public class OilSplash : NetworkBehaviour, IInteractable {
	PlayerMovement lastSlowed;

	private void OnTriggerExit(Collider other) {
		if (other.CompareTag("Player")) {
			other.TryGetComponent<PlayerMovement>(out PlayerMovement playerMovement);
			playerMovement.SlowPlayer(2);
		}
	}

	private void OnTriggerStay(Collider other) {
		if (other.CompareTag("Player")) {
			other.TryGetComponent<PlayerMovement>(out PlayerMovement playerMovement);
			lastSlowed = playerMovement;
			playerMovement.SlowPlayer();
		}
	}

	public void ReleaseLastSlow() {
		if(lastSlowed != null) {
			lastSlowed.SlowPlayer(.1f);
		}
	}

	public void Interact() {
		if (PlayerRole.Instance.GetRole() != PlayerRoles.Janitor) {
			InteractUI.Instance.ShowMessage("Only a janitor can clean this up!");
			return;
		}
		QuickTimeEventManager.Instance.StartMinigame(this.gameObject);
	}
}
