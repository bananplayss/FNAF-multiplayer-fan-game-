using UnityEngine;

public enum PlayerRoles {
	None,
	Security,
	Electrician,
	Technician,
	Janitor
}

public class PlayerRole : MonoBehaviour{
	public static PlayerRole Instance { get; private set; }

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		}

		DontDestroyOnLoad(Instance);
	}

	public PlayerRoles role;

	public void SetRole(PlayerRoles role) {
		this.role = role;
	}

	public PlayerRoles GetRole() {
		return this.role;
	}
}