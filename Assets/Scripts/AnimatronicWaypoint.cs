using UnityEngine;

public class AnimatronicWaypoint : MonoBehaviour {
	public string waypointName = "Waypoint";
	[SerializeField] private AnimatronicWaypoint[] nextWaypoints;
	[SerializeField] private AnimatronicWaypoint[] previousWaypoints;

	[SerializeField] private Transform headRotateTarget;
	[SerializeField] private Transform eyesRotateTarget;

	public bool isDoor = false;

	public AnimatronicWaypoint GetFollowingWaypoint() {
		if (nextWaypoints.Length == 0 && previousWaypoints.Length == 0) {
			Debug.LogWarning($"No next waypoints set for {gameObject.name}");
			return this;
		}
		int rand = 0;
		if (!isDoor) {
			rand = Random.Range(0, nextWaypoints.Length);
			return nextWaypoints[rand];
		}
		return this;
	}

	public AnimatronicWaypoint GetPreviousWaypoint() {
		if (previousWaypoints.Length == 0) {
			Debug.LogWarning($"No previous waypoints set for {gameObject.name}");
			return this;
		}
		int rand = Random.Range(0, previousWaypoints.Length);
		return previousWaypoints[rand]; 
	}

	public Transform GetHeadRotateTarget() {
		return headRotateTarget;
	}

	public Transform GetEyesRotateTarget() {
		return eyesRotateTarget;
	}

	
}
