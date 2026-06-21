using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraViewUI : MonoBehaviour, IPointerClickHandler
{
	public event Action<CameraViewUI> OnCameraViewClicked;

	[SerializeField] private Camera attachedCam;
	private CameraViewRotationHandler rotationHandler;
	
	private Image selectedImage;

	private void Start() {
		GetRotationHandler();
		selectedImage = GetComponent<Image>();
	}

	public void OnPointerClick(PointerEventData eventData) {
		OnCameraViewClicked.Invoke(this);
	}

	public void SelectView() {
		if(selectedImage == null) {
			selectedImage = GetComponent<Image>();
		}
		Color selectedColor = new Color(255, 255, 0, 0.47f);
		selectedImage.color = selectedColor;
		attachedCam.enabled = true;
		GetRotationHandler();
	}

	public void DeselectView() {
		selectedImage.color = new Color(255, 255, 0, 0);
		attachedCam.enabled = false;
		GetRotationHandler();
		rotationHandler.SetSFXVolume(0);
	}

	public CameraViewRotationHandler GetRotationHandler() {
		if (rotationHandler == null) rotationHandler = attachedCam.GetComponent<CameraViewRotationHandler>();
		return rotationHandler;
	}
}
