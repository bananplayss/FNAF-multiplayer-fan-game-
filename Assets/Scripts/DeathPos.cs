using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPos : MonoBehaviour
{

    public static DeathPos Instance { get; private set; }

	private void Awake() {
		Instance = this;
	}

	public Vector3 GetDeathPos() {
        return this.transform.position;
    }
}
