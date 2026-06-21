
using bananplayss;
using QFSW.QC;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public enum AnimatronicType {
	Freddy,
	Bonnie,
	Chica,
	Foxy,
	GoldenFreddy
}
public class AnimatronicBehaviour : NetworkBehaviour {
	public event Action OnFreddyLaugh;

	[SerializeField] private float movementOppTime = 5f;
	[Range(0, 20)]
	[SerializeField] private int aiLevel;
	[SerializeField] AnimatronicWaypoint currentWaypoint;
	[SerializeField] AnimatronicWaypoint kitchenWaypoint;
	[SerializeField] AnimatronicWaypoint storageWaypoint;
	[SerializeField] ChicaKitchen chicaKitchen;
	[SerializeField] OfficeDoor officeDoor;
	[SerializeField] private NavMeshAgent agent;
	[SerializeField] private Transform neckIk;
	[SerializeField] private Transform eyesIk;
	[SerializeField] private bool isFreddy = false;
	[SerializeField] private AnimatronicType animatronicType;
	[SerializeField] private GameObject skin;


	private AnimatronicWaypoint startingWaypoint;
	private AnimatronicWaypoint lastWaypoint = null;
	private MultiAimConstraint neckIkConstraint, eyesIkConstraint;
	private Transform headRotTransform;
	private Transform eyesRotTransform;
	private int maxAiLevel = 20;
	private float timer = 0f;
	private int moveTimes = 0;
	private bool canMove = true;
	private Animator anim;
	private bool atDoor = false;
	private bool waitingForJumpscare = false;
	private bool isStalling = false;
	private float theyAreHereTimer = 0f;
	private bool chaseMode = false;
	private float chaseSpeed;
	private float normalSpeed;
	private Transform playerToChaseDown;
	private float cankillCooldown = 5f;
	private bool canKill = false;
	private float killDistance = 2f;
	private float warnDistance = 6.5f;
	private bool isStunned = false;

	private void Update() {
		if (moveTimes > 0 && !canKill) {
			cankillCooldown -= Time.deltaTime;
			if (cankillCooldown <= 0f) {
				cankillCooldown = 0f;
				canKill = true;
			}
		}
		if (moveTimes > 0 || isStunned) {
			Collider[] colliders = Physics.OverlapSphere(transform.position, warnDistance);
			foreach (Collider collider in colliders) {
				if (collider.CompareTag("Player")) {
					float distance = Vector3.Distance(transform.position, collider.transform.position);
					if (distance < killDistance) {
						if (canKill) {
							JumpscareClientRpc(collider.GetComponent<NetworkObject>());
						}
					} else {
						WarnPlayerClientRpc(collider.GetComponent<NetworkObject>(), distance);
					}
				}
			}
		}

		if (!IsHost) return;
		if (chaseMode) {
			canMove = false;
			ChaseDownRandomPlayer();
			if(playerToChaseDown != null) {
				float jumpscareDist = 1.3f;
				if(Vector3.Distance(transform.position, playerToChaseDown.position) < jumpscareDist) {
					JumpscareClientRpc(playerToChaseDown.GetComponent<NetworkObject>());
					playerToChaseDown = null;
				}
			}
			if (agent.velocity != Vector3.zero) {
				agent.updateRotation = true;
				anim.SetBool("isMoving", true);
				agent.speed = chaseSpeed;
				agent.acceleration = chaseSpeed / 2;
				anim.speed = 2f;
			}
			return;
		}

		if (theyAreHereTimer > 0) {
			theyAreHereTimer -= Time.deltaTime;
			if(theyAreHereTimer < 0) {
				theyAreHereTimer = 0;
			}
		}
		if (canMove) {
			timer += Time.deltaTime;
			if (timer >= movementOppTime) {
				timer = 0f;
				HandleChanceToMove();
			}
		}

		if (agent.velocity != Vector3.zero) {
			agent.updateRotation = true;
			anim.SetBool("isMoving", true);
			canMove = false;

			neckIkConstraint.weight = Mathf.MoveTowards(neckIkConstraint.weight, 0, Time.deltaTime * 1.7f);
			eyesIkConstraint.weight = Mathf.MoveTowards(eyesIkConstraint.weight, 0, Time.deltaTime * 1.7f);

		} else {
			agent.updateRotation = false;
			if(lastWaypoint != null) {
				transform.forward = Vector3.MoveTowards(transform.forward, lastWaypoint.transform.forward, Time.deltaTime * 7f);
			}
			anim.SetBool("isMoving", false);
			canMove = true;
			if (headRotTransform != null) {
				neckIkConstraint.weight = Mathf.MoveTowards(eyesIkConstraint.weight, 1, Time.deltaTime * 2f);
				neckIk.position = Vector3.MoveTowards(neckIk.position, headRotTransform.position, Time.deltaTime * 3.5f);
			}
			if(eyesRotTransform != null) {
				eyesIkConstraint.weight = Mathf.MoveTowards(eyesIkConstraint.weight, 1, Time.deltaTime * 2f);
				eyesIk.position = Vector3.MoveTowards(eyesIk.position, eyesRotTransform.position, Time.deltaTime * 3.5f);
			} 
		}
	}

	[ClientRpc]
	private void WarnPlayerClientRpc(NetworkObjectReference no, float distance) {
		no.TryGet(out NetworkObject player);
		player.GetComponent<PlayerWarn>().WarnPlayer(distance);
	}

	[ClientRpc]
	private void JumpscareClientRpc(NetworkObjectReference nor) {
		nor.TryGet(out NetworkObject no);
		Jumpscare(no.transform);
	}

	public void IncreaseAILevel(int amount) {
		aiLevel += amount;
		if (aiLevel > maxAiLevel) {
			aiLevel = maxAiLevel;
		}
	}

	private void Start() {
		GameStateManager.Instance.OnElectricityOut += GameStateManager_OnElectricityOut;
		GameManager.Instance.OnFlickerDone += GameManager_OnFlickerDone;
		anim = GetComponent<Animator>();
		neckIkConstraint = neckIk.GetComponent<MultiAimConstraint>();
		eyesIkConstraint = eyesIk.GetComponent<MultiAimConstraint>();
		CameraViewManager.Instance.OnCameraViewChanged += CameraViewManager_OnCameraViewChanged;
		CameraViewManager.Instance.OnEnterCamera += Instance_OnEnterCamera;
		CameraViewManager.Instance.OnExitCamera += CameraViewManager_OnExitCamera;
		OfficeAreaManager.Instance.OnStartBerserkMode += OfficeAreaManager_OnStartBerserkMode;
		OfficeAreaManager.Instance.OnEndBerserkMode += OfficeAreaManager_OnEndBerserkMode;
		startingWaypoint = currentWaypoint;
		officeDoor.OnFlash += OfficeDoor_OnFlash;
		GameManager.Instance.OnStunAnimatronics += Instance_OnStunAnimatronics;
		GameManager.Instance.OnLureBonnie += Instance_OnLureBonnie;
		GameManager.Instance.OnLureChica += Instance_OnLureChica;
		GameManager.Instance.OnFreddy += Instance_OnFreddy;
		normalSpeed = agent.speed;
		chaseSpeed = normalSpeed * 2f;
	}

	private void Instance_OnFreddy() {
		if(isFreddy) {
			int aiLevelIncrease = 2;
			aiLevel+= aiLevelIncrease;
		}
	}

	private void Instance_OnLureChica() {
		if(animatronicType == AnimatronicType.Chica) {
			currentWaypoint = kitchenWaypoint;
		}
	}

	private void Instance_OnLureBonnie() {
		if(animatronicType == AnimatronicType.Bonnie) {
			currentWaypoint = storageWaypoint;
		}
	}

	private void Instance_OnStunAnimatronics() {
		StartCoroutine(StunAnimatronics());
	}

	private IEnumerator StunAnimatronics() {
		canMove = false;
		isStunned = true;
		yield return new WaitForSeconds(15f);
		canMove = true;
	}

	private void Instance_OnEnterCamera() {
		if (!IsHost) return;
		if (atDoor && isFreddy) {
			if (CameraViewManager.Instance.IsOnLastFreddyCam()) {
				isStalling = true;
				StopCoroutine(StallFreddy());
				StartCoroutine(StallFreddy());
				Debug.Log("Stalling freddy");
			} else {
				isStalling = false;
				Debug.Log("Not stalling freddy");
			}
		}
	}

	private void OfficeAreaManager_OnEndBerserkMode() {
		if (chaseMode) {
			chaseMode = false;
			canMove = true;
			agent.speed = normalSpeed;
			agent.acceleration = 1;
			playerToChaseDown = null;
			agent.Warp(currentWaypoint.transform.position);
		}
	}

	private void OfficeAreaManager_OnStartBerserkMode() {
		chaseMode = true;
		ChaseDownRandomPlayer();
	}

	private void GameStateManager_OnElectricityOut() {
		chaseMode = true;
		ChaseDownRandomPlayer();
	}

	private void ChaseDownRandomPlayer() {
		if(playerToChaseDown == null) {
			playerToChaseDown = GameManager.Instance.GetRandomPlayerFromList();
		} else {
			agent.SetDestination(playerToChaseDown.position);
		}
	}

	private void OfficeDoor_OnFlash() {
		if (atDoor && theyAreHereTimer <= 0 && !isFreddy) {
			AudioManager.Instance.PlaySound(Sound.TheyAreHere);
			theyAreHereTimer = 10f;
		}
	} 

	private void CameraViewManager_OnExitCamera() {
		if(waitingForJumpscare && !isFreddy) {
			JumpscareClientRpc(OfficeAreaManager.Instance.GetRandomPlayerInOffice().GetComponent<NetworkObject>());
		}
		isStalling = false;
	}

	private void CameraViewManager_OnCameraViewChanged() {

		if(atDoor && isFreddy) {
			if (CameraViewManager.Instance.IsOnLastFreddyCam() ) {
				isStalling = true;
				StopCoroutine(StallFreddy());
				StartCoroutine(StallFreddy());
				Debug.Log("Stalling freddy");
			} else{
				isStalling = false;
				Debug.Log("Not stalling freddy");
			}
		}
	}

	private IEnumerator StallFreddy() {
		canMove = false;
		float randVal = UnityEngine.Random.Range(0.16f, 17f);
		yield return new WaitForSeconds(randVal);
		canMove = true;
	}

	private void ShowSkin() {
		skin.SetActive(true);
	}

	private void HideSkin() {
		skin.SetActive(false);
	}

	private void GameManager_OnFlickerDone() {
		if (GameManager.Instance.HandleAnimatronicMoveOpp(this)) {
			if(currentWaypoint == null) {
				Debug.LogWarning($"No current waypoint set for {gameObject.name}");
				return;
			}
			transform.position = currentWaypoint.transform.position;
			headRotTransform = currentWaypoint.GetHeadRotateTarget();
			eyesRotTransform = currentWaypoint.GetEyesRotateTarget();
			lastWaypoint = currentWaypoint;
			currentWaypoint = currentWaypoint.GetFollowingWaypoint();
			Debug.Log($"Next waypoint: {currentWaypoint.name}");
			moveTimes++;
		}
	}

	private void HandleChanceToMove() {
		int chance = UnityEngine.Random.Range(1, maxAiLevel);
		if (chance <= aiLevel) {
			HandleMovementOppClientRpc((int)animatronicType,moveTimes,isStalling);
		}
	}

	[ClientRpc]
	private void HandleMovementOppClientRpc(int index, int moveTimes, bool isStalling) {
		if ((int)animatronicType != index) return;
		this.isStalling = isStalling;
		this.moveTimes = moveTimes;
		HandleMovementOpp();
	}

	private void HandleMovementOpp() {
		if (isStalling) return;
		if (moveTimes == 0) {
			AudioManager.Instance.ChangeToAmbience2();
			GameManager.Instance.StartLightFlicker(this);
			if (CameraViewManager.Instance.IsGlitchCamsSelected()) {
				GameManager.Instance.PlayCameraGlitchSFX();
				CameraViewManagerUI.Instance.ShowNoise(2.5f);
			}
			
		} else {
			if(waitingForJumpscare) {
				canMove = false;
				JamOfficeEquipment();
				StartCoroutine(TimePullDownCams());
				return;
			}
			else if (atDoor) {
				if (officeDoor.IsDoorClosed()) {
					HandleOnDoorClosed();
				} else {
					if (isFreddy) {
						OnFreddyLaugh?.Invoke();
						StartCoroutine(DelayJumpscare());
					} else {
						waitingForJumpscare = true;
					}
				}
			}
			else if (currentWaypoint.isDoor) { atDoor = true; }
			if (isFreddy) {
				canMove = false;
				StartCoroutine(DelayFreddyMovement());
			} else {
				MoveAgent();
				Debug.Log("Moving as normal animatronic: " + gameObject.name);
				OfficeAreaManager.Instance.TryBerserkMode();
			}
		}
	}

	private IEnumerator TimePullDownCams() {
		yield return new WaitForSeconds(15);
		if (GameManager.Instance.inCams) {
			
			Transform playerCam = OfficeAreaManager.Instance.GetRandomPlayerInOffice();
			JumpscareClientRpc(playerCam.GetComponent<NetworkObject>());
			PullDownCams(playerCam.GetComponent<PlayerMovement>().GetPlayerCam());
		} else {
			StartCoroutine(TimePullDownCams());
		}
		
	}

	private void PullDownCams(Transform playerCam) {
		playerCam.GetComponentInChildren<Tablet>().PullDownCams();
	}

	private IEnumerator DelayFreddyMovement() {
		
		yield return new WaitForSeconds(10/aiLevel);
		OnFreddyLaugh?.Invoke();
		MoveAgent();
	}

	private void MoveAgent() {
		agent.SetDestination(currentWaypoint.transform.position);
		headRotTransform = currentWaypoint.GetHeadRotateTarget();
		eyesRotTransform = currentWaypoint.GetEyesRotateTarget();
		lastWaypoint = currentWaypoint;
		currentWaypoint = currentWaypoint.GetFollowingWaypoint();
		if(animatronicType == AnimatronicType.Chica) {
			if(currentWaypoint == kitchenWaypoint) {
				Debug.Log("ChicaKitchen yay");
				StartCoroutine(DelayKitchenSFX());
			}
		}
		moveTimes++;
	}

	private IEnumerator DelayKitchenSFX() {
		yield return new WaitForSeconds(10);
		chicaKitchen.PlayKitchenSFX();
	}

	private void JamOfficeEquipment(bool jam = true) {
		switch (animatronicType) {
			case AnimatronicType.Bonnie:
				OfficeAreaManager.Instance.JamOfficeEquipment(jam, !jam);
				break;
			case AnimatronicType.Chica:
				OfficeAreaManager.Instance.JamOfficeEquipment(!jam, jam);
				break;
			default:
				Debug.Log("This animatronic cannot jam office equipment.");
				break;

		}
	}

	private void HandleOnDoorClosed() {
		atDoor = false;
		lastWaypoint = currentWaypoint;
		currentWaypoint = currentWaypoint.GetPreviousWaypoint();
		headRotTransform = currentWaypoint.GetHeadRotateTarget();
		eyesRotTransform = currentWaypoint.GetEyesRotateTarget();
		agent.SetDestination(currentWaypoint.transform.position);
		Debug.Log("Door is closed so.. I am going back!" + gameObject.name);
	}

	private IEnumerator DelayJumpscare() {
		HideSkin();
		float randomDelay = UnityEngine.Random.Range(1f, 1.6f);
		yield return new WaitForSeconds(randomDelay);
		if (officeDoor.IsDoorClosed()) {
			Debug.Log("Heya! The door is closed for " + gameObject.name);
			HandleOnDoorClosed();
		} else {
			Transform player = OfficeAreaManager.Instance.GetRandomPlayerInOffice();
			if (GameManager.Instance.inCams) {
				PullDownCams(player.GetComponent<PlayerMovement>().GetPlayerCam());
			}
			JumpscareClientRpc(player.GetComponent<NetworkObject>());
		}
		ShowSkin();
	}

	private void Jumpscare(Transform playerToJumpscare) {
		if (playerToJumpscare.GetComponent<PlayerMovement>().IsDead()) return;
		Debug.Log($"Jumpscaring player {playerToJumpscare.name}");
		playerToJumpscare.GetComponent<PlayerAnimation>().PlayJumpscareClip(animatronicType);

		if (chaseMode) {
			playerToChaseDown = null;
			ChaseDownRandomPlayer();
		} else {
			HideSkin();
			Invoke(nameof(ResetAnimatronic), 2f);
		}

		if (IsHost) {
			GameManager.Instance.RemovePlayerFromListServerRpc(playerToJumpscare.GetComponent<NetworkObject>());
		}
	}

	public void ResetAnimatronic() {
		ShowSkin();
		moveTimes = 1;
		currentWaypoint = startingWaypoint;
		lastWaypoint = null;
		atDoor = false;
		waitingForJumpscare = false;
		isStalling = false;
		canMove = true;
		agent.Warp(currentWaypoint.transform.position);
		JamOfficeEquipment(false);
	}

	[Command]
	public void AdminJumpscare(string animatronic) {
		Transform playerToJumpscare = OfficeAreaManager.Instance.GetRandomPlayerInOffice();
		PlayerAnimation pA = playerToJumpscare.GetComponent<PlayerAnimation>();
		GameManager.Instance.RemovePlayerFromList(playerToJumpscare);
		if (animatronic == "Bonnie") {
			pA.PlayJumpscareClip(AnimatronicType.Bonnie);
		}else if(animatronic == "Chica") {
			pA.PlayJumpscareClip(AnimatronicType.Chica);
		} else if (animatronic == "Freddy") {
			pA.PlayJumpscareClip(AnimatronicType.Freddy);
		} else if (animatronic == "Foxy") {
			pA.PlayJumpscareClip(AnimatronicType.Foxy);
		} else if (animatronic == "GoldenFreddy") {
			pA.PlayJumpscareClip(AnimatronicType.GoldenFreddy);
		} else {
			Debug.Log("Invalid animatronic name for jumpscare");
		}

	}
}

