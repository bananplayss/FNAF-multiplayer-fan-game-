
using bananplayss;
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteract : NetworkBehaviour {

	public event Action OnInitialize;

	public event Action ShowInteractUI;
	public event Action HideInteractUI;

	[SerializeField] private Transform playerCam;
	[SerializeField] private LayerMask raycastMask;
 
	private float maxDistance = 3f;
	private bool canInteract = true;
	private float interactCooldown = .4f;
	private float interactTimer = 0f;


	private void FixedUpdate() {
		if (!IsOwner) return;
		if (GameManager.Instance.inCams) {
			HideInteractUI?.Invoke();
			return;
		}
		HandleRaycast();
		HandleInteractCooldown();
	}

	private void HandleRaycast() {
		Debug.DrawRay(playerCam.position, playerCam.forward * maxDistance, Color.red);
		
		if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, maxDistance, raycastMask) && canInteract) {
			if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable)) {
				ShowInteractUI?.Invoke();
				if (Input.GetMouseButton(0)) {
					interactable.Interact();
					
					canInteract = false;
				}
			} else {
				HideInteractUI?.Invoke();
			}
		} else {
			HideInteractUI?.Invoke();
		}
	}

	private void HandleInteractCooldown() {
		if (!canInteract) {
			interactTimer += Time.fixedDeltaTime;
			if (interactTimer >= interactCooldown) {
				canInteract = true;
				interactTimer = 0f;
			}
		}
	}

	private void Start() {
		PlayerInteractUI.Instance.Initialize(this);
	}
}
