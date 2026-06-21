using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GeneratorManager : NetworkBehaviour
{
	[SerializeField] private Generator[] generatorArray;
    private float generatorLeakageInterval = 45f;

	private void Start() {
		if (!IsHost) return;
		StartCoroutine(LeakRandomGenerator());
	}

	private IEnumerator LeakRandomGenerator() {
		yield return new WaitForSeconds(generatorLeakageInterval);
		int random = Random.Range(0, generatorArray.Length);
		LeakGeneratorClientRpc(random);
		StartCoroutine(LeakRandomGenerator());
	}

	[ClientRpc]
	private void LeakGeneratorClientRpc(int index) {
		generatorArray[index].StartLeaking();
		PowerUI.Instance.ShowLeakingGeneratorIndicator();
	}
}
