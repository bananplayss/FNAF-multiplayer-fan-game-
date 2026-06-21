using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Generator : HoldInteractable
{
	[SerializeField] private float generatorElectricityValue = 5;
	private AudioSource leakSFX;
	private NetworkVariable<bool> isLeaking = new NetworkVariable<bool>(false);

	private void Start() {
		leakSFX = GetComponent<AudioSource>();
	}

	public override void FinishInteract() {
		GameManager.Instance.RepairGeneratorServerRpc(generatorElectricityValue);
		InteractUI.Instance.ShowMessage($"Generator repaired and restored {generatorElectricityValue}% energy");
		if (isLeaking.Value) {
			StopGeneratorLeakServerRpc();
		}
	}

	public override bool HasPermission() {
		if (PlayerRole.Instance.GetRole() != PlayerRoles.Electrician) {
			InteractUI.Instance.ShowMessage("Only an electrician can repair generators.");
			return false;
		}
		return true;
	}

	public void StartLeaking() {
		if (isLeaking.Value) return;
		StartGeneratorLeakServerRpc();
		SetCanInteract(true);
		EnableAudioSourceServerRpc();
		if (IsHost || isLeaking.Value) {
			StartCoroutine(Leak());
		}
	}
	[ServerRpc(RequireOwnership = false)]
	private void StartGeneratorLeakServerRpc() {
		isLeaking.Value = true;
	}

	[ServerRpc(RequireOwnership = false)]
	private void StopGeneratorLeakServerRpc() {
		isLeaking.Value = false;
		StopGeneratorLeakClientRpc();
		StopCoroutine(Leak());
	}

	[ClientRpc]
	private void StopGeneratorLeakClientRpc() {
		DisableAudioSourceServerRpc();
		PowerUI.Instance.HideLeakingGeneratorIndicator();
		StopCoroutine(Leak());
	}

	[ServerRpc(RequireOwnership = false)]
	private void EnableAudioSourceServerRpc() {
		EnableAudioSourceClientRpc();
	}


	[ClientRpc]
	private void EnableAudioSourceClientRpc() {
		leakSFX.enabled = true;
		leakSFX.Play();
	}

	[ServerRpc(RequireOwnership = false)]
	private void DisableAudioSourceServerRpc() {
		DisableAudioSourceClientRpc();
	}


	[ClientRpc]
	private void DisableAudioSourceClientRpc() {
		leakSFX.Stop();
		leakSFX.enabled = false;
	}

	[ClientRpc]
	private void ShowIndicatorClientRpc() {
		PowerUI.Instance.ShowLeakingGeneratorIndicator();
	}

	[ClientRpc]
	private void HideIndicatorClientRpc() {
		PowerUI.Instance.HideLeakingGeneratorIndicator();
	}


	private IEnumerator Leak() {
		if (!IsHost) yield break;
		if (!isLeaking.Value) { 
			StopCoroutine(Leak());
			HideIndicatorClientRpc();
			yield break;
		}
		yield return new WaitForSeconds(3);
		ShowIndicatorClientRpc();
		PowerManager.Instance.LeakGeneratorServerRpc();
		StartCoroutine(Leak());
	}

}
