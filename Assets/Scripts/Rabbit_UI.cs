/*
Code by Hayri Cakir
www.hayricakir.com
*/

using UnityEngine;
using UnityEditor;

public class Rabbit_UI:MonoBehaviour
{
	private void LateUpdate()
	{
		transform.LookAt(SceneView.lastActiveSceneView.camera.transform);

	}
}
