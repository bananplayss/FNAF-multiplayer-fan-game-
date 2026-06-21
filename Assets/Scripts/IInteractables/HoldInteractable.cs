using System;
using Unity.Netcode;
using UnityEngine;

public class HoldInteractable : NetworkBehaviour, IInteractable {
	public event Action<float> OnInteract;

	private bool canInteract = true;
	private bool interacting = false;
	private float interactValue = 0f;
	private float interactTimeNeeded = 3f;

	public void Interact() {
		if (!HasPermission()) return;
		if (!canInteract) {
			InteractUI.Instance.ShowMessage("This generator is already repaired");
			return;
		}
		interacting = !interacting;
		if (!interacting) {
			interactValue = 0f;
			InteractUI.Instance.SetInteractValue(0f);
		}
	}

	public void SetCanInteract(bool canInteract) {
		this.canInteract = canInteract;
	}

	private void Update() {
		if (interacting && canInteract) {
			interactValue += Time.deltaTime;
			float normalizedInteractValue = 1-(interactValue/ interactTimeNeeded);
			InteractUI.Instance.SetInteractValue(normalizedInteractValue);
			if (interactValue >= interactTimeNeeded) {
				interactValue = 0f;
				interacting = false;
				SetCanInteract(false);
				FinishInteract();
			}
		}
	}

	public virtual void FinishInteract() {
		Debug.LogError("This shouldn't happen though.");
	}

	public virtual bool HasPermission() {
		return true;
	}
}
