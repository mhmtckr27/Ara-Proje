/*
Code by Hayri Cakir
www.hayricakir.com
*/

using UnityEngine;
using UnityEditor;

public class Rabbit_UI : MonoBehaviour
{
	private void LateUpdate()
	{
		transform.LookAt(SceneView.lastActiveSceneView.camera.transform);

		GetComponentInChildren<Transform>().localScale = new Vector3(GetComponentInParent<Rabbit>().energy, GetComponentInChildren<Transform>().localScale.y, GetComponentInChildren<Transform>().localScale.z);
	}
}
