using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class OilSplashManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] oilSplashArray;

	private void Start() {
		StartCoroutine(ShowOilSplash());
	}

	public override void OnNetworkSpawn() {
		SpawnSplashesServerRpc();
	}

	private IEnumerator ShowOilSplash() {
        float oilSplashShowInterval = 30f;
        yield return new WaitForSeconds(oilSplashShowInterval);
		int random = Random.Range(0, oilSplashArray.Length);
		ShowOilSplashClientRpc(random);
		StartCoroutine(ShowOilSplash());
    }

	[ServerRpc(RequireOwnership = false)]
	private void SpawnSplashesServerRpc() {
		//foreach (GameObject oilSplash in oilSplashArray) {
		//	oilSplash.GetComponent<NetworkObject>().Spawn(true);
		//}
		SpawnSplashesClientRpc();
	}

	[ClientRpc]
	private void SpawnSplashesClientRpc() {
		foreach (GameObject oilSplash in oilSplashArray) {
			oilSplash.SetActive(false);
		}
	}

	[ClientRpc]
	private void ShowOilSplashClientRpc(int index) {
		oilSplashArray[index].SetActive(true);
	}
}
