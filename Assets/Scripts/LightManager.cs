using QFSW.QC;
using System.Collections;
using UnityEngine;

public class LightManager : MonoBehaviour {
	[SerializeField] private Light[] sceneLightArray;
	[SerializeField] private AudioSource alarmSFX;
	private Color originalColor = Color.white;
	private float originalIntensity = 1f;
	private float alarmLightIntensity = 11f;

	private bool alarmLights = false;

	private void Start() {
		originalColor = sceneLightArray[0].color;
		originalIntensity = sceneLightArray[0].intensity;
		NormalLights();
		OfficeAreaManager.Instance.OnStartBerserkMode += Instance_OnStartBerserkMode;
		OfficeAreaManager.Instance.OnEndBerserkMode += Instance_OnEndBerserkMode;
	}

	private void Instance_OnStartBerserkMode() {
		AlarmLights(9999f);
	}

	private void Instance_OnEndBerserkMode() {
		NormalLights();
		alarmLights = false;
	}

	public void AlarmLights(float duration) {
		alarmLights = true;
		foreach (Light sceneLight in sceneLightArray) {
			sceneLight.color = Color.red;
			sceneLight.intensity = alarmLightIntensity;
		}
		alarmSFX.enabled = true;
		StartCoroutine(DelayDisableAlarmLights(duration));
	}

	private void NormalLights() {
		alarmSFX.enabled = false;
		alarmLights = false;
		foreach (Light sceneLight in sceneLightArray) {
			sceneLight.color = originalColor;
			sceneLight.intensity = originalIntensity;
		}
	}

	private void Update() {
		if (alarmLights) {
			foreach(Light sceneLight in sceneLightArray) {
				sceneLight.intensity = Mathf.PingPong(Time.time * 5f, 5f);
			}
		}
	}

	private IEnumerator DelayDisableAlarmLights(float delay) {
		yield return new WaitForSeconds(delay);
		NormalLights();
	}

	//[Command]
	public void AdminAlarmLights(float duration) {
		AlarmLights(duration);
	}
}
