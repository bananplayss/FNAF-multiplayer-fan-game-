using Unity.Netcode;

public class JSObject : NetworkBehaviour
{
	public void ShowPostJumpscareScreen() {
		if(!IsOwner) return;
		CameraViewManagerUI.Instance.ShowNoise(10);
	}
}
