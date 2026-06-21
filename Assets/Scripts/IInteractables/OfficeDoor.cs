using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class OfficeDoor : NetworkBehaviour
{
	private enum OfficeDoorSFXs {
		JamSFX,
		DoorClose
	}

	public event Action OnFlash;
	public event Action<OfficeDoor> OnDoorOpen;
	public event Action<OfficeDoor> OnDoorClose;
    [SerializeField] private ToggleInteractable doorButton;
	[SerializeField] private AudioSource[] sources;
	[SerializeField] private OfficeLights lights;
	private Animator anim;
	public bool onLockDown = false;
	public bool isLeft = false;
	private bool isClosed = false;
	private bool electricityOut = false;

	private NetworkVariable<bool> isClosedSynced = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

	private void Start() {
		doorButton.OnToggle += HandleDoorToggle;
		anim = GetComponent<Animator>();
		lights.OnLightEnable += Lights_OnLightEnable;
		GameStateManager.Instance.OnElectricityOut += GameStateManager_OnElectricityOut;
		GameManager.Instance.OnOfficeLockdown += Instance_OnOfficeLockdown;
	}

	private void Instance_OnOfficeLockdown() {
		LockDownServerRpc(); 
	}

	[ServerRpc(RequireOwnership = false)]
	private void LockDownServerRpc() {
		if (!IsDoorClosed()) {
			onLockDown = true;
			HandleDoorToggle(true);
			StartCoroutine(UnlockAll());
		}
	} 

	private IEnumerator UnlockAll() {
		float lockInterval = 10f;
		yield return new WaitForSeconds(lockInterval);
		onLockDown = false;
	}

	public override void OnNetworkSpawn() {
		isClosedSynced.OnValueChanged += OnIsClosedChanged;
	}

	private void OnIsClosedChanged(bool previous, bool current) {
		isClosed = current;
	}

	private void GameStateManager_OnElectricityOut() {
		electricityOut = true;
		OpenDoor();
	}

	private void Lights_OnLightEnable(OfficeLights light) {
		OnFlash?.Invoke();
	}

	private void HandleDoorToggle(bool toggled) {
		if(electricityOut) return;
		if (OfficeAreaManager.Instance.IsOfficeJammed(isLeft) || onLockDown) {
			PlaySFXServerRpc((int)OfficeDoorSFXs.JamSFX);
			return;
		} else {
			if (!toggled) {
				OpenDoor();
			} else {
				CloseDoor();
			}
			PlaySFXServerRpc((int)OfficeDoorSFXs.DoorClose);
		}
		
	}

	[ServerRpc(RequireOwnership = false)]
	private void PlaySFXServerRpc(int index) {
		PlaySFXClientRpc(index);
	}
	[ClientRpc]
	private void PlaySFXClientRpc(int index) {
		sources[index].Play();
	}

	private void CloseDoor() {
		OnDoorOpen?.Invoke(this);
		anim.Play("Close");
		isClosed = true;
		SyncIsClosedServerRpc(isClosed);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SyncIsClosedServerRpc(bool isClosed) {
		isClosedSynced.Value = isClosed;
	}

	private void OpenDoor() {
		OnDoorClose?.Invoke(this);
		anim.Play("Open");
		isClosed = false;
		SyncIsClosedServerRpc(isClosed);
	}

	public bool IsDoorClosed() {
		return isClosed;
	}
}
