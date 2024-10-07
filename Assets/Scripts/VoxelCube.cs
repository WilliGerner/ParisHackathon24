using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelCube : MonoBehaviour
{
    [Header("Voxel Cube Settings")]
    public int diameter = 5; // Durchmesser des VoxelCubes
    public GameObject voxelPrefab; // Das Prefab für den Voxel
    public GameObject humanPrefab; // Das Prefab für den Voxel
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

    public List<GameObject> createdVoxels = new List<GameObject>();
    public List<GameObject> humans = new List<GameObject>(); // Liste aller Menschen
    public List<GameObject> trees = new List<GameObject>();  // Liste aller Bäume
    public List<GameObject> water = new List<GameObject>();  // Liste aller Bäume

    private float nextHumanSpawnTime;
    // Function to place tree and start growth
    // Define a parent for all trees
    public Transform treeParent;
    public Transform humanParent;
    public Transform waterParent;
    public Transform voxelCubeParent;
    private void Start()
    {
        // Load saved voxel cube if any exists
        if (PlayerPrefs.HasKey("VoxelCount"))
        {
            LoadVoxelCube();
        }
        else
        {
            // Generate the voxel cube for the first time
            GenerateVoxelCube();
        }

        // Set the next human spawn time
        nextHumanSpawnTime = Time.time + humanSpawnInterval;

        if (createdVoxels.Count == 0 && voxelCubeParent != null)
        {
            PopulateCreatedVoxelsFromParent();
        }
    }

    private void OnApplicationQuit()
    {
        SaveVoxelCube();
    }


    private void PopulateCreatedVoxelsFromParent()
    {
        Debug.Log("Populating createdVoxels from voxelCubeParent.");

        foreach (Transform child in voxelCubeParent)
        {
            // Only add the voxel child objects tagged as "Voxel"
            if (child.CompareTag("Voxel"))
            {
                createdVoxels.Add(child.gameObject);
                Debug.Log("Added voxel at position: " + child.position);
            }
        }

        if (createdVoxels.Count == 0)
        {
            Debug.LogWarning("No voxels found in voxelCubeParent.");
        }
        else
        {
            Debug.Log("Successfully populated createdVoxels with " + createdVoxels.Count + " voxels.");
        }
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
                        GameObject voxel = Instantiate(voxelPrefab, position, Quaternion.identity, voxelCubeParent);

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
            targetVoxel.name = "WaterCube";
            targetVoxel.transform.parent = waterParent;
            createdVoxels.Remove(targetVoxel);
            water.Add(targetVoxel);
        }
    }

    // Menschen spawnen (unverändert)
    public void SpawnHuman()
    {
        Debug.Log("Try Spawn Human");
        Vector3 spawnPosition = new Vector3(0, 0, 0);// FindValidOuterVoxel();  // We now search only the outermost voxels

        if (spawnPosition != Vector3.zero)
        {
            GameObject human = Instantiate(humanPrefab, spawnPosition, Quaternion.identity, humanParent);

            // Calculate the normal direction from the center of the planet to this position
            Vector3 normalDirection = (spawnPosition - cubePosition.position).normalized;

            // Align the human so that "up" is away from the planet's center
            human.transform.rotation = Quaternion.FromToRotation(Vector3.up, normalDirection);

            human.tag = "Human";
            humans.Add(human);
            Debug.Log("Spawned new Human at position: " + spawnPosition);
        }
        else
        {
            Debug.LogWarning("No valid outer voxel found for Human.");
        }
    }


    // Function to check if a voxel is an outer voxel by checking if it has an empty adjacent space
   

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

    public void SaveVoxelCube()
    {
        PlayerPrefs.DeleteAll(); // Optional: Clear previous save data

        // Save voxel data
        for (int i = 0; i < createdVoxels.Count; i++)
        {
            GameObject voxel = createdVoxels[i];
            string voxelKey = "Voxel_" + i;
            Vector3 voxelPos = voxel.transform.position;

            // Save position
            PlayerPrefs.SetFloat(voxelKey + "_x", voxelPos.x);
            PlayerPrefs.SetFloat(voxelKey + "_y", voxelPos.y);
            PlayerPrefs.SetFloat(voxelKey + "_z", voxelPos.z);

            // Save tag (Tree, Water, or normal Voxel)
            PlayerPrefs.SetString(voxelKey + "_tag", voxel.tag);

            // If the voxel is a tree, save the tree's height
            if (voxel.tag == "Tree")
            {
                float treeHeight = voxel.transform.localScale.y; // Assuming scale.y represents the height
                PlayerPrefs.SetFloat(voxelKey + "_height", treeHeight);
            }
        }

        // Save humans (if required)
        for (int i = 0; i < humans.Count; i++)
        {
            GameObject human = humans[i];
            Vector3 humanPos = human.transform.position;
            string humanKey = "Human_" + i;
            PlayerPrefs.SetFloat(humanKey + "_x", humanPos.x);
            PlayerPrefs.SetFloat(humanKey + "_y", humanPos.y);
            PlayerPrefs.SetFloat(humanKey + "_z", humanPos.z);
        }

        // Save water cubes under WaterParent
        for (int i = 0; i < water.Count; i++)
        {
            GameObject waterVoxel = water[i];
            string waterKey = "Water_" + i;
            Vector3 waterPos = waterVoxel.transform.position;

            PlayerPrefs.SetFloat(waterKey + "_x", waterPos.x);
            PlayerPrefs.SetFloat(waterKey + "_y", waterPos.y);
            PlayerPrefs.SetFloat(waterKey + "_z", waterPos.z);

            PlayerPrefs.SetString(waterKey + "_tag", "Water");
        }

        // Save trees under Trees parent
        for (int i = 0; i < trees.Count; i++)
        {
            GameObject tree = trees[i];
            string treeKey = "Tree_" + i;
            Vector3 treePos = tree.transform.position;

            PlayerPrefs.SetFloat(treeKey + "_x", treePos.x);
            PlayerPrefs.SetFloat(treeKey + "_y", treePos.y);
            PlayerPrefs.SetFloat(treeKey + "_z", treePos.z);

            float treeHeight = tree.transform.localScale.y; // Save tree height
            PlayerPrefs.SetFloat(treeKey + "_height", treeHeight);

            PlayerPrefs.SetString(treeKey + "_tag", "Tree");
        }

        // Save counts
        PlayerPrefs.SetInt("VoxelCount", createdVoxels.Count);
        PlayerPrefs.SetInt("WaterCount", water.Count);
        PlayerPrefs.SetInt("TreeCount", trees.Count);
        PlayerPrefs.SetInt("HumanCount", humans.Count);

        PlayerPrefs.Save();
        Debug.Log("Voxel cube saved successfully.");
    }



    public void LoadVoxelCube()
    {
        ClearVoxelCube(); // Clear any existing voxels, humans, or trees

        // Load voxel data
        int voxelCount = PlayerPrefs.GetInt("VoxelCount", 0);
        for (int i = 0; i < voxelCount; i++)
        {
            string voxelKey = "Voxel_" + i;
            Vector3 position = new Vector3(
                PlayerPrefs.GetFloat(voxelKey + "_x"),
                PlayerPrefs.GetFloat(voxelKey + "_y"),
                PlayerPrefs.GetFloat(voxelKey + "_z")
            );
            string tag = PlayerPrefs.GetString(voxelKey + "_tag");

            // Instantiate voxel at saved position
            GameObject voxel = Instantiate(voxelPrefab, position, Quaternion.identity, voxelCubeParent);
            voxel.tag = tag;

            // Set the correct material based on the tag
            Renderer renderer = voxel.GetComponent<Renderer>();
            if (tag == "Tree")
            {
                // Assign tree material and load its height
                renderer.material = treeMaterial;

                // Restore the tree's height
                float treeHeight = PlayerPrefs.GetFloat(voxelKey + "_height", 1);
                voxel.transform.localScale = new Vector3(1, treeHeight, 1);
                voxel.transform.parent = treeParent;  // Set parent to Trees
                trees.Add(voxel);  // Add to trees list
            }
            else
            {
                // Default voxel material for non-tree voxels
                renderer.material = outerLayerMaterial;
            }

            createdVoxels.Add(voxel); // Add to voxel list
        }

        // Load water cubes under WaterParent
        int waterCount = PlayerPrefs.GetInt("WaterCount", 0);
        for (int i = 0; i < waterCount; i++)
        {
            string waterKey = "Water_" + i;
            Vector3 waterPos = new Vector3(
                PlayerPrefs.GetFloat(waterKey + "_x"),
                PlayerPrefs.GetFloat(waterKey + "_y"),
                PlayerPrefs.GetFloat(waterKey + "_z")
            );

            // Instantiate water voxel
            GameObject waterVoxel = Instantiate(voxelPrefab, waterPos, Quaternion.identity, waterParent);
            waterVoxel.tag = "Water";

            // Assign the water material
            Renderer renderer = waterVoxel.GetComponent<Renderer>();
            renderer.material = waterMaterial;

            water.Add(waterVoxel);  // Add to water list
        }

        // Load trees under Trees parent
        int treeCount = PlayerPrefs.GetInt("TreeCount", 0);
        for (int i = 0; i < treeCount; i++)
        {
            string treeKey = "Tree_" + i;
            Vector3 treePos = new Vector3(
                PlayerPrefs.GetFloat(treeKey + "_x"),
                PlayerPrefs.GetFloat(treeKey + "_y"),
                PlayerPrefs.GetFloat(treeKey + "_z")
            );

            float treeHeight = PlayerPrefs.GetFloat(treeKey + "_height", 1);

            // Instantiate tree at the saved position with correct height
            GameObject tree = Instantiate(treePrefab, treePos, Quaternion.identity, treeParent);
            tree.transform.localScale = new Vector3(1, treeHeight, 1);  // Restore tree height
            tree.tag = "Tree";

            trees.Add(tree);  // Add to trees list
        }

        // Load human data (if needed)
        int humanCount = PlayerPrefs.GetInt("HumanCount", 0);
        for (int i = 0; i < humanCount; i++)
        {
            string humanKey = "Human_" + i;
            Vector3 position = new Vector3(
                PlayerPrefs.GetFloat(humanKey + "_x"),
                PlayerPrefs.GetFloat(humanKey + "_y"),
                PlayerPrefs.GetFloat(humanKey + "_z")
            );
            GameObject human = Instantiate(humanPrefab, position, Quaternion.identity, humanParent);
            humans.Add(human);
        }

        Debug.Log("Voxel cube loaded successfully.");
    }


    public void ClearVoxelCube()
    {
        // Clear createdVoxels
        foreach (GameObject voxel in createdVoxels)
        {
            if (voxel != null)
            {
                DestroyImmediate(voxel);
            }
        }
        createdVoxels.Clear();

        // Clear humans
        foreach (GameObject human in humans)
        {
            if (human != null)
            {
                DestroyImmediate(human);
            }
        }
        humans.Clear();

        // Clear trees
        foreach (GameObject tree in trees)
        {
            if (tree != null)
            {
                DestroyImmediate(tree);
            }
        }
        trees.Clear();

        // Clear water cubes
        foreach (GameObject waterVoxel in water)
        {
            if (waterVoxel != null)
            {
                DestroyImmediate(waterVoxel);
            }
        }
        water.Clear();
    }


}



