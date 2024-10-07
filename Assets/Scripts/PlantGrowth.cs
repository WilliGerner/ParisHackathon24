using System.Collections;
using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    public GameObject plantPrefab;  // Das Prefab der Pflanze, das instanziert werden soll
    public float checkInterval = 5f;  // Zeit in Sekunden zwischen den Nachbarschaftspr�fungen
    public LayerMask plantLayer;  // Layer der Pflanzen, damit wir Nachbarn identifizieren k�nnen
    public float checkDistance = 1f;  // Die Distanz, in der nach Nachbarn gesucht wird (1 Einheit = eine Nachbarzelle)

    private void Start()
    {
        // Startet die �berpr�fung der Nachbarn in regelm��igen Abst�nden
        StartCoroutine(CheckNeighborsRoutine());
    }

    // Coroutine, die regelm��ig die Nachbarn �berpr�ft
    private IEnumerator CheckNeighborsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            CheckAndPlacePlant();
        }
    }

    // Funktion, um die Nachbarschaft zu pr�fen und gegebenenfalls eine neue Pflanze zu platzieren
    private void CheckAndPlacePlant()
    {
        // Definiere die vier Richtungen: links, rechts, vorne, hinten
        Vector3[] directions = new Vector3[]
        {
            Vector3.left,  // Nach links
            Vector3.right,  // Nach rechts
            Vector3.forward,  // Nach vorne
            Vector3.back  // Nach hinten
        };

        // �berpr�fe jede Richtung
        foreach (Vector3 direction in directions)
        {
            Vector3 checkPosition = transform.position + direction * checkDistance;

            // Pr�fen, ob an dieser Position bereits eine Pflanze ist (per SphereCast oder BoxCast)
            if (!Physics.CheckSphere(checkPosition, 0.1f, plantLayer))  // Falls keine Pflanze an der Stelle ist
            {
                // Platziere eine neue Pflanze an dieser Position
                Instantiate(plantPrefab, checkPosition, Quaternion.identity);
                break;  // Nur eine Pflanze auf einmal platzieren, dann beenden
            }
        }
    }
}
