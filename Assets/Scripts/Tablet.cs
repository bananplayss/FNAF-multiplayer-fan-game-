using System;
using UnityEngine;

public class Tablet : MonoBehaviour
{
	public event Action OnExitCamera;
    public event Action OnEnterCamera;
	public event Action OnDisableTablet;
	
    private Animator anim;
	private AudioSource monitorSfx;

	private bool electricityOut = false;

	private void Start() {
		anim = GetComponent<Animator>();
		monitorSfx = GetComponent<AudioSource>();
		GameStateManager.Instance.OnElectricityOut += GameStateManager_OnElectricityOut;
		CameraViewManager.Instance.Initialize(this);
		CameraOverlayUI.Instance.Initialize(this);
		PowerManager.Instance.Initialize(this);
		PlayerSecurityCamUI.Instance.Initialize(this);

		if (PlayerRole.Instance.GetRole() != PlayerRoles.Security) {
			OnDisableTablet?.Invoke();
			enabled = false;
		}
	}

	private void GameStateManager_OnElectricityOut() {
		if (GameManager.Instance.inCams) {
			electricityOut = true;
			PullDownCams();
		}
	}

	public void ToggleCam(bool toggle) {
		if (electricityOut) return;
		string clipName = toggle ? "EnterCams" : "ExitCams";
		anim.Play(clipName);
		monitorSfx.Play();
	}
    public void EnterCamera() {
		OnEnterCamera?.Invoke();
	}

	public void ExitCamera() {
		OnExitCamera?.Invoke();
	}

	public void PullDownCams() {
		ToggleCam(false);
	}
}
