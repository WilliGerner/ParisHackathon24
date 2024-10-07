using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelCube : MonoBehaviour
{
    [Header("Voxel Cube Settings")]
    public int diameter = 5; // Durchmesser des VoxelCubes
    public GameObject voxelPrefab; // Das Prefab für den Voxel
    public GameObject treePrefab;  // Prefab für Baumwürfel
    public Transform cubePosition; // Die Position, an der der Cube erstellt wird (Mittelpunkt des Planeten)

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

    // Remove GrowTrees() from Update, and call it only once when placing the tree
    private void Update()
    {
        // Human spawning logic remains the same
        if (Time.time >= nextHumanSpawnTime)
        {
            SpawnHuman();
            nextHumanSpawnTime = Time.time + humanSpawnInterval;
        }

        // Left-click to place a tree
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (hit.transform.CompareTag("Voxel"))
                {
                    PlaceTree(hit.transform.gameObject);
                }
            }
        }

        // Right-click to place water
        if (Input.GetMouseButtonDown(1))
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
    }

    // Funktion, um den VoxelCube zu generieren (unverändert)
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

    // Menschen spawnen (unverändert)
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

    // Function to place tree and start growth
    // Define a parent for all trees
    public Transform treeParent;

    public void PlaceTree(GameObject targetVoxel)
    {
        // RaycastHit to get the surface normal
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            // Get the tree spawn position from the voxel's position
            Vector3 treePosition = targetVoxel.transform.position;

            // Align the tree's up direction with the surface normal (hit.normal)
            Quaternion treeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // Check if the parent container for trees exists, if not, create it
            if (treeParent == null)
            {
                GameObject parentObject = new GameObject("TreeParent");
                treeParent = parentObject.transform;
            }

            // Instantiate the tree with the correct rotation and parent it to the treeParent
            GameObject newTree = Instantiate(treePrefab, treePosition, treeRotation, treeParent);
            newTree.tag = "Tree";
            trees.Add(newTree); // Add tree to the list

            // Start tree growth coroutine just once for each tree
            StartCoroutine(GrowTree(newTree, 1));
        }
    }


    private IEnumerator GrowTree(GameObject treeBase, int currentHeight)
    {
        // Wait for the specified interval before growing
        yield return new WaitForSeconds(treeGrowthInterval);

        // Continue growing if the height is less than 3
        if (currentHeight < 3)
        {
            // Modify the scale of the tree (growth happens only in height, Y-axis)
            Vector3 newScale = treeBase.transform.localScale;
            newScale.y += 1.0f; // Increase height (y-axis) only

            // Apply the new scale to the tree base
            treeBase.transform.localScale = newScale;

            // Adjust the tree's position so the base stays in place, but grows upward
            treeBase.transform.position += treeBase.transform.up * 0.5f; // Move upward by half of the scale increase

            // Continue growing the tree to the next height
            StartCoroutine(GrowTree(treeBase, currentHeight + 1));
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