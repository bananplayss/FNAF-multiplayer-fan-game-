using System.Collections;
using UnityEngine;

public class CameraViewManagerUI : MonoBehaviour {
	public static CameraViewManagerUI Instance { get; private set; }

	[SerializeField] private CameraViewManager cameraViewManager;
	[SerializeField] private GameObject noiseImage;

	private void Awake() {
		Instance = this;
	}
	private void Start() {
		cameraViewManager.OnCameraViewChanged += CameraViewManager_OnCameraViewChanged;
	}

	private void CameraViewManager_OnCameraViewChanged() {
		cameraViewManager.MuteAllCameraRotationSFX();
		noiseImage.SetActive(true);
		StartCoroutine(DisableNoiseAfterDelay());
	}

	public void ShowNoise(float duration) {

		noiseImage.SetActive(true);
		StartCoroutine(DisableNoiseAfterDelay(duration));
	}

	private IEnumerator DisableNoiseAfterDelay(float duration = .3f) {
		yield return new WaitForSeconds(duration);
		noiseImage.SetActive(false);
		cameraViewManager.UnmuteSelectedCameraRotationSFX();
	}
}
