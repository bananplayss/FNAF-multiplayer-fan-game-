using QFSW.QC;
using Unity.Netcode;
using UnityEngine;

public class AdminConsoleManager : MonoBehaviour
{
	[Command]
    public void AdminStartHost() {
        NetworkManager.Singleton.StartHost();
	}

	[Command]
	public void AdminStartClient() {
		NetworkManager.Singleton.StartClient();
	}

	[Command]
	public void CloseApplication() {
		Application.Quit();
	}
}
