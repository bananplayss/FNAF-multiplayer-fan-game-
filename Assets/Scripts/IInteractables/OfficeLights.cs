
using System;
using Unity.Netcode;
using UnityEngine;

public class OfficeLights : NetworkBehaviour
{
	public event Action<OfficeLights> OnLightEnable;
	public event Action<OfficeLights> OnLightDisable;
	[SerializeField] private ToggleInteractable doorFlash;
	[SerializeField] private AudioSource lightSfx;
	[SerializeField] private AudioSource jamSfx;
	[SerializeField] private GameObject curtain;
	public bool isLeft = false;
	private new Light light;
	private bool electricityOut = false;

	private void Start() {
		GameStateManager.Instance.OnElectricityOut += GameStateManager_OnElectricityOut;
		doorFlash.OnToggle += HandleFlashToggle;
		light = GetComponent<Light>();
	}

	private void GameStateManager_OnElectricityOut() {
		electricityOut = true;
		DisableFlash();
	}

	private void HandleFlashToggle(bool toggled) {
		if (electricityOut) return;
		if (OfficeAreaManager.Instance.IsOfficeJammed(isLeft)){
			PlayJamSFXServerRpc();
			return;
		} else {
			if (toggled) {
				EnableFlashServerRpc();
			} else {
				DisableFlashServerRpc();
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void PlayJamSFXServerRpc() {
		PlayJamSFXClientRpc();
	}
	[ClientRpc]
	private void PlayJamSFXClientRpc() {
		jamSfx.Play();
	}

	[ServerRpc(RequireOwnership = false)]
	private void EnableFlashServerRpc() {
		EnableFlashClientRpc();
	}

	[ClientRpc]
	private void EnableFlashClientRpc() {
		EnableFlash();
	}

	[ServerRpc(RequireOwnership = false)]
	private void DisableFlashServerRpc() {
		DisableFlashClientRpc();
	}

	[ClientRpc]
	private void DisableFlashClientRpc() {
		DisableFlash();
	}

	private void EnableFlash() {
		light.enabled = true;
		lightSfx.enabled = true;
		curtain.SetActive(false);
		OnLightEnable?.Invoke(this);
	}

	private void DisableFlash() {
		light.enabled = false;
		lightSfx.enabled = false;
		curtain.SetActive(true);
		OnLightDisable?.Invoke(this);
	}
}
