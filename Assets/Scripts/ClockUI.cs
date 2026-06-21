using QFSW.QC;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ClockUI : NetworkBehaviour {
	public event Action<int> OnHourChanged;
	[SerializeField] private TextMeshProUGUI timeText;

	private float hourInterval = 69f;
	private int currentTime = 0;
	private float timer = 0;

	private NetworkVariable<int> currentTimeSynced = new NetworkVariable<int>();

	private void Start() {
		GameStateManager.Instance.OnGameOver += HideUI;
		GameStateManager.Instance.OnWin += HideUI;
	}

	public override void OnNetworkSpawn() {
		currentTimeSynced.OnValueChanged += OnTimeChanged;
	}

	private void OnTimeChanged(int previous, int current) {
		UpdateTimeUI(current);
	}

	private void HideUI() {
		gameObject.SetActive(false);
	}

	private void Update() {
		if (!IsHost) return;
		timer += Time.deltaTime;
		if (timer >= hourInterval) {
			timer = 0;
			UpdateTime();
		}
	}

	private void UpdateTime() {
		currentTime++;
		OnHourChanged?.Invoke(currentTime);
		if (currentTime == 6) {
			GameStateManager.Instance.SendWinGameClientRpc();
		}
		currentTimeSynced.Value = currentTime;
		UpdateTimeUI(currentTimeSynced.Value);
	}

	private void UpdateTimeUI(int time) {
		timeText.text = time + " AM";
	}

	[Command]
	public void AdminIncrementHour() {
		UpdateTime();
		timer = 0;
	}
}
