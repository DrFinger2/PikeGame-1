#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(ParallaxController))]
public class ParallaxControllerEditor : Editor
{
    SerializedProperty trackedCameraProp;
    SerializedProperty parallaxPositionProp;
    SerializedProperty zoomProp;

    // List to store positions of our GUI fields
    private void OnEnable()
    {
        trackedCameraProp = serializedObject.FindProperty("trackedCamera");
        parallaxPositionProp = serializedObject.FindProperty("ParallaxPosition");
        zoomProp = serializedObject.FindProperty("zoom");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        bool mouseIsDown = false;
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0) // 0 = left mouse button
        {
            mouseIsDown = true;
        }

        EditorGUILayout.PropertyField(trackedCameraProp);
  
        EditorGUILayout.PropertyField(parallaxPositionProp);

        EditorGUILayout.PropertyField(zoomProp, new GUIContent(
            "Zoom",
            "Adjust the zoom level of the parallax camera. >1 zooms in, <1 zooms out.")
        );
        
        // Check for left mouse click on any field
        if (mouseIsDown && EditorGUIUtility.hotControl != 0)
        {
            ParallaxController camera = (ParallaxController)target;
            camera.RegisterUndo();
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif