
using System;
using Unity.Netcode;
using UnityEngine;


public class PlayerAnimation : NetworkBehaviour
{
	public event Action OnJumpscare;
    
	[SerializeField] private Animator anim;

	private float movementValue = 0f;
	private float crossFadeValue = .5f;
	private float blendDampValue = 0.05f;

	public enum AnimationClipEnum {
		Idle,
		ArmStretch,
		BonnieJS,
		ChicaJS,
		FreddyJS,
		FoxyJS,
		GoldenFreddyJS
	}

	[Header("Idle Timer / Arm stretching")]
	private float idleTimer = 0f;
	private float minIdleTime = 15f;

	public void PlayClip(string clipName) {
		anim.CrossFade(clipName, crossFadeValue);
	}
	public void PlayClip(AnimationClipEnum animEnum) {
		anim.CrossFade(animEnum.ToString(), crossFadeValue);
	}

	public void PlayJumpscareClip(AnimationClipEnum animEnum) {
		anim.Play(animEnum.ToString());
	}

	public void SetMovementValue(float newValue) {
		if (!IsOwner) return;
		movementValue = newValue;
	}

	private void Update() {
		if (!IsOwner || anim == null) return;
		HandleIdleTimer();
		SetMovementValue();
	}

	private void HandleIdleTimer() {
		if(movementValue > 0) idleTimer = 0f;
		idleTimer += Time.deltaTime;
		if(idleTimer >= minIdleTime) {
			PlayClip(AnimationClipEnum.ArmStretch);
			idleTimer = 0f;
		}
	}

	private void SetMovementValue() {
		if (!IsOwner) return;
		anim.SetFloat("MovementValue", movementValue, blendDampValue,Time.deltaTime);
	}

	public void PlayJumpscareClip(AnimatronicType animatronicType) {
		if (!IsOwner) return;
		OnJumpscare?.Invoke();
		if (ArcadeConsoleUI.Instance.IsActive()) {
			ArcadeConsoleUI.Instance.Hide();
		}
		PlayJumpscareSFX();
		switch (animatronicType) {
			case AnimatronicType.Bonnie:
				PlayJumpscareClip(AnimationClipEnum.BonnieJS);
				break;
			case AnimatronicType.Chica:
				PlayJumpscareClip(AnimationClipEnum.ChicaJS);
				break;
			case AnimatronicType.Freddy:
				PlayJumpscareClip(AnimationClipEnum.FreddyJS);
				break;
			case AnimatronicType.Foxy:
				PlayJumpscareClip(AnimationClipEnum.FoxyJS);
				break;
			case AnimatronicType.GoldenFreddy:
				PlayJumpscareClip(AnimationClipEnum.GoldenFreddyJS);
				break;
		}
	}

	private void PlayJumpscareSFX() {
		AudioManager.Instance.PlaySound(Sound.Jumpscare, 0.7f);
	}
}
