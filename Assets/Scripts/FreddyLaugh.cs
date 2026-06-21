
using Unity.Netcode;
using UnityEngine;

public class FreddyLaugh : NetworkBehaviour
{
	[SerializeField] private AudioClip[] laughClipArray;

	private AudioSource audioSource;
	private AnimatronicBehaviour animatronic;

	private void Start() {
		audioSource = GetComponent<AudioSource>();
		animatronic = GetComponent<AnimatronicBehaviour>();
		animatronic.OnFreddyLaugh += Animatronic_OnFreddyLaugh;
	}

	private void Animatronic_OnFreddyLaugh() {
		if (!IsOwner) return;
		int random = Random.Range(0, laughClipArray.Length);
		PlayFreddyLaughSFXClientRpc(random);
	}

	[ClientRpc]
	private void PlayFreddyLaughSFXClientRpc(int laughClipIndex) {
		audioSource.clip = laughClipArray[laughClipIndex];
		audioSource.Play();
	}
}
