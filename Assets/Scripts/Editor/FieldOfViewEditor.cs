using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (Animal))]
public class FieldOfViewEditor : Editor {

	void OnSceneGUI() {
		Animal animal = (Animal)target;

		Vector3 viewAngleFA = animal.DirFromAngle(-animal.viewAngleFront / 2, false);
		Vector3 viewAngleFB = animal.DirFromAngle(animal.viewAngleFront / 2, false);
		float degreesToDrawFront = animal.viewAngleFront < 180 ? (-Vector3.Angle(viewAngleFA, viewAngleFB)) : -360 + Vector3.Angle(viewAngleFA, viewAngleFB);

		Vector3 viewAngleBA = animal.DirFromAngle(-animal.viewAngleBack / 2, false);
		Vector3 viewAngleBB = animal.DirFromAngle(animal.viewAngleBack / 2, false);
		float degreesToDrawBack = animal.viewAngleBack < 180 ? (-Vector3.Angle(viewAngleBA, viewAngleBB)) : -360 + Vector3.Angle(viewAngleBA, viewAngleBB);


		//ön görüş alanını görsel olarak editöre aktarır
		Handles.color = Color.white;
		Handles.DrawLine(animal.transform.position, animal.transform.position + viewAngleFA * animal.viewRadiusFront);
		Handles.DrawLine(animal.transform.position, animal.transform.position + viewAngleFB * animal.viewRadiusFront);
		Handles.DrawWireArc(animal.transform.position, Vector3.up, viewAngleFB, degreesToDrawFront, animal.viewRadiusFront);

		//arka görüş alanını görsel olarak editöre aktarır
		Handles.color = Color.black;
		Handles.DrawLine(animal.transform.position, animal.transform.position - viewAngleBA * animal.viewRadiusBack);
		Handles.DrawLine(animal.transform.position, animal.transform.position - viewAngleBB * animal.viewRadiusBack);
		Handles.DrawWireArc(animal.transform.position, Vector3.up, -viewAngleBB, degreesToDrawBack, animal.viewRadiusBack);


		Handles.color = Color.red;
		foreach (GameObject visibleTarget in animal.objectsDictionary.Values) {
			Handles.DrawLine (animal.transform.position, visibleTarget.transform.position);
		}
	}

}
