using System.Collections;
using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    VoxelCube voxelCube;
    public GameObject plantPrefab;  // Prefab der Pflanze, die instanziert werden soll
    public float checkInterval = 5f;  // Zeit in Sekunden zwischen den Nachbarschafts�berpr�fungen
    public float checkDistance = 1f;  // Distanz zur �berpr�fung der Nachbarn (eine Einheit = eine Zelle)
    public string[] blockedTags = { "Swamp", "Mountain", "Water", "City", "Tree" };  // Tags, die verhindern, dass Pflanzen wachsen
    private Transform earthCore;  // Zentrum der Erde, f�r die Rotation
    public int attackPower = 5;  // Angriffskraft (optional f�r sp�tere Logik)

    private void Start()
    {
        voxelCube = GameObject.Find("VoxelManager").GetComponent<VoxelCube>();
        earthCore = GameObject.Find("Earthcore").transform;  // Setzt das Earthcore-Objekt
        StartCoroutine(CheckNeighborsRoutine());  // Startet das Flood-Fill-Wachstum
    }

    // Coroutine, die regelm��ig die Nachbarn �berpr�ft
    private IEnumerator CheckNeighborsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            // Starte das flood-fill-artige Wachsen von Pflanzen
            SpreadPlant();
        }
    }

    private void SpreadPlant()
    {
        // Get the scale of the VoxelManager (this is what contains the voxelCube)
        Vector3 scaleFactor = voxelCube.transform.localScale;

        // Calculate the direction from the current plant to the Earthcore (center of the planet)
        Vector3 directionFromCore = (transform.position - earthCore.position).normalized;

        // Calculate the local horizontal directions relative to the position on the surface
        Vector3 right = Vector3.Cross(Vector3.up, directionFromCore).normalized;
        Vector3 forward = Vector3.Cross(directionFromCore, right).normalized;

        // Define the four horizontal directions relative to the Earth's surface
        Vector3[] directions = new Vector3[]
        {
        forward,
        -forward,
        right,
        -right
        };

        // Check each direction
        bool plantGrown = false;
        foreach (Vector3 direction in directions)
        {
            // Reduce the distance to place the blocks closer to the surface
Vector3 checkPosition = transform.position + Vector3.Scale(direction, scaleFactor) * (checkDistance * 0.8f);  // Adjust this factor as needed


            // Check if the position is blocked or already has a plant
            if (!IsBlockedByTag(checkPosition) && !HasPlantAtPosition(checkPosition))
            {
                // Grow a new plant at the correct scaled position
                Vector3 plantPosition = checkPosition;

                // Apply the rotation considering the scale
                GameObject newPlant = Instantiate(plantPrefab, plantPosition, GetRotationFromEarthCore(checkPosition), transform.parent);
                voxelCube.trees.Add(newPlant);

                // Set specific logic for the new plant script here if needed
                PlantGrowth newPlantScript = newPlant.GetComponent<PlantGrowth>();

                // Mark that at least one plant has grown
                plantGrown = true;
            }
        }
    }


    // Function to check if a plant already exists at the position, considering the scale
    private bool HasPlantAtPosition(Vector3 position)
    {
        Vector3 scaleFactor = voxelCube.transform.localScale;
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f * scaleFactor.x); // Adjust radius by the scale factor
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Tree") || hitCollider.CompareTag("Mountain") ||
                hitCollider.CompareTag("City") || hitCollider.CompareTag("Swamp"))
            {
                return true;  // Position is occupied
            }
        }
        return false;  // No plant at the position
    }

    // Function to check if the position is blocked by a tag, considering the scale
    private bool IsBlockedByTag(Vector3 position)
    {
        Vector3 scaleFactor = voxelCube.transform.localScale;
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f * scaleFactor.x); // Adjust radius by the scale factor
        foreach (Collider hitCollider in hitColliders)
        {
            foreach (string tag in blockedTags)
            {
                if (hitCollider.CompareTag(tag))
                {
                    return true;  // Blocked
                }
            }
        }
        return false;  // Not blocked
    }


    // Funktion, um die Rotation basierend auf dem Earthcore zu berechnen
    private Quaternion GetRotationFromEarthCore(Vector3 plantPosition)
    {
        // Berechne die Richtung vom Earthcore zur neuen Pflanze
        Vector3 directionFromCore = (plantPosition - earthCore.position).normalized;

        // Richtige Rotation, sodass "up" weg vom Earthcore zeigt
        return Quaternion.LookRotation(Vector3.Cross(directionFromCore, Vector3.up), directionFromCore);
    }
}
