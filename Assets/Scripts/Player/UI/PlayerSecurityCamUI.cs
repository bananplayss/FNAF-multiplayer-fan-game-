using Unity.Netcode;
using UnityEngine;

public class PlayerSecurityCamUI : NetworkBehaviour
{
	public static PlayerSecurityCamUI Instance { get; private set; }

	private PlayerSecurityCamManager psc_man;
	[SerializeField] private TabletOverlayUI tabletOverlay;
	private Tablet tablet;
	private bool inCams = false;
	private bool isDead = false;

	private void Awake() {
		Instance = this;
	}

	private void Start() {
		if(PlayerRole.Instance.GetRole() != PlayerRoles.Security) {
			ShowTabletOverlay(false);
		}

		tabletOverlay.OnHoverEnter += TabletOverlay_OnHoverEnter;
		tabletOverlay.OnOpenCamsAfterDeath += TabletOverlay_OnOpenCamsAfterDeath;
		
		GameStateManager.Instance.OnHideUI += Instance_OnHideUI;
	}

	private void TabletOverlay_OnOpenCamsAfterDeath() {
		tabletOverlay.OnHoverEnter -= TabletOverlay_OnHoverEnter;
		ShowTabletOverlay(false);
		tablet.EnterCamera();
		GameManager.Instance.inCams = true;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void Initialize(PlayerSecurityCamManager psc) {
		psc_man = psc;
		psc_man.OnSecurityRoomStatusChanged += Psc_man_OnSecurityRoomStatusChanged;
	}

	private void Update() {
		if (PlayerRole.Instance.GetRole() != PlayerRoles.Security) {
			ShowTabletOverlay(false);
		}
	}

	public void Initialize(Tablet tablet) {
		this.tablet = tablet;
		tablet.OnEnterCamera += Tablet_OnEnterCamera;
		tablet.OnExitCamera += Tablet_OnExitCamera;
		tablet.OnDisableTablet += Tablet_OnDisableTablet;
	}

	private void Tablet_OnDisableTablet() {
		ShowTabletOverlay(false);
		enabled = false;
	}

	private void Instance_OnHideUI() {
		ShowTabletOverlay(false);
	}

	private void Tablet_OnExitCamera() {
		ShowTabletOverlay();
		if (PlayerRole.Instance.GetRole() != PlayerRoles.Security) {
			ShowTabletOverlay(false);
		}
	}

	private void Tablet_OnEnterCamera() {
		ShowTabletOverlay();
		if (PlayerRole.Instance.GetRole() != PlayerRoles.Security) {
			ShowTabletOverlay(false);
		}
	}

	private void TabletOverlay_OnHoverEnter() {
		inCams = !inCams;
		GameManager.Instance.inCams = inCams;
		tablet.ToggleCam(inCams);
		ShowTabletOverlay(false);
	}

	private void Psc_man_OnSecurityRoomStatusChanged(bool inSecRoom) {
		if (PlayerRole.Instance.GetRole() != PlayerRoles.Security) return;
		ShowTabletOverlay(inSecRoom);
	}

	private void ShowTabletOverlay(bool show = true) {
		tabletOverlay.gameObject.SetActive(show);
	}
}
