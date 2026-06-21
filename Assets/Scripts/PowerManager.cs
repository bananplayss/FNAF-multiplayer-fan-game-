
using QFSW.QC;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerManager : NetworkBehaviour {
	public static PowerManager Instance { get; private set; }

	public event Action<float> OnPowerDrain;
	public event Action<int> OnUpdateIndicators;

	[SerializeField] private OfficeLights[] lightsArray;
	[SerializeField] private OfficeDoor[] doorArray;
	private Tablet tablet;

	private List<OfficeLights> enabledLights = new List<OfficeLights>();

	private int drainLevel = 1;
	private float powerAvailable = 100f;
	private float passiveDrainInterval = 5f;
	private float passiveDrainTimer = 0f;
	private float drainInterval = 5f;
	private float drainTimer = 0f;

	private NetworkVariable<float> powerAvailableSynced = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	private NetworkVariable<int> drainLevelSynced = new NetworkVariable<int>(1,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

	public override void OnNetworkSpawn() {
		powerAvailableSynced.OnValueChanged += OnPowerChanged;
		drainLevelSynced.OnValueChanged += OnDrainLevelChanged;
	}

	private void OnPowerChanged(float previous, float current) {
		OnPowerDrain?.Invoke(current);
		if (current <= 0) {
			current = 0;
			GameStateManager.Instance.ElectricityOut();
			Debug.Log("Power Out!");
		}
	}

	private void OnDrainLevelChanged(int previous, int current) {
		OnUpdateIndicators?.Invoke(current);
	}

	public void Initialize(Tablet tablet) {
		this.tablet = tablet;

		tablet.OnEnterCamera += Tablet_OnEnterCamera;
		tablet.OnExitCamera += Tablet_OnExitCamera;
	}

	public void AddEnergy(float energy) {
		powerAvailable += energy;
		if(powerAvailable > 100) {
			powerAvailable = 100;
		}
		powerAvailableSynced.Value = powerAvailable;
	}

	private void Awake() {
		Instance = this;
	}

	[ServerRpc(RequireOwnership = false)]
	public void LeakGeneratorServerRpc() {
		DrainPower(2);
	}

	private void Start() {
		foreach (OfficeLights light in lightsArray) {
			light.OnLightEnable += Light_OnLightEnable;
			light.OnLightDisable += Light_OnLightDisable;
		}
		foreach (OfficeDoor door in doorArray) {
			door.OnDoorOpen += Door_OnDoorOpen;
			door.OnDoorClose += Door_OnDoorClose;
		}

		GameManager.Instance.OnStunAnimatronics += Instance_OnStunAnimatronics;
	}

	private void Instance_OnStunAnimatronics() {
		float powerToDrain = 7f;
		DrainPower(powerToDrain);
	}

	private void Tablet_OnExitCamera() {
		if (!IsHost) return;
		drainLevel--;
		drainLevelSynced.Value = drainLevel;
		OnUpdateIndicators?.Invoke(drainLevelSynced.Value);
	}

	private void Tablet_OnEnterCamera() {
		if (!IsHost) return;
		drainLevel++;
		drainLevelSynced.Value = drainLevel;
		OnUpdateIndicators?.Invoke(drainLevelSynced.Value);
	}

	private void Door_OnDoorClose(OfficeDoor obj) {
		if (!IsHost) return;
		
		drainLevel--;
		drainLevelSynced.Value = drainLevel;
		OnUpdateIndicators?.Invoke(drainLevelSynced.Value);
	}

	private void Door_OnDoorOpen(OfficeDoor obj) {
		if (!IsHost) return;
		if (obj.onLockDown) return;
		drainLevel++;
		drainLevelSynced.Value = drainLevel;
		OnUpdateIndicators?.Invoke(drainLevelSynced.Value);
	}

	private void Light_OnLightEnable(OfficeLights obj) {
		if(!IsHost) return;
		enabledLights.Add(obj);
		if (enabledLights.Count == 1) {
			drainLevel++;
		}
		drainLevelSynced.Value = drainLevel;
		OnUpdateIndicators?.Invoke(drainLevelSynced.Value);
	}

	private void Light_OnLightDisable(OfficeLights obj) {
		if (!IsHost) return;
		enabledLights.Remove(obj);
		if (enabledLights.Count == 0) {
			drainLevel--;
		}
		drainLevelSynced.Value = drainLevel;
		OnUpdateIndicators?.Invoke(drainLevelSynced.Value);
	}

	private void Update() {
		if (!IsHost) return;
		if (drainLevel == 1) {
			passiveDrainTimer += Time.deltaTime;
			if (passiveDrainTimer >= passiveDrainInterval) {
				passiveDrainTimer = 0f;
				DrainPower(1);
			}
		}

		if (drainLevel > 1) {
			drainInterval = GetCorrectDrainInterval();
			drainTimer += Time.deltaTime;
			if (drainTimer >= drainInterval) {
				drainTimer = 0f;
				DrainPower(1);
			}
		}
	}

	private void DrainPower(float powerToDrain) {
		if (!IsHost) return;
		powerAvailable -= powerToDrain;
		powerAvailableSynced.Value = powerAvailable;
		OnPowerDrain?.Invoke(powerAvailableSynced.Value);
		if(powerAvailable <= 0) {
			powerAvailable = 0;
			GameStateManager.Instance.ElectricityOut();
			Debug.Log("Power Out!");
		}
	}

	public void DrainPowerOnBonk(float powerToDrain) {
		if(!IsHost) return;
		DrainPower(powerToDrain);
	}

	private float GetCorrectDrainInterval() {
		if (drainLevel == 1) {
			return 5f;
		} else if (drainLevel == 2) {
			return 3.5f;
		} else if (drainLevel == 3) {
			return 2.3f;
		} else if (drainLevel == 4) {
			return 1.3f;
		} else {
			return .8f;
		}
	}

	[Command]
	public void AdminForcePowerOutage() {
		DrainPower(powerAvailable);
	}
}
