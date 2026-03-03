#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ParallaxGroup))]
public class ParallaxGroupEditor : Editor
{
    private SerializedProperty alphaProp;

    private void OnEnable()
    {
        alphaProp = serializedObject.FindProperty("alpha");
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

        EditorGUILayout.PropertyField(alphaProp);

         if (mouseIsDown && EditorGUIUtility.hotControl != 0)
        {
            ParallaxGroup group = (ParallaxGroup)target;
            group.RecordUndoForChildren();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif