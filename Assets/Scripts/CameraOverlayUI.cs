
using UnityEngine;

public class CameraOverlayUI : MonoBehaviour
{
	public static CameraOverlayUI Instance { get; private set; }

    private Tablet tablet;
	[SerializeField] private GameObject cameraOverlay;


	public void Initialize(Tablet tablet) {
		this.tablet = tablet;
		tablet.OnExitCamera += Tablet_OnExitCamera;
		tablet.OnEnterCamera += Tablet_OnEnterCamera;
	}

	private void Awake() {
		Instance = this;
	}

	private void Tablet_OnEnterCamera() {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = false;
		cameraOverlay.SetActive(true);
	}

	private void Tablet_OnExitCamera() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = true;
		cameraOverlay.SetActive(false);
	}
}
