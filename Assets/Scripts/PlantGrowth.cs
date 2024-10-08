using System;
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
    
    [SerializeField]
    private EarthManager earthManager;

    private void OnEnable()
    {
        earthManager.onCompleteRotation += OnRotationCompleted;
    }
    private void OnDisable()
    {
        earthManager.onCompleteRotation += OnRotationCompleted;
    }

    private void Start()
    {
        voxelCube = GameObject.Find("VoxelManager").GetComponent<VoxelCube>();
        earthCore = GameObject.Find("Earthcore").transform;  // Setzt das Earthcore-Objekt
        StartCoroutine(CheckNeighborsRoutine());  // Startet das Flood-Fill-Wachstum
    }

    private void OnRotationCompleted(object sender)
    {
        SpreadPlant();
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

    // Funktion, die �berpr�ft, ob Pflanzen um den aktuellen Punkt wachsen k�nnen
    private void SpreadPlant()
    {
        // Berechne die Richtung von der aktuellen Pflanze zum Earthcore (Zentrum des Planeten)
        Vector3 directionFromCore = (transform.position - earthCore.position).normalized;

        // Berechne die lokalen horizontalen Richtungen relativ zur Position auf der Oberfl�che
        Vector3 right = Vector3.Cross(Vector3.up, directionFromCore).normalized;  // Rechtwinklig zur Oberfl�che
        Vector3 forward = Vector3.Cross(directionFromCore, right).normalized;  // Nach "vorne" relativ zur Oberfl�che

        // Definiere die vier horizontalen Richtungen relativ zur Erdoberfl�che
        Vector3[] directions = new Vector3[]
        {
            forward,        // Nach "vorne"
            -forward,       // Nach "hinten"
            right,          // Nach "rechts"
            -right          // Nach "links"
        };

        // �berpr�fe jede Richtung
        bool plantGrown = false;
        foreach (Vector3 direction in directions)
        {
            Vector3 checkPosition = transform.position + direction * checkDistance;  // Neue Position in der Richtung

            // Pr�fe, ob die Position blockiert ist oder ob sich dort bereits eine Pflanze befindet
            if (!IsBlockedByTag(checkPosition) && !HasPlantAtPosition(checkPosition))
            {
                // Wachse eine neue Pflanze auf derselben H�he
                GameObject newPlant = Instantiate(plantPrefab, checkPosition, GetRotationFromEarthCore(checkPosition), transform.parent);
                voxelCube.trees.Add(newPlant);

                // Setze spezifische Logik f�r das neue Pflanzenskript hier ein (falls ben�tigt)
                PlantGrowth newPlantScript = newPlant.GetComponent<PlantGrowth>();

                // Markiere, dass in mindestens einer Richtung eine Pflanze gewachsen ist
                plantGrown = true;
            }
        }
    }

    // Funktion, um zu �berpr�fen, ob an der Position bereits eine Pflanze existiert
    private bool HasPlantAtPosition(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f);  // �berpr�ft einen kleinen Radius um die Position
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

    // Funktion, um zu �berpr�fen, ob ein bestimmtes Tag die Position blockiert
    private bool IsBlockedByTag(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f);  // �berpr�fe Objekte in einem kleinen Radius
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
