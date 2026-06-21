
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class AILevelManager : NetworkBehaviour
{
	[SerializeField] private ClockUI clockUi;
    [SerializeField] private AnimatronicBehaviour bonnie;
	[SerializeField] private AnimatronicBehaviour chica;
	[SerializeField] private AnimatronicBehaviour freddy;
	[SerializeField] private FoxyBehaviour foxy;

	private float mascotTuneInterval;

	private void Start() {
		if (!IsOwner) return;
		clockUi.OnHourChanged += ClockUi_OnHourChanged;
		mascotTuneInterval = Random.Range(220, 405);
		StartCoroutine(MascotTuneUpCoroutine());
	}

	private IEnumerator MascotTuneUpCoroutine() {
		yield return new WaitForSeconds(mascotTuneInterval);
		//AudioManager.Instance.PlaySound(Sound.MascotTune, .6f);
		AudioManager.Instance.PlaySoundServerRpc((int)Sound.MascotTune, 0.6f);
	}

	private void ClockUi_OnHourChanged(int hour) {
		Debug.Log("Increased ai levels just so you know mate");
		if(hour == 1) {
			bonnie.IncreaseAILevel(1);
			chica.IncreaseAILevel(1);
		} else if(hour == 2) {
			bonnie.IncreaseAILevel(2);
			chica.IncreaseAILevel(1);
			foxy.IncreaseAILevel(1);
		}
		else if (hour == 3) {
			bonnie.IncreaseAILevel(2);
			chica.IncreaseAILevel(2);
			freddy.IncreaseAILevel(3);
		} else if (hour == 4) {
			bonnie.IncreaseAILevel(2);
			chica.IncreaseAILevel(1);
			foxy.IncreaseAILevel(1);
			freddy.IncreaseAILevel(2);
		}
		else if (hour == 5) {
			foxy.IncreaseAILevel(1);
			freddy.IncreaseAILevel(1);
		}
	}
}
