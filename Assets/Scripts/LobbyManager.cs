using QFSW.QC;
using System.Collections;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;

public class LobbyManager : MonoBehaviour
{
	public static LobbyManager Instance {  get; private set; }

	public event Action<string> OnPlayerJoinedLobby;
	public event Action<string> OnCreateLobby;
	public event Action<int,int, bool> OnUpdateLobbyPlayerCount;
	public event Action<PlayerRoles> OnUpdatePlayerRole;
	[SerializeField] private LobbyUI lobbyUI;
	private Lobby hostLobby;
	private Lobby joinedLobby;
	private string playerName = "";
	private bool isHost = false;


	private void Awake() {
		Instance = this;
	}
	private void Start() {
		lobbyUI.OnAuthenticate += LobbyUI_OnAuthenticate;
		lobbyUI.OnHostLobby += LobbyUI_OnHostLobby;
		lobbyUI.OnJoinLobby += LobbyUI_OnJoinLobby;
		lobbyUI.OnUpdatePlayerRole += LobbyUI_OnUpdatePlayerRole;

		NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	private void Singleton_OnClientConnectedCallback(ulong obj) {
		OnPlayerJoinedLobby?.Invoke(playerName);
	}

	private async void LobbyUI_OnUpdatePlayerRole(PlayerRoles newRole) {
		try {
			await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
				Data = new Dictionary<string, PlayerDataObject> {
				{"PlayerRole", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,newRole.ToString())}
			}
		});
			OnUpdatePlayerRole(newRole);
		} catch (LobbyServiceException e) {
			Debug.Log(e.ToString());
		}
	}

	private void LobbyUI_OnAuthenticate(string playerName) {
		Authenticate(playerName);
	}

	private void LobbyUI_OnJoinLobby() {
		QuickJoinLobby();
	}

	private void LobbyUI_OnHostLobby() {
		CreateLobby();
	}

	private async void Authenticate(string playerName) {
		this.playerName = playerName;
		InitializationOptions options = new InitializationOptions();
		options.SetProfile(playerName.Trim());

		await UnityServices.InitializeAsync();

		AuthenticationService.Instance.SignedIn += () => {
			Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
		};

		await AuthenticationService.Instance.SignInAnonymouslyAsync();
	}

	[Command]
	private async void CreateLobby() {
		try {

			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
				allocation.RelayServer.IpV4,
				(ushort)allocation.RelayServer.Port,
				allocation.AllocationIdBytes,
				allocation.Key,
				allocation.ConnectionData
				);
			NetworkManager.Singleton.StartHost();

			int maxPlayers = 4;
			CreateLobbyOptions options = new CreateLobbyOptions {
				IsPrivate = false,
				Data = new Dictionary<string, DataObject> {
					{"RelayCode",new DataObject(DataObject.VisibilityOptions.Member,joinCode) }
				},
				Player = GetPlayer(),
			};

			Debug.Log(joinCode); 
			string lobbyName = playerName + "s Lobby";
			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
			hostLobby = lobby;
			joinedLobby = hostLobby;

			StartCoroutine(LobbyHeartbeatCoroutine());
			StartCoroutine(UpdateLobbyCoroutine());
			isHost = AuthenticationService.Instance.PlayerId == joinedLobby.HostId;
			Debug.Log($"Your playername is {playerName}");
			OnPlayerJoinedLobby?.Invoke(playerName);
			OnUpdateLobbyPlayerCount?.Invoke(joinedLobby.Players.Count,joinedLobby.MaxPlayers, isHost);
			
			Debug.Log("Created Lobby: " + lobby.Name + " with code: " + lobby.LobbyCode);
			PrintPlayers(hostLobby);
		} catch (LobbyServiceException e) {
			Debug.Log(e);
		}
	}

	private async void AutoJoinRelay() {
		try {
			Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
			string joinCode = lobby.Data["RelayCode"].Value;
			JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

			NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
				joinAllocation.RelayServer.IpV4,
				(ushort)joinAllocation.RelayServer.Port,
				joinAllocation.AllocationIdBytes,
				joinAllocation.Key,
				joinAllocation.ConnectionData,
				joinAllocation.HostConnectionData
				);
			NetworkManager.Singleton.StartClient();
			
		} catch (RelayServiceException e) {
			Debug.Log(e);
		}
	}

	[Command]
	private async void ListLobbies() {
		try {
			QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

			Debug.Log("Lobbies found: " + queryResponse.Results.Count);

			foreach (Lobby lobby in queryResponse.Results) {
				Debug.Log(lobby.Name);
			}
		} catch (LobbyServiceException e) {
			Debug.Log(e);
		}
	}

	[Command]
	private async void QuickJoinLobby() {
		try {
			QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions {
				Player = GetPlayer()
			};
			Lobby quickJoinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
			joinedLobby = quickJoinedLobby;

			isHost = AuthenticationService.Instance.PlayerId == joinedLobby.HostId;
			OnUpdateLobbyPlayerCount?.Invoke(joinedLobby.Players.Count, joinedLobby.MaxPlayers, isHost);
			AutoJoinRelay();
			Debug.Log("Joined lobby");
			
			PrintPlayers(quickJoinedLobby);
		} catch(LobbyServiceException e) {
			Debug.Log(e);
		}
	}

	private Player GetPlayer() {
		return new Player {
			Data = new Dictionary<string, PlayerDataObject> {
				{ "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
			}
		};
	}

	private IEnumerator LobbyHeartbeatCoroutine() {
		if(hostLobby != null) {
			float heartBeatInterval = 15f;
			yield return new WaitForSeconds(heartBeatInterval);

			LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
			Debug.Log("Sent heartbeat to " + hostLobby.Id);
			StartCoroutine(LobbyHeartbeatCoroutine());
		}
	}

	private  IEnumerator UpdateLobbyCoroutine() {
		if (hostLobby != null) {
			float updateLobbyInterval = 1.2f;
			yield return new WaitForSeconds(updateLobbyInterval);
			UpdateLobby();
			StartCoroutine(UpdateLobbyCoroutine());
		}
	}

	private async void UpdateLobby() {
		Lobby lobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
		OnUpdateLobbyPlayerCount?.Invoke(joinedLobby.Players.Count, joinedLobby.MaxPlayers, isHost);
		joinedLobby = lobby;
	}

	[Command]
	private async void UpdatePlayerName(string newPlayerName) {
		try {
			playerName = newPlayerName;
			await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
				Data = new Dictionary<string, PlayerDataObject> {
				{"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
			}
			}
			);
		}catch(LobbyServiceException e) {
			Debug.Log(e.ToString());
		}
	}

	[Command]
	private async void LeaveLobby() {
		try {
			await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
		} catch(LobbyServiceException e) {
			Debug.Log(e);
		}
	}

	[Command]
	private async void DeleteLobby() {
		try {
			await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
		}catch(LobbyServiceException e) {
			Debug.Log(e);
		}
	}

	[Command]
	private void PrintPlayers() {
		PrintPlayers(joinedLobby);
	}

	private void PrintPlayers(Lobby lobby) {
		Debug.Log($"There are {lobby.Players.Count} players in {lobby.Name}");
		foreach (Player player in lobby.Players) { 
			Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
		}
	}
}
