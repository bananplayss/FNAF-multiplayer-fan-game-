using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class QuickTimeEventUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] patternTexts;
    [SerializeField] private Image[] patternImages;
	[SerializeField] private GameObject container;
	[SerializeField] private Image background;
	[SerializeField] private TextMeshProUGUI messageText;

	string loseText = "You have failed to clean up...";
	string winText = "Keeping the floors clean...Good.";


	private void Start() {
		background.color = Color.gray;
		QuickTimeEventManager.Instance.OnMinigameStarted += QuickTimeEventManager_OnMinigameStarted;
		QuickTimeEventManager.Instance.OnMinigameEnded += QuickTimeEventManager_OnMinigameEnded;
		QuickTimeEventManager.Instance.OnSubmitKeycode += QuickTimeEventManager_OnSubmitKeycode;
	}

	private void QuickTimeEventManager_OnSubmitKeycode(bool success, int index) {
		Color color = success ? Color.green : Color.red;
		patternImages[index].color = color;
	}

	private void QuickTimeEventManager_OnMinigameEnded(bool success) {
		StartCoroutine(HideContainer());
		string text = success == true ? winText : loseText;
		background.color = success == true ? Color.green : Color.red;
		ShowMessage(text);
	}

	private void QuickTimeEventManager_OnMinigameStarted(System.Collections.Generic.List<KeyCode> obj) {
		foreach (Image image in patternImages) {
			image.color = Color.black;
		}
		background.color = Color.gray;
		container.SetActive(true);
		for (int i = 0; i < patternTexts.Length; i++) {
			patternTexts[i].text = ConvertKeyCodeToString(obj[i]);
		}
	}

	private IEnumerator HideContainer() {
		yield return new WaitForSeconds(1.2f);
		messageText.gameObject.SetActive(false);
		container.SetActive(false);
	}

	private void ShowMessage(string message) {
		messageText.text = message;
		messageText.gameObject.SetActive(true);
	}

	private string ConvertKeyCodeToString(KeyCode keyCode) {
		switch (keyCode) {
			case KeyCode.E:
				return "E";
			case KeyCode.R:
				return "R";
			case KeyCode.T:
				return "T";
			case KeyCode.Q:
				return "Q";
		}
		Debug.LogError("Something is not right... Try pressing ESC");
		return null;
	}
}
