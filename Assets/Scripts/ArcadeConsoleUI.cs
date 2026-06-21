
using Mono.CSharp;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CommandEnum {
	Error,
	OfficeLockdown,
	SoundTest,
	CamRecalib,
	StunAnimatronics,
	LureBonnie,
	LureChica,
	Freddy
}

public class ArcadeConsoleUI : MonoBehaviour
{
	public static ArcadeConsoleUI Instance {  get; private set; }

	[SerializeField] private Button officeLockdownButton;
	[SerializeField] private Button soundTestButton;
	[SerializeField] private Button camRecalibButton;
	[SerializeField] private Button stunAnimatronicsButton;
	[SerializeField] private Button lureBonnieButton;
	[SerializeField] private Button lureChicaButton;
	[SerializeField] private Button freddyButton;
	[SerializeField] private Button exitButton;
	[SerializeField] private GameObject executingGo;
	[SerializeField] private GameObject container;
	[SerializeField] private TextMeshProUGUI messageText;

	private bool canExecute = true;
	private float commandCooldown = 10f;

	private bool luredBonnie = false;
	private bool luredChica = false;
	private bool playedSoundTest = false;
	private bool electricityOut = false;

	public bool IsActive() {
		return container.activeSelf;
	}

	public void Show() {
		if (electricityOut) return;
		EventSystem.current.SetSelectedGameObject(exitButton.gameObject);
		Debug.Log(EventSystem.current.currentSelectedGameObject.name);
		CameraViewManagerUI.Instance.ShowNoise(.7f);
		container.SetActive(true);
	}

	public void Hide() {
		container.SetActive(false);
	}

	private void Update() {
		if (IsActive() && Input.GetKeyDown(KeyCode.Escape)) {
			Hide();
		}
	}

	private void Awake() {
		Instance = this;

		exitButton.onClick.AddListener(() => {
			Hide();
		});
		officeLockdownButton.onClick.AddListener(() => {
			ExecuteCommand(CommandEnum.OfficeLockdown);
		});
		soundTestButton.onClick.AddListener(() => {
			if (playedSoundTest) ExecuteCommand(CommandEnum.Error);
			playedSoundTest = true;
			ExecuteCommand(CommandEnum.SoundTest);
		});
		camRecalibButton.onClick.AddListener(() => {
			ExecuteCommand(CommandEnum.CamRecalib);
		});
		stunAnimatronicsButton.onClick.AddListener(() => {
			ExecuteCommand(CommandEnum.StunAnimatronics);
		});
		lureBonnieButton.onClick.AddListener(() => {
			if(luredBonnie) ExecuteCommand(CommandEnum.Error);
			luredBonnie = true;
			ExecuteCommand(CommandEnum.LureBonnie);
		});
		lureChicaButton.onClick.AddListener(() => {
			if (luredChica) ExecuteCommand(CommandEnum.Error);
			luredChica = true;
			ExecuteCommand(CommandEnum.LureChica);
		});
		freddyButton.onClick.AddListener(() => {
			ExecuteCommand(CommandEnum.Freddy);
		});
	}

	private void Start() {
		GameStateManager.Instance.OnElectricityOut += Instance_OnElectricityOut;
	}

	private void Instance_OnElectricityOut() {
		electricityOut = true;
	}

	private void ExecuteCommand(CommandEnum command) {
		string message = "";
		if (!canExecute) {
			message = $"Command execution is on a {commandCooldown}s cooldown...";
			ShowMessage(message);
			return;
		}
		messageText.gameObject.SetActive(false);
		executingGo.SetActive(true);

		switch (command) {
			case CommandEnum.Error:
				message = "Error executing command...";
				break;
			case CommandEnum.OfficeLockdown:
				message = "Office put on lockdown...";
				GameManager.Instance.OffliceLockDownServerRpc();
				break;
			case CommandEnum.SoundTest:
				message = "Sending soundTest...";
				GameManager.Instance.SoundTestServerRpc();
				break;
			case CommandEnum.CamRecalib:
				message = "The cameras are being recalibrated...";
				GameManager.Instance.CamRecalibrateServerRpc();
				break;
			case CommandEnum.StunAnimatronics:
				message = "Animatronics stunned for 15 seconds...";
				GameManager.Instance.StunAnimatronicsServerRpc();
				break;
			case CommandEnum.LureBonnie:
				message = "Luring Bonnie to storage room...";
				GameManager.Instance.LureBonnieServerRpc();
			break;
			case CommandEnum.LureChica:
				message = "Luring Chica to kitchen...";
				GameManager.Instance.LureChicaServerRpc();
			break;
				case CommandEnum.Freddy:
				message = "Successfully increased Freddy's aggression...";
				GameManager.Instance.FreddyAggressionServerRpc();
				break;
		}
		canExecute = false;
		StartCoroutine(CooldownExecution());
		StartCoroutine(DelayShowMessage(message));
	}

	private IEnumerator CooldownExecution() {
		yield return new WaitForSeconds(commandCooldown);
		canExecute = true;
	}

	private IEnumerator DelayShowMessage(string message) {
		yield return new WaitForSeconds(1);
		ShowMessage(message);
		executingGo.SetActive(false);
	}

	private void ShowMessage(string message) {
		messageText.text = message;
	}
	
}
