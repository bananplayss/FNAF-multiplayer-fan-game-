
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : NetworkBehaviour {
	private List<Transform> playerTransforms = new List<Transform>();

	public event Action OnStunAnimatronics;
	public event Action OnOfficeLockdown;
	public event Action OnFreddy;
	public event Action OnLureBonnie;
	public event Action OnLureChica;
	public event Action OnFlickerDone;
	public static GameManager Instance { get; private set; }
	public bool inCams = false;

	[SerializeField] private Volume volume;

	public OfficeDoor leftDoor;
	public OfficeDoor rightDoor;

	private bool lightFlickering = false;
	private float timeBetweenFlickers = .03f;
	private float lightFlickerPEValue = -10f;
	private float normalPEValue = 0f;
	private float timer = 0f;
	private int flickerTimes = 20;
	private List<AnimatronicBehaviour> animatronicList = new List<AnimatronicBehaviour>();

	private NetworkVariable<int> playerCount = new NetworkVariable<int>(0);

	[field: SerializeField] public float cameraRotationSfxVolume { get; private set; }

	private void Awake() {
		Instance = this;
	}

	public void PlayCameraGlitchSFX() {
		AudioManager.Instance.PlaySound(Sound.CameraGlitch, .6f);
	} 

	private IEnumerator DelayPhoneCall() {
		yield return new WaitForSeconds(5);
		AudioManager.Instance.PlaySound(Sound.PhoneCall, .9f);
	}

	private void Start() {
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments);
		normalPEValue = colorAdjustments.postExposure.value;
		GameStateManager.Instance.OnElectricityOut += GameStateManager_OnElectricityOut;
		CameraViewManager.Instance.OnEnterCamera += Instance_OnEnterCamera;
		StartCoroutine(DelayPhoneCall());
	}

	private void Instance_OnEnterCamera() {
		inCams = true;
		Cursor.visible = true;
		Cursor.lockState= CursorLockMode.None;
	}

	public void AddPlayer(Transform playerTransform) {
		if (!playerTransforms.Contains(playerTransform)) {
			playerTransforms.Add(playerTransform);
		}
	
	}

	[ServerRpc(RequireOwnership = false)]
	public void RepairGeneratorServerRpc(float generatorValue) {
		PowerManager.Instance.AddEnergy(generatorValue);
	}

	public Transform GetRandomPlayerFromList() {
		if (!IsHost) return null;
		if (playerTransforms.Count == 0) {
			return null;
		}
		int randomIndex = UnityEngine.Random.Range(0, playerTransforms.Count);
		return playerTransforms[randomIndex];
	}

	public void RemovePlayerFromList(Transform playerToRemove) {
		if (playerTransforms.Contains(playerToRemove)) {
			playerTransforms.Remove(playerToRemove);
		}
		if (!IsHost) return;
		if(playerTransforms.Count == 0) {
			Debug.Log("Game is officially over hey");
			//GameOverClientRpc();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void RemovePlayerFromListServerRpc(NetworkObjectReference no) {
		no.TryGet(out NetworkObject player);
		RemovePlayerFromList(player.transform);
	}

	private void GameStateManager_OnElectricityOut() {
		volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments);
		normalPEValue = -4.5f;
		colorAdjustments.postExposure.value = normalPEValue;
	}

	public void StartLightFlicker(AnimatronicBehaviour animatronic, int flickerTimes = 16) {
		lightFlickering = true;
		this.flickerTimes = flickerTimes;
		if(animatronic != null) {
			this.animatronicList.Add(animatronic);
		}
	}

	private void Update() {
		if (lightFlickering && animatronicList != null) {
			HandleLightFlicker(animatronicList);
		}

		if (Input.GetKey(KeyCode.Q)) {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		} else {
			if (!inCams) {
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}

	private void HandleLightFlicker(List<AnimatronicBehaviour> animatronic) {
		timer += Time.deltaTime;
		if (timer > timeBetweenFlickers) {
			volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments);
			colorAdjustments.postExposure.value = colorAdjustments.postExposure.value == normalPEValue ? lightFlickerPEValue : normalPEValue;
			flickerTimes--;
			timer = 0f;
			if(flickerTimes == 0) {
				colorAdjustments.postExposure.value = lightFlickerPEValue;
				StartCoroutine(QueueNormalLights(colorAdjustments));
				lightFlickering = false;
			}
		}
	}

	public bool HandleAnimatronicMoveOpp(AnimatronicBehaviour animatronic) {
		if (animatronicList.Contains(animatronic)){
			animatronicList.Remove(animatronic);
			return true;
		}
		return false;
	}

	private IEnumerator QueueNormalLights(ColorAdjustments ca) {
		OnFlickerDone?.Invoke();
		float timeToWait = 1.25f;
		yield return new WaitForSeconds(timeToWait);
		ca.postExposure.value = normalPEValue; 
	}

	[ServerRpc(RequireOwnership = false)]
	public void OffliceLockDownServerRpc() {
		OnOfficeLockdown?.Invoke();
	}

	[ServerRpc(RequireOwnership = false)]
	public void SoundTestServerRpc() {
		AudioManager.Instance.PlaySoundServerRpc((int)Sound.MascotTune,.5f);
	}

	[ServerRpc(RequireOwnership =false)]
	public void CamRecalibrateServerRpc() {
		if (inCams) {
			PlayCameraGlitchSFX();
			CameraViewManagerUI.Instance.ShowNoise(5f);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void StunAnimatronicsServerRpc() {
		OnStunAnimatronics?.Invoke();
	}


	[ServerRpc(RequireOwnership = false)]
	public void LureBonnieServerRpc() {
		OnLureBonnie?.Invoke();
	}

	[ServerRpc(RequireOwnership = false)]
	public void LureChicaServerRpc() {
		OnLureChica?.Invoke();
	}

	[ServerRpc(RequireOwnership = false)]
	public void FreddyAggressionServerRpc() {
		OnFreddy?.Invoke();
	}
}
