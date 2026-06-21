
using System;
using UnityEngine;

public class CameraViewManager : MonoBehaviour {
	public static CameraViewManager Instance { get; private set; }

	public event Action OnExitCamera;
	public event Action OnEnterCamera;
	public event Action OnCameraViewChanged;

	[SerializeField] private CameraViewUI[] cameraViews;
	[SerializeField] private CameraViewUI lastFreddyCam;
	[SerializeField] private CameraViewUI foxyCam;
	[SerializeField] private CameraViewUI leftHallwayCam;
	private Tablet tablet;
	private AudioSource camSwitchSFX;
	private CameraViewUI selectedCameraView;
	private CameraViewUI lastSelectedCameraView = null;

	public CameraViewUI GetSelectedCameraViewUI() {
		return selectedCameraView;
	}

	public void Initialize(Tablet tablet) {
		this.tablet = tablet;
		tablet.OnExitCamera += Tablet_OnExitCamera;
		tablet.OnEnterCamera += Tablet_OnEnterCamera;
	}

	private void Awake() {
		Instance = this;
	}

	private void Start() {
		foreach (var cameraView in cameraViews) {
			cameraView.OnCameraViewClicked += CameraView_OnCameraViewClicked;
		}

		camSwitchSFX = GetComponent<AudioSource>();
	}

	private void Tablet_OnEnterCamera() {
		if (selectedCameraView == null) {
			if (lastSelectedCameraView == null) lastSelectedCameraView = cameraViews[0];
			selectedCameraView = lastSelectedCameraView;
			selectedCameraView.SelectView();
			selectedCameraView.GetRotationHandler().SetSFXVolume(GameManager.Instance.cameraRotationSfxVolume);
			camSwitchSFX.Play();
		}
		OnEnterCamera?.Invoke();
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

	}

	public bool IsGlitchCamsSelected() {
		bool isGlitchCam = selectedCameraView == cameraViews[0] || selectedCameraView == cameraViews[1];
		return isGlitchCam;
	}

	private void Tablet_OnExitCamera() {
		OnExitCamera?.Invoke();
		if (selectedCameraView != null) {
			selectedCameraView.DeselectView();
			lastSelectedCameraView = selectedCameraView;
			selectedCameraView = null;
		}
	}

	private void CameraView_OnCameraViewClicked(CameraViewUI obj) {
		if (obj != selectedCameraView) {
			if (selectedCameraView != null) {
				selectedCameraView.DeselectView();
			}
			selectedCameraView = obj;
			selectedCameraView.SelectView();
			camSwitchSFX.Play();
			OnCameraViewChanged?.Invoke();
		}

	}

	public void MuteAllCameraRotationSFX() {
		foreach (var cameraView in cameraViews) {
			CameraViewRotationHandler rotationHandler = cameraView.GetRotationHandler();
			if (rotationHandler != null) {
				rotationHandler.SetSFXVolume(0f);
			}
		}
	}

	public bool IsOnLastFreddyCam() {
		return selectedCameraView == lastFreddyCam && GameManager.Instance.inCams;
	}

	public bool IsOnFoxyCam() {
		return selectedCameraView == foxyCam && GameManager.Instance.inCams;
	}
	public bool IsOnLeftHallwayCam() {
		return selectedCameraView == leftHallwayCam && GameManager.Instance.inCams;
	}
	

	public void UnmuteSelectedCameraRotationSFX() {
		if(selectedCameraView != null) {
			selectedCameraView.GetRotationHandler().SetSFXVolume(GameManager.Instance.cameraRotationSfxVolume);
		}
	}
}
