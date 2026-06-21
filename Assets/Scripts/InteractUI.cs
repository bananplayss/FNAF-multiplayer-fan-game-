using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractUI : MonoBehaviour
{
    public static InteractUI Instance {  get; private set; }

	[SerializeField] private Image fill;
	[SerializeField] private CanvasGroup messageText;

	private void Awake() {
		 Instance = this;
	}
	public void SetInteractValue(float value) {
		fill.fillAmount = value;
    }

	public void ShowMessage(string mes) {
		messageText.GetComponent<TextMeshProUGUI>().text = mes;
		LeanTween.alphaCanvas(messageText, 1, 1f);
		StartCoroutine(FadeMessageText());
	}

	private IEnumerator FadeMessageText() {
		yield return new WaitForSeconds(3);
		LeanTween.alphaCanvas(messageText, 0, 2.5f);
	}

}
