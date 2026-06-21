
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerSecurityCamManager : NetworkBehaviour
{
	public event Action<bool> OnSecurityRoomStatusChanged;
	public static PlayerSecurityCamManager Instance { get; private set; }
	private bool inSecurityRoom = false;

	private void Start() {
		PlayerSecurityCamUI.Instance.Initialize(this);
	}

	public void SetInSecurityRoom(bool inRoom) {
		if (!IsOwner) return;
		inSecurityRoom = inRoom;
		OnSecurityRoomStatusChanged?.Invoke(inSecurityRoom);
	}

	private void Awake() {
		Instance = this;
	}
}
