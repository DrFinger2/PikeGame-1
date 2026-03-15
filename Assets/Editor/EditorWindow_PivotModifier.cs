using UnityEngine;
using UnityEditor;
using System.IO;

public class PivotModifierWindow : EditorWindow
{
    private enum XAxis { Left, Center, Right }
    private enum YAxis { Top, Center, Bottom }

    private XAxis horizontal = XAxis.Center;
    private YAxis vertical = YAxis.Center;

    [MenuItem("Tools/Modify Pivot")]
    public static void ShowWindow()
    {
        GetWindow<PivotModifierWindow>("Modify Pivot");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select a GameObject with a MeshFilter", EditorStyles.boldLabel);
        
        horizontal = (XAxis)EditorGUILayout.EnumPopup("World Horizontal", horizontal);
        vertical = (YAxis)EditorGUILayout.EnumPopup("World Vertical", vertical);

        GUI.enabled = Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<MeshFilter>() != null;
        if (GUILayout.Button("Apply & Save Mesh Asset", GUILayout.Height(30)))
        {
            ApplyPivotOffsetAndSave();
        }
        GUI.enabled = true;
    }

    private void ApplyPivotOffsetAndSave()
    {
        GameObject go = Selection.activeGameObject;
        MeshFilter mf = go.GetComponent<MeshFilter>();
        
        if (mf.sharedMesh == null) return;

        // Get the path of the original mesh to save the new one in the same folder
        string originalPath = AssetDatabase.GetAssetPath(mf.sharedMesh);
        string directory = string.IsNullOrEmpty(originalPath) ? "Assets" : Path.GetDirectoryName(originalPath);
        string newMeshName = mf.sharedMesh.name + "_PivotFixed.asset";
        string savePath = AssetDatabase.GenerateUniqueAssetPath(directory + "/" + newMeshName);

        Mesh newMesh = Instantiate(mf.sharedMesh);
        newMesh.name = mf.sharedMesh.name + "_PivotFixed";
        
        Vector3[] vertices = newMesh.vertices;
        int[] triangles = newMesh.triangles;

        if (triangles.Length == 0) return;

        // 1. Calculate World Bounds
        Vector3 worldMin = go.transform.TransformPoint(vertices[triangles[0]]);
        Vector3 worldMax = worldMin;

        foreach (int index in triangles)
        {
            Vector3 worldPt = go.transform.TransformPoint(vertices[index]);
            worldMin = Vector3.Min(worldMin, worldPt);
            worldMax = Vector3.Max(worldMax, worldPt);
        }

        Vector3 worldCenter = (worldMin + worldMax) * 0.5f;

        // 2. Determine target World Pivot
        float worldX = horizontal == XAxis.Left ? worldMin.x : (horizontal == XAxis.Right ? worldMax.x : worldCenter.x);
        float worldY = vertical == YAxis.Bottom ? worldMin.y : (vertical == YAxis.Top ? worldMax.y : worldCenter.y);
        Vector3 worldPivot = new Vector3(worldX, worldY, worldCenter.z);

        // 3. Save World Space positions of vertices AND children
        Vector3[] worldVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) worldVertices[i] = go.transform.TransformPoint(vertices[i]);

        Transform[] children = new Transform[go.transform.childCount];
        Vector3[] childWorldPos = new Vector3[children.Length];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = go.transform.GetChild(i);
            childWorldPos[i] = children[i].position;
            Undo.RecordObject(children[i], "Change Child Transform");
        }

        // 4. Move Transform
        Undo.RecordObject(go.transform, "Change Transform Pivot");
        go.transform.position = worldPivot;

        // 5. Convert world vertices back to NEW local space and restore children
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = go.transform.InverseTransformPoint(worldVertices[i]);
        }

        for (int i = 0; i < children.Length; i++)
        {
            children[i].position = childWorldPos[i];
        }
        
        // 6. Apply to mesh and SAVE the asset permanently
        newMesh.vertices = vertices;
        newMesh.RecalculateBounds();

        AssetDatabase.CreateAsset(newMesh, savePath);
        AssetDatabase.SaveAssets();

        // 7. Swap the old mesh for the new permanently saved one
        Undo.RecordObject(mf, "Change Mesh Pivot");
        mf.sharedMesh = newMesh;

        if (go.TryGetComponent(out MeshCollider mc))
        {
            Undo.RecordObject(mc, "Change Collider Pivot");
            mc.sharedMesh = newMesh;
        }

        Debug.Log($"Saved new mesh permanently at: {savePath}");
    }
}