using UnityEngine;

public class OfficeAreaCollider : MonoBehaviour
{
	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			PlayerSecurityCamManager.Instance.SetInSecurityRoom(true);
			OfficeAreaManager.Instance.PlayerEnteredOffice(other.transform);
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.CompareTag("Player")) {
			PlayerSecurityCamManager.Instance.SetInSecurityRoom(false);
			OfficeAreaManager.Instance.PlayerExitedOffice(other.transform);
		}
	}
}
