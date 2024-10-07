using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelCube : MonoBehaviour
{
    [Header("Voxel Cube Settings")]
    public int diameter = 5; // Durchmesser des VoxelCubes
    public GameObject voxelPrefab; // Das Prefab für den Voxel
    public Transform cubePosition; // Die Position, an der der Cube erstellt wird

    [Header("Material Settings")]
    public Material outerLayerMaterial;  // Material für die äußerste Schicht (z.B. Erde, braun)
    public Material middleLayerMaterial; // Material für die mittlere Schicht (z.B. Stein, grau)
    public Material waterMaterial;       // Material für Wasser (ersetzt Blöcke)
    public Material treeMaterial;        // Material für Bäume (grün)
    public Material humanMaterial;       // Material für den Menschen (schwarz)

    [Header("Spawning Settings")]
    public float humanSpawnInterval = 10f; // Zeit in Sekunden, nach der ein neuer Mensch gespawnt wird
    public float treeGrowthInterval = 5f;  // Zeit in Sekunden, nach der ein Baum um einen Block wächst

    private List<GameObject> createdVoxels = new List<GameObject>();
    private List<GameObject> humans = new List<GameObject>(); // Liste aller Menschen
    private List<GameObject> trees = new List<GameObject>();  // Liste aller Bäume

    private float nextHumanSpawnTime;

    private void Start()
    {
        // Setze den nächsten Spawn-Zeitpunkt für Menschen
        nextHumanSpawnTime = Time.time + humanSpawnInterval;
    }

    private void Update()
    {
        // Überprüfen, ob es Zeit ist, einen neuen Menschen zu spawnen
        if (Time.time >= nextHumanSpawnTime)
        {
            SpawnHuman();
            nextHumanSpawnTime = Time.time + humanSpawnInterval;
        }

        // Bäume oder Wasser per Raycast setzen
        if (Input.GetMouseButtonDown(0)) // Linksklick
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (hit.transform.CompareTag("Voxel"))
                {
                    SetTree(hit.transform.gameObject);
                }
            }
        }
        if (Input.GetMouseButtonDown(1)) // Rechtsklick
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (hit.transform.CompareTag("Voxel"))
                {
                    ReplaceWithWater(hit.transform.gameObject);
                }
            }
        }

        // Lasse alle Bäume wachsen
        GrowTrees();
    }

    // Funktion, um den VoxelCube zu generieren
    public void GenerateVoxelCube()
    {
        ClearVoxelCube();

        if (voxelPrefab == null || cubePosition == null)
        {
            Debug.LogError("Bitte stelle sicher, dass das VoxelPrefab und die Position gesetzt sind.");
            return;
        }

        float radius = diameter / 2f; // Berechne den Radius der Kugel
        Vector3 center = new Vector3(radius, radius, radius); // Der Mittelpunkt der Kugel
        float outerLayerThickness = radius * 0.33f; // Dicke der äußeren Schichten

        for (int x = 0; x < diameter; x++)
        {
            for (int y = 0; y < diameter; y++)
            {
                for (int z = 0; z < diameter; z++)
                {
                    Vector3 voxelPosition = new Vector3(x, y, z);
                    float distanceFromCenter = Vector3.Distance(voxelPosition, center);

                    if (distanceFromCenter >= radius - outerLayerThickness && distanceFromCenter <= radius)
                    {
                        Vector3 position = cubePosition.position + new Vector3(x, y, z);
                        GameObject voxel = Instantiate(voxelPrefab, position, Quaternion.identity, cubePosition);

                        Renderer voxelRenderer = voxel.GetComponent<Renderer>();

                        if (distanceFromCenter > radius * 0.66f)
                        {
                            voxelRenderer.material = outerLayerMaterial;
                        }
                        else
                        {
                            voxelRenderer.material = middleLayerMaterial;
                        }

                        voxel.tag = "Voxel"; // Setze den Tag, damit wir ihn per Raycast identifizieren können

                        createdVoxels.Add(voxel);
                    }
                }
            }
        }
    }

    // Bäume setzen (Grüne Cubes)
    public void SetTree(GameObject targetVoxel)
    {
        Renderer renderer = targetVoxel.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = treeMaterial;
            targetVoxel.tag = "Tree"; // Ändere den Tag, damit Menschen nicht darüber gehen können
            trees.Add(targetVoxel); // Füge den Baum der Liste hinzu
            StartCoroutine(GrowTree(targetVoxel, 1)); // Beginne das Wachstum des Baumes
        }
    }

    // Wasser setzen (Voxel ersetzen)
    public void ReplaceWithWater(GameObject targetVoxel)
    {
        Renderer renderer = targetVoxel.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = waterMaterial;
            targetVoxel.tag = "Water"; // Ändere den Tag, damit Menschen nicht darüber gehen können
        }
    }

    // Menschen spawnen
    public void SpawnHuman()
    {
        Vector3 spawnPosition = FindValidSpawnPosition();

        if (spawnPosition != Vector3.zero)
        {
            GameObject human = Instantiate(voxelPrefab, spawnPosition, Quaternion.identity);
            Renderer humanRenderer = human.GetComponent<Renderer>();
            humanRenderer.material = humanMaterial;
            human.tag = "Human";
            humans.Add(human);
        }
    }

    // Finde eine gültige Position für den Menschen (kein Wasser, keine Bäume)
    private Vector3 FindValidSpawnPosition()
    {
        foreach (GameObject voxel in createdVoxels)
        {
            if (voxel.CompareTag("Voxel")) // Nur normale Blöcke
            {
                return voxel.transform.position; // Spawnen an der Position dieses Voxels
            }
        }
        return Vector3.zero; // Falls keine gültige Position gefunden wird
    }

    // Lasse alle Bäume wachsen
    private void GrowTrees()
    {
        foreach (GameObject tree in trees)
        {
            StartCoroutine(GrowTree(tree, 1));
        }
    }

    // Coroutine, um einen Baum wachsen zu lassen
    private IEnumerator GrowTree(GameObject treeBase, int currentHeight)
    {
        yield return new WaitForSeconds(treeGrowthInterval);

        if (currentHeight < 3) // Baum wächst bis maximal 3 Blöcke
        {
            Vector3 newTreePosition = treeBase.transform.position + Vector3.up;

            // Überprüfe, ob Platz für Wachstum vorhanden ist
            if (!Physics.CheckSphere(newTreePosition, 0.1f))
            {
                GameObject newTreeBlock = Instantiate(voxelPrefab, newTreePosition, Quaternion.identity, cubePosition);
                Renderer renderer = newTreeBlock.GetComponent<Renderer>();
                renderer.material = treeMaterial;
                newTreeBlock.tag = "Tree"; // Neuer Block auch als "Tree" taggen
                trees.Add(newTreeBlock);   // Neuer Block wird auch als Baum betrachtet

                // Wiederhole das Wachstum für den nächsten Block
                StartCoroutine(GrowTree(newTreeBlock, currentHeight + 1));
            }
        }
    }

    // Funktion, um den VoxelCube zu löschen
    public void ClearVoxelCube()
    {
        foreach (GameObject voxel in createdVoxels)
        {
            if (voxel != null)
            {
                DestroyImmediate(voxel);
            }
        }
        createdVoxels.Clear();

        // Lösche auch alle Menschen
        foreach (GameObject human in humans)
        {
            if (human != null)
            {
                DestroyImmediate(human);
            }
        }
        humans.Clear();

        // Lösche alle Bäume
        foreach (GameObject tree in trees)
        {
            if (tree != null)
            {
                DestroyImmediate(tree);
            }
        }
        trees.Clear();
    }
}
