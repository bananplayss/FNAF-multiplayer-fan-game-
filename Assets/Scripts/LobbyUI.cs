using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : NetworkBehaviour
{
	public event Action<string> OnAuthenticate;
	public event Action OnHostLobby;
	public event Action OnJoinLobby;
	public event Action OnStartLobby;
	public event Action<PlayerRoles> OnUpdatePlayerRole;

	[SerializeField] private Button securityButton;
	[SerializeField] private Button electricianButton;
	[SerializeField] private Button technicianButton;
	[SerializeField] private Button janitorButton;
	[SerializeField] private Button creditsButton;
	[SerializeField] private TextMeshProUGUI yourRoleText;

	[SerializeField] private TextMeshProUGUI playerCountText;
	[SerializeField] private Button startButton;
	[SerializeField] private TextMeshProUGUI startText;

	[SerializeField] private GameObject authenticationContainer;
	[SerializeField] private GameObject lobbyButtonContainer;
	[SerializeField] private GameObject lobbyContainer;
	[SerializeField] private GameObject selectTriangle;
	[SerializeField] private GameObject loadingText;
	[SerializeField] private GameObject creditUI;
	[SerializeField] private TextMeshProUGUI[] playerNamesText;
	[SerializeField] private TextMeshProUGUI lobbyNameText;

	private string[] playerNames = new string[4] {" " , " ", " ", " "};
	string lobbyName = String.Empty;

	[SerializeField] private TMP_InputField playerNameField;

	[Header("Buttons")]
	[SerializeField] private Button authenticateButton;
    [SerializeField] private Button hostLobby;
    [SerializeField] private Button joinLobby;

	int playerCount = 0;

	private void Awake() {
		playerNameField.text = "Player" + UnityEngine.Random.Range(0, 999);

		securityButton.onClick.AddListener(() => {
			MoveTriangle(securityButton.transform.position.x);
			OnUpdatePlayerRole?.Invoke(PlayerRoles.Security);
		});
		electricianButton.onClick.AddListener(() => {
			MoveTriangle(electricianButton.transform.position.x);
			OnUpdatePlayerRole?.Invoke(PlayerRoles.Electrician);
		});
		technicianButton.onClick.AddListener(() => {
			MoveTriangle(technicianButton.transform.position.x);
			OnUpdatePlayerRole?.Invoke(PlayerRoles.Technician);
		});
		janitorButton.onClick.AddListener(() => {
			MoveTriangle(janitorButton.transform.position.x);
			OnUpdatePlayerRole?.Invoke(PlayerRoles.Janitor);
		});

		authenticateButton.onClick.AddListener(() => {
			Debug.Log("Authenticated");
			Authenticate();
		});
		hostLobby.onClick.AddListener(() => {
			OnHostLobby?.Invoke();
			ShowLobby();
		});
		joinLobby.onClick.AddListener(() => {
			OnJoinLobby?.Invoke();
			ShowLobby();
		});
		startButton.onClick.AddListener(() => {
			StartLobby();
		});
		creditsButton.onClick.AddListener(() => {
			ToggleCredits();
		});
	}

	private void ToggleCredits() {
		creditUI.SetActive(!creditUI.activeSelf);
	}

	private void Start() {
		LobbyManager.Instance.OnUpdateLobbyPlayerCount += LobbyManager_OnPlayerJoinedLobby;
		LobbyManager.Instance.OnUpdatePlayerRole += LobbyManager_OnUpdatePlayerRole;
		LobbyManager.Instance.OnPlayerJoinedLobby += Instance_OnPlayerJoinedLobby;

		NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
	}

	private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
		NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneAt(0));
	}

	private void Singleton_OnClientConnectedCallback(ulong obj) {
		UpdateLobbyInfoServerRpc();
		NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
	}

	private void Instance_OnPlayerJoinedLobby(string name) {
		PlayerJoinedLobbyServerRpc(name);
	}

	[ServerRpc(RequireOwnership = false)]
	private void PlayerJoinedLobbyServerRpc(string playerName) {
		playerNames[playerCount] = playerName;
	}

	[ServerRpc(RequireOwnership = false)]
	private void UpdateLobbyInfoServerRpc() {
		UpdatePlayerNameTextsClientRpc(playerNames[0], playerNames[1], playerNames[2], playerNames[3]);
	}

	[ClientRpc]
	private void UpdatePlayerNameTextsClientRpc(string player1, string player2, string player3, string player4) {
		playerNamesText[0].text = player1;
		playerNamesText[1].text = player2;
		playerNamesText[2].text = player3;
		playerNamesText[3].text = player4;
	}

	private void LobbyManager_OnUpdatePlayerRole(PlayerRoles role) {
		PlayerRole.Instance.SetRole(role);
		UpdateYourRoleText();
	}

	private void UpdateYourRoleText() {
		yourRoleText.text = $"Your role is {PlayerRole.Instance.GetRole()}";
	}

	private void Authenticate() {
		OnAuthenticate?.Invoke(playerNameField.text);
		authenticationContainer.SetActive(false);
		lobbyButtonContainer.SetActive(true);
	}

	private void MoveTriangle(float x) {
		selectTriangle.SetActive(true);

		Vector3 pos = selectTriangle.transform.position;
		pos = new Vector3(x, pos.y, pos.z);
		selectTriangle.transform.position = pos;
	}

	private void LobbyManager_OnPlayerJoinedLobby(int playerCount, int maxPlayers, bool isHost) {
		if(!isHost) {
			startButton.gameObject.SetActive(false);
		}
		UpdateLobbyUI(playerCount, maxPlayers, isHost);
	}

	private void UpdateLobbyUI(int playerCount, int maxPlayers, bool isHost) {
		this.playerCount = playerCount;
		playerCountText.text = $"Players: {playerCount}/{maxPlayers}";
		int playersNeeded = 1;
		if(playerCount >= playersNeeded) {
			startText.alpha = 1;
			startButton.enabled = true;
		}
	}

	private void ShowLobby() {
		lobbyButtonContainer?.SetActive(false);
		lobbyContainer.SetActive(true);
	}

	[ClientRpc]
	private void ShowLoadingTextClientRpc() {
		loadingText.gameObject.SetActive(true);
	}

	private void StartLobby() {
		startButton.gameObject.SetActive(false);
		ShowLoadingTextClientRpc();
		OnStartLobby?.Invoke();
		Invoke(nameof(DelayLoadScene), 10f);
	}

	private void DelayLoadScene() {
		NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", LoadSceneMode.Additive);
	}
}
