using bananplayss;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class FoxyBehaviour : NetworkBehaviour {
	private enum FoxySFX {
		Hum,
		Run,
		Bonk
	}

	private enum PirateCoveState {
		Stage1, Stage2, Stage3, Stage4
	}
	[SerializeField] private PirateCoveState currentState;
	[SerializeField] private float moveOppInterval = 5f;
	[SerializeField] private Transform stageThreePos;
	[SerializeField] private Transform stageFourPos;
	[SerializeField] private OfficeDoor leftDoor;
	[SerializeField] private Transform leftDoorPos;
	[SerializeField] private GameObject skin;
	[SerializeField] private AudioSource[] foxySFXArray;
	[SerializeField] private Transform leftCurtain;
	[SerializeField] private Transform rightCurtain;
	[Range(0, 20)]
	[SerializeField] private int aiLevel;
	private int maxAiLevel = 20;

	private Animator anim;
	private Vector3 originalPos = Vector3.zero;
	private Quaternion originalRot = Quaternion.identity;
	private Quaternion leftCurtainOriginalRot = Quaternion.identity;
	private Quaternion rightCurtainOriginalRot = Quaternion.identity;
	private NavMeshAgent agent;
	private float runDownTimer = 25f;
	private float timer = 0f;
	private bool isStalling = false;
	private bool runningDownHallway = false;
	private float stopDist = 0.2f;
	private float powerToDrain = 1f;
	private float powerDrainIncrement = 5f;
	private bool bonked = false;
	private float singCooldown = 50f;
	private float singTimer = 0;
	private bool sang = false;
	private float warnDistance = 5f;
	private float killDistance = 1.5f;


	private NetworkVariable<int> currentStateSynced = new NetworkVariable<int>();

	private void Start() {
		anim = GetComponent<Animator>();
		originalPos = transform.position;
		originalRot = transform.rotation;
		agent = GetComponent<NavMeshAgent>();
		CameraViewManager.Instance.OnCameraViewChanged += CameraViewManager_OnCameraViewChanged;
		CameraViewManager.Instance.OnExitCamera += CameraViewManager_OnExitCamera;
		CameraViewManager.Instance.OnEnterCamera += Instance_OnEnterCamera;
		currentStateSynced.OnValueChanged += StateChanged;
	}

	private void StateChanged(int previous, int current) {
		currentState = (PirateCoveState)current;
	}

	private void Instance_OnEnterCamera() {
		if (CameraViewManager.Instance.IsOnFoxyCam()) {
			StopCoroutine(StallForRandomDuration());
			isStalling = true;
		} else {
			StartCoroutine(StallForRandomDuration());
		}

		if (CameraViewManager.Instance.IsOnLeftHallwayCam()) {
			if (currentState == PirateCoveState.Stage4) {
				RunDownHallway();
			}
		}
	}

	private void CameraViewManager_OnExitCamera() {
		isStalling = false;
	}

	private void CameraViewManager_OnCameraViewChanged() {
		if (CameraViewManager.Instance.IsOnFoxyCam()) {
			StopCoroutine(StallForRandomDuration());
			isStalling = true;
		} else {
			StartCoroutine(StallForRandomDuration());
		}

		if (CameraViewManager.Instance.IsOnLeftHallwayCam()) {
			if (currentState == PirateCoveState.Stage4) {
				RunDownHallway();
			}
		}
	}

	private IEnumerator StallForRandomDuration() {
		float stallDuration = Random.Range(.7f, 17f);
		yield return new WaitForSeconds(stallDuration);
		isStalling = false;
	}

	public void IncreaseAILevel(int amount) {
		aiLevel += amount;
		if (aiLevel > maxAiLevel) {
			aiLevel = maxAiLevel;
		}
	}

	private void Update() {
		if (!IsOwner) return;
		if (agent.velocity != Vector3.zero) {
			anim.SetBool("isMoving", true);
		} else {
			anim.SetBool("isMoving", false);
		}

		
		if (currentState == PirateCoveState.Stage4) {
			Collider[] colliders = Physics.OverlapSphere(transform.position, warnDistance);
			foreach (Collider collider in colliders) {
				if (collider.CompareTag("Player")) {
					float distance = Vector3.Distance(transform.position, collider.transform.position);
					if (distance < killDistance) {
						Jumpscare(collider.transform);
					} else {
						collider.GetComponent<PlayerWarn>().WarnPlayer(distance);
					}
				}
			}
		}

		if (!IsHost) return;

		if (sang) {
			singTimer += Time.deltaTime;
			if (singTimer >= singCooldown) {
				singTimer = 0f;
				sang = false;
			}
		}

			if (runningDownHallway) {
			if (agent.velocity == Vector3.zero) {
				if(Vector3.Distance(transform.position, leftDoorPos.transform.position) < stopDist) {
					if (leftDoor.IsDoorClosed()){
						if (!bonked) {
							PlaySFXServerRpc((int)FoxySFX.Bonk);
							PowerManager.Instance.DrainPowerOnBonk(powerToDrain);
							powerToDrain += powerDrainIncrement;
							float resetDelay = 2.5f;
							Invoke(nameof(ResetAnimatronic), resetDelay);
							bonked = true;
						}
						
					} else {
						if(OfficeAreaManager.Instance.GetRandomPlayerInOffice() == null) {
							ResetAnimatronic();
							OfficeAreaManager.Instance.StartBerserkModeClientRpc();
							return;
						}
						JumpscareClientRpc(OfficeAreaManager.Instance.GetRandomPlayerInOffice().GetComponent<NetworkObject>());
						runningDownHallway = false;
					}
				}
				return;
			}
		}
			if (currentState == PirateCoveState.Stage4) {
				timer += Time.deltaTime;
				if (timer >= runDownTimer) {
					RunDownHallway();
				}
			}
			if (isStalling || currentState == PirateCoveState.Stage4) return;
			timer += Time.deltaTime;
			if (timer >= moveOppInterval) {
				timer = 0f;
				HandleMoveOpp();
			}
		}

	private void HandleMoveOpp() {
		if (isStalling) return;
		int randomValue = Random.Range(0, 20);
		if (randomValue < aiLevel) {
			AdvanceState();
		} else {
			if (currentState == PirateCoveState.Stage1) {
				int humChance = 2;
				randomValue = Random.Range(0, 20);
				if (randomValue <= humChance && !sang) {
					PlaySFXServerRpc((int)FoxySFX.Hum);
					sang = true;
				}
			}
		}
	}

	[ClientRpc]
	private void JumpscareClientRpc(NetworkObjectReference nor) {
		nor.TryGet(out NetworkObject no);
		Jumpscare(no.transform);
	}

	private void RunDownHallway() {
		PlaySFXServerRpc((int)FoxySFX.Run);
		agent.SetDestination(leftDoorPos.position);
		runningDownHallway = true;
	}

	[ServerRpc(RequireOwnership = false)]
	private void PlaySFXServerRpc(int index) {
		PlaySFXClientRpc(index);
	}

	[ClientRpc]
	private void PlaySFXClientRpc(int index) {
		foxySFXArray[index].Play();
	}

	private void AdvanceState() {
		if (!IsHost) return;
		currentState = currentState + 1;
		if(currentState == PirateCoveState.Stage2) {
			rightCurtain.localRotation = Quaternion.Euler(0, 0, -55);
			leftCurtain.localRotation = Quaternion.Euler(0, 0, 85);
		}
		if (currentState == PirateCoveState.Stage3) {
			transform.position = stageThreePos.position;
			transform.forward = stageThreePos.forward;
			rightCurtain.localRotation = Quaternion.Euler(0, 0, -90);
			leftCurtain.localRotation = Quaternion.Euler(0, 0, 110);
		}
		if(currentState == PirateCoveState.Stage4) {
			transform.position = stageFourPos.position;
			transform.forward = stageFourPos.forward;
			timer = 0;
		}
	}

	private void Jumpscare(Transform playerToJumpscare) {
		if (playerToJumpscare.GetComponent<PlayerMovement>().IsDead()) return;
		Debug.Log($"Jumpscaring player {playerToJumpscare.name}");
		playerToJumpscare.GetComponent<PlayerAnimation>().PlayJumpscareClip(AnimatronicType.Foxy);
		HideSkin();
		GameManager.Instance.RemovePlayerFromList(playerToJumpscare);
		float resetDelay = 2f;
		Invoke(nameof(ResetAnimatronic), resetDelay);
	}

	private void HideSkin() {
		skin.SetActive(false);
	}

	private void ShowSkin() {
		skin.SetActive(true);
	}

	private void ResetAnimatronic() {
		agent.Warp(originalPos);
		bonked = false;
		transform.rotation = originalRot;
		runningDownHallway = false;
		int randomInt = randomInt = Random.Range(0, 2);
		currentState = (PirateCoveState)randomInt;
		if(currentState == PirateCoveState.Stage2) {
			rightCurtain.localRotation = Quaternion.Euler(0, 0, -55);
			leftCurtain.localRotation = Quaternion.Euler(0, 0, 85);
		} else {
			rightCurtain.localRotation = rightCurtainOriginalRot;
			leftCurtain.localRotation = leftCurtainOriginalRot;
		}
		ShowSkin();
	}
}
