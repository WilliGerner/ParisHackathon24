using System.Collections;
using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    public float distanceMultiplier;
    VoxelCube voxelCube;
    public GameObject plantPrefab;  // Prefab der Pflanze, die instanziert werden soll
    public float checkInterval = 5f;  // Zeit in Sekunden zwischen den Nachbarschaftsüberprüfungen
    public float checkDistance = 1f;  // Distanz zur Überprüfung der Nachbarn (eine Einheit = eine Zelle)
    public string[] blockedTags = { "Swamp", "Mountain", "Water", "City", "Tree" };  // Tags, die verhindern, dass Pflanzen wachsen
    private Transform earthCore;  // Zentrum der Erde, für die Rotation
    public int attackPower = 5;  // Angriffskraft (optional für spätere Logik)

    private void Start()
    {
        voxelCube = GameObject.Find("VoxelManager").GetComponent<VoxelCube>();
        earthCore = GameObject.Find("Earthcore").transform;  // Setzt das Earthcore-Objekt
        StartCoroutine(CheckNeighborsRoutine());  // Startet das Flood-Fill-Wachstum
    }

    // Coroutine, die regelmäßig die Nachbarn überprüft
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
        // Hol dir die Skalierung des VoxelManagers (dieser enthält das VoxelCube)
        Vector3 scaleFactor = voxelCube.transform.localScale;

        // Berechne die Richtung von der aktuellen Pflanze zum Earthcore (Zentrum des Planeten)
        Vector3 directionFromCore = (transform.position - earthCore.position).normalized;

        // Berechne die lokalen horizontalen Richtungen relativ zur Position auf der Oberfläche
        Vector3 right = Vector3.Cross(Vector3.up, directionFromCore).normalized;
        Vector3 forward = Vector3.Cross(directionFromCore, right).normalized;

        // Definiere die vier horizontalen Richtungen relativ zur Erdoberfläche
        Vector3[] directions = new Vector3[]
        {
        forward,        // Nach "vorne"
        -forward,       // Nach "hinten"
        right,          // Nach "rechts"
        -right          // Nach "links"
        };

        bool plantGrown = false;
        foreach (Vector3 direction in directions)
        {
            // Skaliere die Wachstumsposition entsprechend der Skalierung des VoxelManagers
            Vector3 checkPosition = transform.position + direction * (checkDistance * scaleFactor.x);

            // Prüfe, ob die Position blockiert ist oder ob sich dort bereits eine Pflanze befindet
            if (!IsBlockedByTag(checkPosition) && !HasPlantAtPosition(checkPosition))
            {
                // Wachse eine neue Pflanze auf der skalierten Position
                GameObject newPlant = Instantiate(plantPrefab, checkPosition, GetRotationFromEarthCore(checkPosition), transform.parent);
                voxelCube.trees.Add(newPlant);

                // Setze spezifische Logik für das neue Pflanzenskript hier ein (falls benötigt)
                PlantGrowth newPlantScript = newPlant.GetComponent<PlantGrowth>();

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