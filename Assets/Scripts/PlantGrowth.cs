using System.Collections;
using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
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

    // Funktion, die überprüft, ob Pflanzen um den aktuellen Punkt wachsen können
    private void SpreadPlant()
    {
        // Berechne die Richtung von der aktuellen Pflanze zum Earthcore (Zentrum des Planeten)
        Vector3 directionFromCore = (transform.position - earthCore.position).normalized;

        // Berechne die lokalen horizontalen Richtungen relativ zur Position auf der Oberfläche
        Vector3 right = Vector3.Cross(Vector3.up, directionFromCore).normalized;  // Rechtwinklig zur Oberfläche
        Vector3 forward = Vector3.Cross(directionFromCore, right).normalized;  // Nach "vorne" relativ zur Oberfläche

        // Definiere die vier horizontalen Richtungen relativ zur Erdoberfläche
        Vector3[] directions = new Vector3[]
        {
            forward,        // Nach "vorne"
            -forward,       // Nach "hinten"
            right,          // Nach "rechts"
            -right          // Nach "links"
        };

        // Überprüfe jede Richtung
        bool plantGrown = false;
        foreach (Vector3 direction in directions)
        {
            Vector3 checkPosition = transform.position + direction * checkDistance;  // Neue Position in der Richtung

            // Prüfe, ob die Position blockiert ist oder ob sich dort bereits eine Pflanze befindet
            if (!IsBlockedByTag(checkPosition) && !HasPlantAtPosition(checkPosition))
            {
                // Wachse eine neue Pflanze auf derselben Höhe
                GameObject newPlant = Instantiate(plantPrefab, checkPosition, GetRotationFromEarthCore(checkPosition), transform.parent);
                voxelCube.trees.Add(newPlant);

                // Setze spezifische Logik für das neue Pflanzenskript hier ein (falls benötigt)
                PlantGrowth newPlantScript = newPlant.GetComponent<PlantGrowth>();

                // Markiere, dass in mindestens einer Richtung eine Pflanze gewachsen ist
                plantGrown = true;
            }
        }
    }

    // Funktion, um zu überprüfen, ob an der Position bereits eine Pflanze existiert
    private bool HasPlantAtPosition(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f);  // Überprüft einen kleinen Radius um die Position
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Tree") || hitCollider.CompareTag("Mountain") ||
                hitCollider.CompareTag("City") || hitCollider.CompareTag("Swamp"))
            {
                Debug.Log("Hit Tag");
                return true;  // Position ist belegt
            }

            if (hitCollider.CompareTag("Voxel"))
            {
                Debug.Log("Hit Voxel");
            }
        }
        return false;  // Keine Pflanze an der Position
    }

    // Funktion, um zu überprüfen, ob ein bestimmtes Tag die Position blockiert
    private bool IsBlockedByTag(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f);  // Überprüfe Objekte in einem kleinen Radius
        foreach (Collider hitCollider in hitColliders)
        {
            // Wenn eines der Objekte einen blockierenden Tag hat
            foreach (string tag in blockedTags)
            {
                if (hitCollider.CompareTag(tag))
                {
                    return true;  // Blockiert
                }
            }
        }
        return false;  // Nicht blockiert
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
