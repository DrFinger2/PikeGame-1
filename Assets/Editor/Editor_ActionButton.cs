using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ActionButton), true)]
[CanEditMultipleObjects]
public class ActionButtonEditor : SelectableEditor
{
    private bool showSelectableSettings = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. Draw the standard Script field at the top
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUILayout.Space();

        // 2. Put the built-in Selectable logic into a clean foldout
        showSelectableSettings = EditorGUILayout.Foldout(showSelectableSettings, "Button Settings", true, EditorStyles.foldoutHeader);
        if (showSelectableSettings)
        {
            EditorGUI.indentLevel++;
            // This cleanly draws the native colors/sprites/transitions menu
            base.OnInspectorGUI(); 
            EditorGUI.indentLevel--;
        }

        DrawPropertiesExcluding(serializedObject, 
            "m_Script", 
            "m_Interactable", 
            "m_TargetGraphic", 
            "m_Transition", 
            "m_Colors", 
            "m_SpriteState", 
            "m_AnimationTriggers", 
            "m_Navigation"
        );

        serializedObject.ApplyModifiedProperties();
    }
}