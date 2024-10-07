using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelCube : MonoBehaviour
{
    [Header("Voxel Cube Settings")]
    public int diameter = 5; // Durchmesser des VoxelCubes
    public GameObject voxelPrefab; // Das Prefab f�r den Voxel
    public Transform cubePosition; // Die Position, an der der Cube erstellt wird

    [Header("Material Settings")]
    public Material outerLayerMaterial;  // Material f�r die �u�erste Schicht (z.B. Erde, braun)
    public Material middleLayerMaterial; // Material f�r die mittlere Schicht (z.B. Stein, grau)
    public Material waterMaterial;       // Material f�r Wasser (ersetzt Bl�cke)
    public Material treeMaterial;        // Material f�r B�ume (gr�n)
    public Material humanMaterial;       // Material f�r den Menschen (schwarz)

    [Header("Spawning Settings")]
    public float humanSpawnInterval = 10f; // Zeit in Sekunden, nach der ein neuer Mensch gespawnt wird
    public float treeGrowthInterval = 5f;  // Zeit in Sekunden, nach der ein Baum um einen Block w�chst

    private List<GameObject> createdVoxels = new List<GameObject>();
    private List<GameObject> humans = new List<GameObject>(); // Liste aller Menschen
    private List<GameObject> trees = new List<GameObject>();  // Liste aller B�ume

    private float nextHumanSpawnTime;

    private void Start()
    {
        // Setze den n�chsten Spawn-Zeitpunkt f�r Menschen
        nextHumanSpawnTime = Time.time + humanSpawnInterval;
    }

    private void Update()
    {
        // �berpr�fen, ob es Zeit ist, einen neuen Menschen zu spawnen
        if (Time.time >= nextHumanSpawnTime)
        {
            SpawnHuman();
            nextHumanSpawnTime = Time.time + humanSpawnInterval;
        }

        // B�ume oder Wasser per Raycast setzen
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

        // Lasse alle B�ume wachsen
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
        float outerLayerThickness = radius * 0.33f; // Dicke der �u�eren Schichten

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

                        voxel.tag = "Voxel"; // Setze den Tag, damit wir ihn per Raycast identifizieren k�nnen

                        createdVoxels.Add(voxel);
                    }
                }
            }
        }
    }

    // B�ume setzen (Gr�ne Cubes)
    public void SetTree(GameObject targetVoxel)
    {
        Renderer renderer = targetVoxel.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = treeMaterial;
            targetVoxel.tag = "Tree"; // �ndere den Tag, damit Menschen nicht dar�ber gehen k�nnen
            trees.Add(targetVoxel); // F�ge den Baum der Liste hinzu
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
            targetVoxel.tag = "Water"; // �ndere den Tag, damit Menschen nicht dar�ber gehen k�nnen
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

    // Finde eine g�ltige Position f�r den Menschen (kein Wasser, keine B�ume)
    private Vector3 FindValidSpawnPosition()
    {
        foreach (GameObject voxel in createdVoxels)
        {
            if (voxel.CompareTag("Voxel")) // Nur normale Bl�cke
            {
                return voxel.transform.position; // Spawnen an der Position dieses Voxels
            }
        }
        return Vector3.zero; // Falls keine g�ltige Position gefunden wird
    }

    // Lasse alle B�ume wachsen
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

        if (currentHeight < 3) // Baum w�chst bis maximal 3 Bl�cke
        {
            Vector3 newTreePosition = treeBase.transform.position + Vector3.up;

            // �berpr�fe, ob Platz f�r Wachstum vorhanden ist
            if (!Physics.CheckSphere(newTreePosition, 0.1f))
            {
                GameObject newTreeBlock = Instantiate(voxelPrefab, newTreePosition, Quaternion.identity, cubePosition);
                Renderer renderer = newTreeBlock.GetComponent<Renderer>();
                renderer.material = treeMaterial;
                newTreeBlock.tag = "Tree"; // Neuer Block auch als "Tree" taggen
                trees.Add(newTreeBlock);   // Neuer Block wird auch als Baum betrachtet

                // Wiederhole das Wachstum f�r den n�chsten Block
                StartCoroutine(GrowTree(newTreeBlock, currentHeight + 1));
            }
        }
    }

    // Funktion, um den VoxelCube zu l�schen
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

        // L�sche auch alle Menschen
        foreach (GameObject human in humans)
        {
            if (human != null)
            {
                DestroyImmediate(human);
            }
        }
        humans.Clear();

        // L�sche alle B�ume
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
