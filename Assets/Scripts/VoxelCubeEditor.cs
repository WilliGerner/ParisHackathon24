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

        // Add a button in the Inspector to manually save the voxel cube
        if (GUILayout.Button("Save Voxel Cube"))
        {
            voxelCubeScript.SaveVoxelCube();
            Debug.Log("Voxel Cube saved via Editor.");
        }

        // Optionally, you can add a button to load the voxel cube
        if (GUILayout.Button("Load Voxel Cube"))
        {
            voxelCubeScript.LoadVoxelCube();
            Debug.Log("Voxel Cube loaded via Editor.");
        }
    }

}
