
using TMPro;
using UnityEngine;

public class PowerUI : MonoBehaviour
{
	public static PowerUI Instance { get; private set; }

	[SerializeField] private TextMeshProUGUI powerAvailableText;
	[SerializeField] private GameObject[] drainLevelObjects;
	[SerializeField] private GameObject container;
	[SerializeField] private GameObject leakingGeneratorIndicator;

	private int maxDrainLevel = 5;

	private void Awake() {
		Instance = this;
	}

	private void Start() {
		PowerManager.Instance.OnPowerDrain += PowerManager_OnPowerDrain;
		PowerManager.Instance.OnUpdateIndicators += PowerManager_OnUpdateIndicators;
		GameStateManager.Instance.OnHideUI += HideUI;
	}
	private void HideUI() {
		container.SetActive(false);
	}

	private void PowerManager_OnUpdateIndicators(int drainLevel) {
		for (int i = 0; i < maxDrainLevel; i++) {
			if (i < drainLevel) {
				drainLevelObjects[i].SetActive(true);
			} else {
				drainLevelObjects[i].SetActive(false);
			}
		}
		drainLevelObjects[0].SetActive(true); // Always show first indicator
	}

	private void PowerManager_OnPowerDrain(float powerAvailable) {
		powerAvailableText.text = $"{powerAvailable}%";
	}

	public void ShowLeakingGeneratorIndicator() {
		leakingGeneratorIndicator.SetActive(true);
	}

	public void HideLeakingGeneratorIndicator() {
		leakingGeneratorIndicator.SetActive(false);
	}
}
