using QFSW.QC;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public enum Sound {
	Jumpscare,
	TheyAreHere,
	FreddyTune,
	GameOverStatic,
	MascotTune,
	CameraGlitch,
	PhoneCall,
	PowerOut,
}

public class AudioManager : NetworkBehaviour
{
    private AudioSource source;

	[SerializeField] private AudioMixer mainMixer;
	[SerializeField] private AudioSource mainAmbienceSource;
	[SerializeField] private AudioClip ambience2;

    public static AudioManager Instance { get; private set; }

	[SerializeField] private AudioClip[] audioClips;

	private bool warningPlayer = false;
	private bool mixerMuted = false;

	private void Awake() {
		Instance = this;
	}

	private void Start() {
		source = GetComponent<AudioSource>();
	}

	public void MuteMixer() {
		mainMixer.SetFloat("Volume", -80f);
		mixerMuted = true;
	}
	public void UnmuteMixer() {
		mainMixer.SetFloat("Volume", 0f);
		mixerMuted = false;
	}

	public void WarnPlayerVolume() {
		warningPlayer = true;
	}

	public void ResetWarn() {
		warningPlayer = false;
	}

	private void Update() {
		if(mixerMuted) return;
		if (warningPlayer) {
			mainMixer.GetFloat("Volume", out float currentVol);
			currentVol = Mathf.MoveTowards(currentVol, -20f, Time.deltaTime*5);
			mainMixer.SetFloat("Volume", currentVol);
		} else {
			mainMixer.GetFloat("Volume", out float currentVol);
			while(currentVol < 0f) {
				currentVol = Mathf.MoveTowards(currentVol, 0f, Time.deltaTime*.2f);
				mainMixer.SetFloat("Volume", currentVol);
			}
		}
	}


	[ServerRpc(RequireOwnership = false)]
	public void PlaySoundServerRpc(int clipIndex, float volume = 1f) {
		PlaySoundClientRpc(clipIndex, volume);
	}

	[ClientRpc]
	private void PlaySoundClientRpc(int clipIndex, float volume = 1f) {
		PlaySound((Sound)clipIndex, volume);
	}

	public void PlaySound(Sound clip, float volume = 1f) {
		Debug.Log("PlayedSound: " + clip.ToString());
		source.PlayOneShot(audioClips[(int)clip], volume);
	}

	public void ChangeToAmbience2() {
		if(mainAmbienceSource.clip == ambience2) return;
		mainAmbienceSource.Stop();
		mainAmbienceSource.clip = ambience2;
		mainAmbienceSource.Play();
	}
}
