using QFSW.QC;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OfficeAreaManager : NetworkBehaviour
{
	public event Action OnStartBerserkMode;
	public event Action OnEndBerserkMode;

	private List<Transform> playersInOffice = new List<Transform>();

	public static OfficeAreaManager Instance { get; private set; }

	private NetworkVariable<bool> jammedLeftSynced = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	private NetworkVariable<bool> jammedRightSynced = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	private NetworkVariable<int> playersInOfficeSynced = new NetworkVariable<int>();

	private int berserkModeThreshold = 3;
	private int currentBerserkValue = 0;
	private bool inBerserkMode = false;

	private void Awake() {
		Instance = this;
	}

	public void PlayerEnteredOffice(Transform player) {
		if (inBerserkMode) {
			EndBerserkModeServerRpc();
		}
		if (!IsHost) return;

		if (!playersInOffice.Contains(player)) {
			playersInOffice.Add(player);
			playersInOfficeSynced.Value = playersInOffice.Count;
			Debug.Log("Player entered office area. Total players in office: " + playersInOffice.Count);
		}
	}

	public void PlayerExitedOffice(Transform player) {
		if (!IsHost) return;
		if (playersInOffice.Contains(player)) {
			playersInOffice.Remove(player);
			playersInOfficeSynced.Value = playersInOffice.Count;
			Debug.Log("Player exited office area. Total players in office: " + playersInOffice.Count);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void EndBerserkModeServerRpc() {
		EndBerserkModeClientRpc();
	}

	[ClientRpc]
	private void EndBerserkModeClientRpc() {
		EndBerserkMode();
	}

	private void EndBerserkMode() {
		inBerserkMode = false;
		OnEndBerserkMode?.Invoke();
		GameManager.Instance.StartLightFlicker(null, 8);
		Debug.Log("Exiting berserk mode due to player entry.");
	}

	public bool IsOfficeJammed(bool isLeft) {
		if(isLeft) {
			return jammedLeftSynced.Value;
		} else {
			return jammedRightSynced.Value;
		}
	}

	public void JamOfficeEquipment(bool jamLeft, bool jamRight) {
		JamOfficeEquipmentServerRpc(jamLeft, jamRight);
	}

	[ServerRpc(RequireOwnership = false)]
	private void JamOfficeEquipmentServerRpc(bool jamLeft, bool jamRight) {
		jammedLeftSynced.Value = jamLeft;
		jammedRightSynced.Value = jamRight;

		jammedLeftSynced.Value = jamLeft;
		jammedRightSynced.Value = jamRight;
	}

	public Transform GetRandomPlayerInOffice() {
		if (playersInOffice.Count == 0) {
			Debug.LogWarning("No players in office to select from!"); 
			return null;
		}
		return playersInOffice[UnityEngine.Random.Range(0,playersInOffice.Count)];
	}

	public void TryBerserkMode() {
		if (!IsOwner) return;
		if (playersInOffice.Count > 0) {
			currentBerserkValue = 0;
		} else if(playersInOffice.Count == 0){
			currentBerserkValue++;
			Debug.Log("Increased berserk value to " + currentBerserkValue  + " need " + berserkModeThreshold);
			if (currentBerserkValue >= berserkModeThreshold) {
				StartBerserkModeClientRpc();
			}
		}
	}

	private void StartBerserkMode() {
		inBerserkMode = true;
		OnStartBerserkMode?.Invoke();
		currentBerserkValue = 0;
	}

	[ClientRpc]
	public void StartBerserkModeClientRpc() {
		StartBerserkMode();
	}

	[Command]
	private void AdminOfficeJam(bool left, bool right) {
		JamOfficeEquipment(left, right);
	}

	[Command]
	private void AdminBerserk() {
		StartBerserkModeClientRpc();
	}
}
