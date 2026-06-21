using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace bananplayss {
	public class PlayerMovement : NetworkBehaviour {

		public float walkSpeed = 5f;
		public float slowSpeed = 2f;
		float speed;
		private float sprintSpeed = .07f;

		public float mouseSensitivity = 2f;
		private float verticalRotation = 0f;
		
		private float moveHorizontal;
		private float moveVertical;
		private Vector3 movementVector;
		private float timer;
		private float defaultYPos = 0;
		private float headBobSpeedCurrent;
		private float headBobAmountCurrent;
		private float headBobSpeed = 9.3f;
		private float headBobSpeedSprint = 15f;
		private float headBobAmount = 0.0095f;
		private float headBobAmountSprint = 0.015f;

		private bool isBusy = false;
		private bool isDead = false;
		private bool isSlowed = false;

		private float minimumWalkTime = .4f;
		private float walkTimer;

		private NetworkVariable<bool> isDeadSynced = new NetworkVariable<bool> (false, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

		[SerializeField] private CharacterController characterController;
		[SerializeField] private AudioSource footsteps;
		[SerializeField] private Transform cameraSway;
		[SerializeField] private Transform cameraTransform;
		[SerializeField] private SkinnedMeshRenderer[] skinRendererArray;
		[SerializeField] private Tablet tablet;

		[Header("Sway Settings")]
		private float swayAmount = 2f;
		private float smoothAmount = 4f;
		private float maxRotation = 7f;
		Quaternion initialRotation;

		PlayerAnimation playerAnim;

		public void SetIsBusy(bool isBusy) {
			this.isBusy = isBusy;
		}
		public void SlowPlayer() {
			isSlowed = true;
			speed = slowSpeed;
			headBobSpeedCurrent = headBobSpeed/2;
			headBobAmountCurrent = headBobAmount%2;
			footsteps.pitch = 1.2f;
			footsteps.volume = 0.15f;
		}

		public void SlowPlayer(float duration) {
			StartCoroutine(RemoveSlowCoroutine(duration));
		}

		private IEnumerator RemoveSlowCoroutine(float duration) {
			yield return new WaitForSeconds(duration);
			RemoveSlow();
		}

		private void RemoveSlow() {
			speed = walkSpeed;
			isSlowed = false;
		}

		private void Start() {
			if (!IsOwner) {
				GetPlayerCam().gameObject.SetActive(false);
				footsteps.enabled = false;
				return;
			}
			HideSkin();

			initialRotation = cameraSway.localRotation;
			characterController = GetComponent<CharacterController>();
			playerAnim = GetComponent<PlayerAnimation>();
			AddPlayerToListServerRpc(GetComponent<NetworkObject>());
			playerAnim.OnJumpscare += PlayerAnim_OnJumpscare;
			tablet.OnEnterCamera += Tablet_OnEnterCamera;
			tablet.OnExitCamera += Tablet_OnExitCamera;
			GameStateManager.Instance.OnGameOver += Instance_OnGameOver;
		}

		[ServerRpc(RequireOwnership = false)]
		private void AddPlayerToListServerRpc(NetworkObjectReference player) {
			player.TryGet(out NetworkObject playerNo);
			GameManager.Instance.AddPlayer(playerNo.gameObject.transform);
			Debug.Log(playerNo.gameObject.name + " added to list");
		}

		private void Instance_OnGameOver() {
			footsteps.enabled = false;
		}

		private void Tablet_OnExitCamera() {
			isBusy = false;
		}

		private void Tablet_OnEnterCamera() {
			isBusy = true;
			footsteps.enabled = false;
		}

		public bool IsDead() {
			return isDeadSynced.Value;
		}

		private void PlayerAnim_OnJumpscare() {
			isBusy = true;
			cameraTransform.localRotation = Quaternion.Euler(0, 0, 0);
			GameStateManager.Instance.PlayerDieServerRpc();
			PerishAndVanishFromExistence();
		}

		private void PerishAndVanishFromExistence() {
			//If you're reading this... I know what you are. I know what you did. You can't hide from me. I see you.
			//You think you can just come into my code and take a look around?
			//Well, think again. I'm always watching. Always waiting. And when the time is right, I'll be there.
			//So go ahead, keep reading.
			//But remember, I'm always one step ahead.
			isDead = true;
			isDeadSynced.Value = isDead;
			characterController.enabled = false;
			HideSkin();

			Invoke(nameof(DelayOpenCams), 3f);
		}

		private void DelayOpenCams() {
			TabletOverlayUI.Instance.OpenCamsAfterDeath();
		}

		private void HideSkin() {
			foreach (SkinnedMeshRenderer skin in skinRendererArray) {
				skin.enabled = false;
			}
			Invoke(nameof(GoToDeathPos), 2f);
		}

		private void GoToDeathPos() {
			transform.position = DeathPos.Instance.GetDeathPos();
		}

		public Transform GetPlayerCam() {
			return cameraTransform;
		}
		private void MovePlayer() {
			Vector3 move;
			if (characterController.enabled == false) {                                    
				move = Vector3.zero;
				movementVector = Vector3.zero;
				return;
			}
			float inputMagnitude = Mathf.Clamp01(movementVector.magnitude);
			if (!isSlowed) {
				speed = walkSpeed;
				headBobSpeedCurrent = headBobSpeed;
				headBobAmountCurrent = headBobAmount;
				footsteps.pitch = 1.5f;
				footsteps.volume = 0.20f;
			}
			
			if (Input.GetKey(KeyCode.LeftShift) && !isSlowed) {
				footsteps.pitch = 2.1f;
				footsteps.volume = 0.3f;
				inputMagnitude *= 2f;
				speed = sprintSpeed;
				headBobSpeedCurrent = headBobSpeedSprint;
				headBobAmountCurrent = headBobAmountSprint;
			}
			playerAnim.SetMovementValue(inputMagnitude);
			move = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized;
			
			movementVector = move * speed;
			
			if (movementVector != Vector3.zero && characterController.velocity != Vector3.zero) {
				walkTimer += Time.deltaTime;
				if (walkTimer > minimumWalkTime && !isBusy) {
					footsteps.enabled = true;
				}
			} else {
				walkTimer = 0f;
				footsteps.enabled = false;
			}

			Debug.DrawRay(transform.position, -transform.up);

			if(Physics.Raycast(transform.position+new Vector3(0,.4f,0), -transform.up, out RaycastHit hit, 7f)) {
				characterController.Move(-transform.up);
			}
			
			characterController.Move(movementVector);
		}

		private void HandleHeadbob() {
			if (movementVector != Vector3.zero && characterController.velocity != Vector3.zero) {
				timer += Time.deltaTime * headBobSpeedCurrent;
				cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, defaultYPos + Mathf.Sin(timer) * headBobAmountCurrent, cameraTransform.localPosition.z);

				Vector3 velocity = characterController.velocity.normalized;

				Vector3 localVelocity = cameraTransform.InverseTransformDirection(velocity.normalized);

				float swayZ = Mathf.Clamp(-localVelocity.x * swayAmount, -maxRotation, maxRotation);
				float swayX = Mathf.Clamp(localVelocity.z * swayAmount, -maxRotation, maxRotation);

				Quaternion targetRotation = Quaternion.Euler(swayX, 0f, swayZ);

				cameraSway.localRotation = Quaternion.Slerp(cameraSway.localRotation, initialRotation * targetRotation, Time.deltaTime * smoothAmount);
			}
		}

		private void RotateCamera() {
			float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
			transform.Rotate(0, horizontalRotation, 0);

			float maxAngle = 70f;
			verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
			verticalRotation = Mathf.Clamp(verticalRotation, -maxAngle, maxAngle);

			cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

		}

		private void Update() {
			if (isBusy || !IsOwner) return;
			moveHorizontal = Input.GetAxisRaw("Horizontal");
			moveVertical = Input.GetAxisRaw("Vertical");

			RotateCamera();
		}
		private void FixedUpdate() {
			if (isBusy || !IsOwner) return;
			MovePlayer();
			HandleHeadbob();
		}
	}

}
