
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabletOverlayUI : MonoBehaviour, IPointerEnterHandler {
	public static TabletOverlayUI Instance {  get; private set; }

	public event Action OnOpenCamsAfterDeath;
	public event Action OnHoverEnter;

	public void OnPointerEnter(PointerEventData eventData) {
		OnHoverEnter?.Invoke();
	}

	private void Awake() {
		Instance = this;
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			OnHoverEnter?.Invoke();
		}
	}

	public void OpenCamsAfterDeath() {
		OnOpenCamsAfterDeath?.Invoke();
	}
}
