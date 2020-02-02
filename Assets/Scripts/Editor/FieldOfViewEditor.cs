using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (FieldOfView))]

public class FieldOfViewEditor : Editor {
	void OnSceneGUI() {
		FieldOfView fieldOfView = (FieldOfView)target;

		Vector3 viewAngleDA = fieldOfView.DirFromAngle(-fieldOfView.viewAngleDanger / 2, false);
		Vector3 viewAngleDB = fieldOfView.DirFromAngle(fieldOfView.viewAngleDanger / 2, false);
		float degreesToDrawDanger = fieldOfView.viewAngleDanger < 180 ? (-Vector3.Angle(viewAngleDA, viewAngleDB)) : -360 + Vector3.Angle(viewAngleDA, viewAngleDB);

		Vector3 viewAngleFA = fieldOfView.DirFromAngle(-fieldOfView.viewAngleFront / 2, false);
		Vector3 viewAngleFB = fieldOfView.DirFromAngle(fieldOfView.viewAngleFront / 2, false);
		float degreesToDrawFront = fieldOfView.viewAngleFront < 180 ? (-Vector3.Angle(viewAngleFA, viewAngleFB)) : -360 + Vector3.Angle(viewAngleFA, viewAngleFB);

		Vector3 viewAngleBA = fieldOfView.DirFromAngle(-fieldOfView.viewAngleBack / 2, false);
		Vector3 viewAngleBB = fieldOfView.DirFromAngle(fieldOfView.viewAngleBack / 2, false);
		float degreesToDrawBack = fieldOfView.viewAngleBack < 180 ? (-Vector3.Angle(viewAngleBA, viewAngleBB)) : -360 + Vector3.Angle(viewAngleBA, viewAngleBB);


		//kaçış görüş alanını görsel olarak editöre aktarır
		Handles.color = Color.blue;
		Handles.DrawLine(fieldOfView.transform.position, fieldOfView.transform.position + viewAngleDA * (fieldOfView.viewRadiusFront - .25f));
		Handles.DrawLine(fieldOfView.transform.position, fieldOfView.transform.position + viewAngleDB * (fieldOfView.viewRadiusFront - .25f));
		Handles.DrawWireArc(fieldOfView.transform.position, Vector3.up, viewAngleDB, degreesToDrawDanger, fieldOfView.viewRadiusFront - .25f);

		//ön görüş alanını görsel olarak editöre aktarır
		Handles.color = Color.white;
		Handles.DrawLine(fieldOfView.transform.position, fieldOfView.transform.position + viewAngleFA * fieldOfView.viewRadiusFront);
		Handles.DrawLine(fieldOfView.transform.position, fieldOfView.transform.position + viewAngleFB * fieldOfView.viewRadiusFront);
		Handles.DrawWireArc(fieldOfView.transform.position, Vector3.up, viewAngleFB, degreesToDrawFront, fieldOfView.viewRadiusFront);

		//arka görüş alanını görsel olarak editöre aktarır
		Handles.color = Color.black;
		Handles.DrawLine(fieldOfView.transform.position, fieldOfView.transform.position - viewAngleBA * fieldOfView.viewRadiusBack);
		Handles.DrawLine(fieldOfView.transform.position, fieldOfView.transform.position - viewAngleBB * fieldOfView.viewRadiusBack);
		Handles.DrawWireArc(fieldOfView.transform.position, Vector3.up, -viewAngleBB, degreesToDrawBack, fieldOfView.viewRadiusBack);

		Handles.color = Color.red;
		foreach (GameObject visibleTarget in fieldOfView.animalScript.objectsDictionary.Values) {
			try
			{
				Handles.DrawLine (fieldOfView.transform.position, visibleTarget.transform.position);

			}
			catch (MissingReferenceException) { }
		}
	}

}
