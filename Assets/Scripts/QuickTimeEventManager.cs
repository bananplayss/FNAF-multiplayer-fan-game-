
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class QuickTimeEventManager : NetworkBehaviour
{
    public static QuickTimeEventManager Instance { get; private set; }
	private bool minigameStarted = false;
	public event Action<List<KeyCode>> OnMinigameStarted;
	public event Action<bool> OnMinigameEnded;
	public event Action<bool, int> OnSubmitKeycode;
	private List<KeyCode> pattern = new List<KeyCode>();
	private GameObject oilSplashOnStake;

	private int currentIndex = 0;

	private void Awake() {
		Instance = this;
	}

	public void StartMinigame(GameObject oilSplashOnStake) {
		if(minigameStarted) return;
		this.oilSplashOnStake = oilSplashOnStake;
		minigameStarted = true;
		currentIndex = 0;
		pattern = GeneratePattern();
		OnMinigameStarted?.Invoke(pattern);
	}

	private void Update() {
		if(minigameStarted) {
			if (Input.GetKey(KeyCode.Escape)) {
				OnMinigameEnded?.Invoke(false);
				minigameStarted=false;
			}
			if (Input.GetKeyDown(KeyCode.E)) {
				CheckForInput(KeyCode.E);
			}
			if (Input.GetKeyDown(KeyCode.R)) {
				CheckForInput(KeyCode.R);
			}
			if (Input.GetKeyDown(KeyCode.T)) {
				CheckForInput(KeyCode.T);
			}
			if (Input.GetKeyDown(KeyCode.Q)) {
				CheckForInput(KeyCode.Q);
			}
		}
	}

	private bool CheckForInput(KeyCode keyCode) {
		if (pattern[currentIndex] == keyCode) {
			OnSubmitKeycode?.Invoke(true, currentIndex);
			currentIndex++;
			if (currentIndex == 5) {
				OnMinigameEnded?.Invoke(true);
				minigameStarted = false;
				oilSplashOnStake.GetComponent<OilSplash>().ReleaseLastSlow();
				DisableOilSplashServerRpc(oilSplashOnStake.GetComponent<NetworkObject>());
				return true;
			}
			return true;
		} else {
			OnSubmitKeycode?.Invoke(false, currentIndex);
			OnMinigameEnded?.Invoke(false);
			minigameStarted = false;
			return false;
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void DisableOilSplashServerRpc(NetworkObjectReference no) {
		DisableOilSplashClientRpc(no);
	}

	[ClientRpc]
	private void DisableOilSplashClientRpc(NetworkObjectReference no) {
		no.TryGet(out NetworkObject nobject);
		nobject.gameObject.SetActive(false);
	}

	private List<KeyCode> GeneratePattern() {
		int randomE = UnityEngine.Random.Range(2,3);
		int randomR = UnityEngine.Random.Range(1,3);
		int randomT = UnityEngine.Random.Range(1,3);
		int randomQ = UnityEngine.Random.Range(1,3);

		List<KeyCode> pattern = new List<KeyCode>();

		for (int i = 0; i < randomE; i++) {
			pattern.Add(KeyCode.E);
		}
		for (int i = 0; i < randomR; i++) {
			pattern.Add(KeyCode.R);
		}
		for (int i = 0; i < randomT; i++) {
			pattern.Add(KeyCode.T);
		}
		for (int i = 0; i < randomQ; i++) {
			pattern.Add(KeyCode.Q);
		}
		
		pattern.Shuffle();
		return pattern;
	}

	
}

public static class ThreadSafeRandom {
	[ThreadStatic] private static System.Random Local;

	public static System.Random ThisThreadsRandom {
		get { return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
	}
}

static class MyExtensions {
	public static void Shuffle<T>(this IList<T> list) {
		int n = list.Count;
		while (n > 1) {
			n--;
			int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
}
