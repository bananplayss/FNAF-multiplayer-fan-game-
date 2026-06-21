using UnityEngine;
using UnityEngine.EventSystems;

public class ArcadeConsole : MonoBehaviour, IInteractable
{
	public void Interact() {
		if(PlayerRole.Instance.GetRole() != PlayerRoles.Technician) {
			InteractUI.Instance.ShowMessage("Only technicians can interact with this!");
			return;
		}
		if (ArcadeConsoleUI.Instance.IsActive()) return;
		ArcadeConsoleUI.Instance.Show();
	}
}
