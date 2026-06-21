using System;
using System.Collections;
using UnityEngine;

public class CameraViewRotationHandler : MonoBehaviour
{
	public event Action OnStopRotation;
	public event Action OnStartRotation;
	[SerializeField] private float rotationAngle;

	private AudioSource rotationSFX;
	private float rotationSpeed = .18f;
	private float currentAngle = 0f;
	private float rotationValue = 0f;
	private bool goingRight = true;
	private bool paused = false;

	private void Start() {
		currentAngle = transform.eulerAngles.y;
		rotationSFX = GetComponent<AudioSource>();
		StartRotationSFX();
	}

	private void Update() {
		if(paused) {
			return;
		}
		float targetAngle = transform.eulerAngles.y; 
		if (goingRight) {
			targetAngle += rotationAngle;
		} else {
			targetAngle -= rotationAngle;
		}
		
		float t = Time.deltaTime*rotationSpeed;
		float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, t);
		float angleDelta = Mathf.Abs(Mathf.DeltaAngle(currentAngle, transform.eulerAngles.y));
		rotationValue = angleDelta;
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, angle, transform.eulerAngles.z);
		if(rotationValue >= rotationAngle) {
			StartCoroutine(WaitForNextRotate());
			rotationValue = 0f;
			currentAngle = transform.eulerAngles.y;
			StopRotationSFX();
		}
	}

	private IEnumerator WaitForNextRotate() {
		paused = true;
		rotationValue = 0f;
		float pauseTime = UnityEngine.Random.Range(3f, 6f);
		yield return new WaitForSeconds(pauseTime);
		goingRight = !goingRight;
		paused = false;
		StartRotationSFX();
	}

	public void StopRotationSFX() {
		rotationSFX.Stop();
	}

	public void StartRotationSFX() {
		rotationSFX.Play();
	}

	public void SetSFXVolume(float newVolume) {
		rotationSFX.volume = newVolume;
	}
}
