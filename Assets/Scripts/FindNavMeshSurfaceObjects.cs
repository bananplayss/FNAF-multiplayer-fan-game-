using UnityEngine;
using UnityEditor;
using Unity.AI.Navigation;

public class FindNavMeshSurfaceObjects {
	
	static void FindSurfaces() {
		var surfaces = Object.FindObjectsOfType<NavMeshSurface>(true);
		foreach (var surface in surfaces) {
			Debug.Log($"Found: {surface.gameObject.name}", surface.gameObject);
		}
	}
}