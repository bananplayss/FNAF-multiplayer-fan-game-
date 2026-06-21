using System;
using UnityEngine;

public class ToggleInteractable : MonoBehaviour,IInteractable {

	public new string name;
	public event Action<bool> OnToggle;

	private bool toggled = false;

	public void Interact() {
		toggled = !toggled;
		UpdateToggle();
	}

	private void UpdateToggle() {
		OnToggle.Invoke(toggled);
	}
}
