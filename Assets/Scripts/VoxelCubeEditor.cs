using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelCube))]
public class VoxelCubeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Standard-Inspector für die Variablen
        DrawDefaultInspector();

        VoxelCube voxelCubeScript = (VoxelCube)target;

        // Button, um den VoxelCube zu generieren
        if (GUILayout.Button("Generate VoxelCube"))
        {
            voxelCubeScript.GenerateVoxelCube();
        }

        // Button, um den VoxelCube zu löschen
        if (GUILayout.Button("Clear VoxelCube"))
        {
            voxelCubeScript.ClearVoxelCube();
        }
    }
}
