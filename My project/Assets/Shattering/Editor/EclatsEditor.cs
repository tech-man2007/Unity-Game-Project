using UnityEngine;
using System.Collections;
using UnityEditor;

//*
[CustomEditor(typeof(Shattering))]
public class EclatsEditor : Editor {

//	SerializedProperty tModeles;
//	SerializedProperty DoubFace;

	void OnEnable () {
		// Setup the SerializedProperties.
//		tModeles = serializedObject.FindProperty ("tModels");
//		DoubFace = serializedObject.FindProperty ("DoubleFace");
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		Shattering script = (Shattering) target;
		//EditorGUILayout.HelpBox("This is a help box", MessageType.Info);

//		EditorGUILayout.PropertyField (tModeles, GUILayout.ExpandHeight(true));
//		EditorGUILayout.ObjectField( script.tModels, typeof(GameObject[]), true);
//		EditorGUILayout.PropertyField (DoubFace);
//		script.scaleVariation = EditorGUILayout.CurveField ("Scale Variation", script.scaleVariation, GUILayout.MinHeight (80) );

		// si utilisation pour création en plus de destruction alors on change légerement l'apparence de l'éditeur.

	}
}

//*/
