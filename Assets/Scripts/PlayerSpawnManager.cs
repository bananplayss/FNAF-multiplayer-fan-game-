using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
	[SerializeField] private GameObject[] playerPrefabs;
	[SerializeField] private Transform[] playerSpawns;

	private void Start() {
		DontDestroyOnLoad(gameObject);
	}

	public override void OnNetworkSpawn() {
		NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
	}

	private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut) {
		if(IsHost &&sceneName == "SampleScene") {
			for (int i = 0; i < clientsCompleted.Count; i++) {
				GameObject player = Instantiate(playerPrefabs[i], playerSpawns[i].position,Quaternion.identity);
				player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientsCompleted[i],true);
				player.transform.forward = playerSpawns[i].forward;
			}
		}
	}
}
