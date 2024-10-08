using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelCube : MonoBehaviour
{
    // Singleton instance
    public static VoxelCube Instance { get; private set; }

    public Transform targetPosition;
    public Transform earthCore;

    [Header("Voxel Cube Settings")]
    public int diameter = 5; // Diameter of the VoxelCube
    public GameObject voxelPrefab; // Prefab for general voxel
    public GameObject humanPrefab; // Prefab for the human
    public GameObject treePrefab;  // Prefab for the tree
    public GameObject mountainPrefab; // Prefab for the mountain
    public GameObject swampPrefab; // Prefab for the swamp
    public GameObject cityPrefab; // Prefab for the city
    public Transform cubePosition; // Position where the voxel cube is created (center of the planet)

    [Header("Material Settings")]
    public Material outerLayerMaterial;
    public Material middleLayerMaterial;
    public Material waterMaterial;
    public Material treeMaterial;
    public Material humanMaterial;
    public Material mountainMaterial;
    public Material swampMaterial;
    public Material cityMaterial;

    [Header("Spawning Settings")]
    public float humanSpawnInterval = 10f;

    // Lists to store all objects in the scene
    public List<GameObject> createdVoxels = new List<GameObject>();
    public List<GameObject> humans = new List<GameObject>();
    public List<GameObject> trees = new List<GameObject>();
    public List<GameObject> water = new List<GameObject>();
    public List<GameObject> mountains = new List<GameObject>();
    public List<GameObject> swamps = new List<GameObject>();
    public List<GameObject> cities = new List<GameObject>();

    private float nextHumanSpawnTime;

    // Parent transforms to organize objects
    public Transform treeParent;
    public Transform humanParent;
    public Transform waterParent;
    public Transform voxelCubeParent;
    public Transform mountainParent;
    public Transform swampParent;
    public Transform cityParent;

    // Singleton setup
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of VoxelCube detected. Destroying the new one.");
            Destroy(gameObject); // Destroy the new instance if one already exists
        }
        else
        {
            Instance = this; // Assign the instance
            DontDestroyOnLoad(gameObject); // Ensure the singleton persists across scenes
        }
    }

    private void Start()
    {
        // If needed, initialize voxel cube or load previously saved state here
        nextHumanSpawnTime = Time.time + humanSpawnInterval;

        if (createdVoxels.Count == 0 && voxelCubeParent != null)
        {
            PopulateCreatedVoxelsFromParent();
        }
    }

    private void OnApplicationQuit()
    {
        //SaveVoxelCube(); // Save the voxel cube state when quitting the application
    }

    private void PopulateCreatedVoxelsFromParent()
    {
        foreach (Transform child in voxelCubeParent)
        {
            if (child.CompareTag("Voxel"))
            {
                createdVoxels.Add(child.gameObject);
            }
        }
    }

    public void CheckAndMoveToTargetPosition()
    {
        if (targetPosition != null)
        {
            if (transform.position != targetPosition.position)
            {
                Debug.Log("Position mismatch. Moving to target position.");
                transform.position = targetPosition.position;
            }
        }
    }

    private void Update()
    {
        CheckAndMoveToTargetPosition();

        // Handle user input for placing various objects on the voxel cube
        HandleInput();
    }

    private void HandleInput()
    {
        int layerMask = ~LayerMask.GetMask("HackathonIgnore");

        //if (Input.GetMouseButtonDown(0))
        //{
        //    RaycastHit hit;
        //    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask))
        //    {
        //        if (hit.transform.CompareTag("Voxel"))
        //        {
        //            PlaceTree(hit.transform.gameObject);
        //        }
        //    }
        //}

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask))
            {
                if (hit.transform.CompareTag("Voxel"))
                {
                    ReplaceWithWater(hit.transform.gameObject);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask))
            {
                if (hit.transform.CompareTag("Voxel"))
                {
                    PlaceMountain(hit.transform.gameObject);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask))
            {
                if (hit.transform.CompareTag("Voxel"))
                {
                    PlaceSwamp(hit.transform.gameObject);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask))
            {
                if (hit.transform.CompareTag("Voxel"))
                {
                    PlaceCity(hit.transform.gameObject);
                }
            }
        }
    }

    public void GenerateVoxelCube()
    {
        ClearVoxelCube(); // Clear old data before generating new

        if (voxelPrefab == null || cubePosition == null)
        {
            Debug.LogError("Please make sure VoxelPrefab and Position are set.");
            return;
        }

        float radius = diameter / 2f; // Calculate the radius of the cube
        Vector3 center = new Vector3(radius, radius, radius);
        float outerLayerThickness = radius * 0.33f;

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

                        voxel.tag = "Voxel"; // Set tag to voxel
                        createdVoxels.Add(voxel); // Add to voxel list
                    }
                }
            }
        }
    }

    public void ReplaceWithWater(GameObject targetVoxel)
    {
        Renderer renderer = targetVoxel.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = waterMaterial;
            targetVoxel.tag = "Water";
            targetVoxel.transform.parent = waterParent;
            createdVoxels.Remove(targetVoxel);
            water.Add(targetVoxel);
        }
    }

    public void SpawnHuman()
    {
        Vector3 spawnPosition = new Vector3(0, 0, 0); // Adjust spawn position logic as needed

        if (spawnPosition != Vector3.zero)
        {
            GameObject human = Instantiate(humanPrefab, spawnPosition, Quaternion.identity, humanParent);
            human.tag = "Human";
            humans.Add(human);
        }
        else
        {
            Debug.LogWarning("No valid voxel found for human spawn.");
        }
    }

    public void PlaceTree(Vector3 hitPoint, Vector3 surfaceNormal, Vector3 directionFromCore)
    {
        // Use the exact hit point to position the tree
        Vector3 treePosition = hitPoint;

        // Calculate the rotation based on the direction from the core and the surface normal
        Quaternion treeRotation = Quaternion.LookRotation(directionFromCore, surfaceNormal);

        // Instantiate the tree at the hit point with the correct rotation
        GameObject newTree = Instantiate(treePrefab, treePosition, treeRotation, treeParent);
        newTree.tag = "Tree";
        trees.Add(newTree);
    }




    public void PlaceMountain(GameObject targetVoxel)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            Vector3 directionFromCore = (hit.point - earthCore.position).normalized;
            Vector3 mountainPosition = hit.point + directionFromCore * 0.5f; // Offset outward

            mountainPosition = new Vector3(Mathf.Round(mountainPosition.x), Mathf.Round(mountainPosition.y), Mathf.Round(mountainPosition.z));

            Quaternion mountainRotation = GetRotationFromDirection(directionFromCore);

            GameObject newMountain = Instantiate(mountainPrefab, mountainPosition, mountainRotation, mountainParent);
            newMountain.tag = "Mountain";
            mountains.Add(newMountain);
        }
    }

    public void PlaceSwamp(GameObject targetVoxel)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            Vector3 directionFromCore = (hit.point - earthCore.position).normalized;
            Vector3 swampPosition = hit.point + directionFromCore * 0.5f; // Offset outward

            swampPosition = new Vector3(Mathf.Round(swampPosition.x), Mathf.Round(swampPosition.y), Mathf.Round(swampPosition.z));

            Quaternion swampRotation = GetRotationFromDirection(directionFromCore);

            GameObject newSwamp = Instantiate(swampPrefab, swampPosition, swampRotation, swampParent);
            newSwamp.tag = "Swamp";
            swamps.Add(newSwamp);
        }
    }

    public void PlaceCity(GameObject targetVoxel)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            Vector3 directionFromCore = (hit.point - earthCore.position).normalized;
            Vector3 cityPosition = hit.point + directionFromCore * 0.5f; // Offset outward

            cityPosition = new Vector3(Mathf.Round(cityPosition.x), Mathf.Round(cityPosition.y), Mathf.Round(cityPosition.z));

            Quaternion cityRotation = GetRotationFromDirection(directionFromCore);

            GameObject newCity = Instantiate(cityPrefab, cityPosition, cityRotation, cityParent);
            newCity.tag = "City";
            cities.Add(newCity);
        }
    }

    // This function calculates the correct rotation based on the direction from the Earth core
    private Quaternion GetRotationFromDirection(Vector3 outwardDirection)
    {
        // We want the "up" direction of the object to point outward from the Earthcore
        return Quaternion.LookRotation(Vector3.Cross(outwardDirection, Vector3.up), outwardDirection);
    }

    public void SaveVoxelCube()
    {
        PlayerPrefs.DeleteAll(); // Optional: clear old data

        // Save voxels
        if(createdVoxels.Count > 0 )
        {
            for (int i = 0; i < createdVoxels.Count; i++)
            {
                GameObject voxel = createdVoxels[i];
                string voxelKey = "Voxel_" + i;
                Vector3 voxelPos = voxel.transform.position;

                PlayerPrefs.SetFloat(voxelKey + "_x", voxelPos.x);
                PlayerPrefs.SetFloat(voxelKey + "_y", voxelPos.y);
                PlayerPrefs.SetFloat(voxelKey + "_z", voxelPos.z);
                PlayerPrefs.SetString(voxelKey + "_tag", voxel.tag);
            }
        }
      

        // Save humans
        for (int i = 0; i < humans.Count; i++)
        {
            GameObject human = humans[i];
            Vector3 humanPos = human.transform.position;
            string humanKey = "Human_" + i;

            PlayerPrefs.SetFloat(humanKey + "_x", humanPos.x);
            PlayerPrefs.SetFloat(humanKey + "_y", humanPos.y);
            PlayerPrefs.SetFloat(humanKey + "_z", humanPos.z);
        }

        // Save water cubes
        for (int i = 0; i < water.Count; i++)
        {
            GameObject waterVoxel = water[i];
            string waterKey = "Water_" + i;
            Vector3 waterPos = waterVoxel.transform.position;

            PlayerPrefs.SetFloat(waterKey + "_x", waterPos.x);
            PlayerPrefs.SetFloat(waterKey + "_y", waterPos.y);
            PlayerPrefs.SetFloat(waterKey + "_z", waterPos.z);
        }

        // Save mountains
        for (int i = 0; i < mountains.Count; i++)
        {
            GameObject mountain = mountains[i];
            string mountainKey = "Mountain_" + i;
            Vector3 mountainPos = mountain.transform.position;

            PlayerPrefs.SetFloat(mountainKey + "_x", mountainPos.x);
            PlayerPrefs.SetFloat(mountainKey + "_y", mountainPos.y);
            PlayerPrefs.SetFloat(mountainKey + "_z", mountainPos.z);
        }

        // Save swamps
        for (int i = 0; i < swamps.Count; i++)
        {
            GameObject swamp = swamps[i];
            string swampKey = "Swamp_" + i;
            Vector3 swampPos = swamp.transform.position;

            PlayerPrefs.SetFloat(swampKey + "_x", swampPos.x);
            PlayerPrefs.SetFloat(swampKey + "_y", swampPos.y);
            PlayerPrefs.SetFloat(swampKey + "_z", swampPos.z);
        }

        // Save cities
        for (int i = 0; i < cities.Count; i++)
        {
            GameObject city = cities[i];
            string cityKey = "City_" + i;
            Vector3 cityPos = city.transform.position;

            PlayerPrefs.SetFloat(cityKey + "_x", cityPos.x);
            PlayerPrefs.SetFloat(cityKey + "_y", cityPos.y);
            PlayerPrefs.SetFloat(cityKey + "_z", cityPos.z);
        }

        // Save Trees
        for (int i = 0; i < trees.Count; i++)
        {
            GameObject tree = trees[i];
            string treeKey = "Tree_" + i;
            Vector3 treePos = tree.transform.position;

            PlayerPrefs.SetFloat(treeKey + "_x", treePos.x);
            PlayerPrefs.SetFloat(treeKey + "_y", treePos.y);
            PlayerPrefs.SetFloat(treeKey + "_z", treePos.z);
        }

        // Save counts
        PlayerPrefs.SetInt("VoxelCount", createdVoxels.Count);
        PlayerPrefs.SetInt("WaterCount", water.Count);
        PlayerPrefs.SetInt("TreeCount", trees.Count);
        PlayerPrefs.SetInt("HumanCount", humans.Count);
        PlayerPrefs.SetInt("MountainCount", mountains.Count);
        PlayerPrefs.SetInt("SwampCount", swamps.Count);
        PlayerPrefs.SetInt("CityCount", cities.Count);

        PlayerPrefs.Save();
        Debug.Log("Voxel cube saved successfully.");
    }

    public void LoadVoxelCube()
    {
        ClearVoxelCube(); // Clear any existing voxels, humans, or trees

        // Load voxels
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

            GameObject voxel = Instantiate(voxelPrefab, position, Quaternion.identity, voxelCubeParent);
            voxel.tag = tag;

            Renderer renderer = voxel.GetComponent<Renderer>();
            if (tag == "Tree")
            {
                renderer.material = treeMaterial;
                voxel.transform.parent = treeParent;
                trees.Add(voxel);
            }
            else if (tag == "Water")
            {
                renderer.material = waterMaterial;
                voxel.transform.parent = waterParent;
                water.Add(voxel);
            }
            else
            {
                renderer.material = outerLayerMaterial;
            }

            createdVoxels.Add(voxel);
        }

        // Load water cubes
        int waterCount = PlayerPrefs.GetInt("WaterCount", 0);
        for (int i = 0; i < waterCount; i++)
        {
            string waterKey = "Water_" + i;
            Vector3 waterPos = new Vector3(
                PlayerPrefs.GetFloat(waterKey + "_x"),
                PlayerPrefs.GetFloat(waterKey + "_y"),
                PlayerPrefs.GetFloat(waterKey + "_z")
            );

            GameObject waterVoxel = Instantiate(voxelPrefab, waterPos, Quaternion.identity, waterParent);
            waterVoxel.tag = "Water";
            Renderer renderer = waterVoxel.GetComponent<Renderer>();
            renderer.material = waterMaterial;

            water.Add(waterVoxel);
        }

        // Load humans
        int humanCount = PlayerPrefs.GetInt("HumanCount", 0);
        for (int i = 0; i < humanCount; i++)
        {
            string humanKey = "Human_" + i;
            Vector3 humanPos = new Vector3(
                PlayerPrefs.GetFloat(humanKey + "_x"),
                PlayerPrefs.GetFloat(humanKey + "_y"),
                PlayerPrefs.GetFloat(humanKey + "_z")
            );

            GameObject human = Instantiate(humanPrefab, humanPos, Quaternion.identity, humanParent);
            humans.Add(human);
        }

        // Load mountains
        int mountainCount = PlayerPrefs.GetInt("MountainCount", 0);
        for (int i = 0; i < mountainCount; i++)
        {
            string mountainKey = "Mountain_" + i;
            Vector3 mountainPos = new Vector3(
                PlayerPrefs.GetFloat(mountainKey + "_x"),
                PlayerPrefs.GetFloat(mountainKey + "_y"),
                PlayerPrefs.GetFloat(mountainKey + "_z")
            );

            GameObject mountain = Instantiate(mountainPrefab, mountainPos, Quaternion.identity, mountainParent);
            mountains.Add(mountain);
        }

        // Load swamps
        int swampCount = PlayerPrefs.GetInt("SwampCount", 0);
        for (int i = 0; i < swampCount; i++)
        {
            string swampKey = "Swamp_" + i;
            Vector3 swampPos = new Vector3(
                PlayerPrefs.GetFloat(swampKey + "_x"),
                PlayerPrefs.GetFloat(swampKey + "_y"),
                PlayerPrefs.GetFloat(swampKey + "_z")
            );

            GameObject swamp = Instantiate(swampPrefab, swampPos, Quaternion.identity, swampParent);
            swamps.Add(swamp);
        }

        // Load cities
        int cityCount = PlayerPrefs.GetInt("CityCount", 0);
        for (int i = 0; i < cityCount; i++)
        {
            string cityKey = "City_" + i;
            Vector3 cityPos = new Vector3(
                PlayerPrefs.GetFloat(cityKey + "_x"),
                PlayerPrefs.GetFloat(cityKey + "_y"),
                PlayerPrefs.GetFloat(cityKey + "_z")
            );

            GameObject city = Instantiate(cityPrefab, cityPos, Quaternion.identity, cityParent);
            cities.Add(city);
        }

        // Load Trees
        int treeCount = PlayerPrefs.GetInt("TreeCount", 0);
        for (int i = 0; i < treeCount; i++)
        {
            string treeKey = "Tree_" + i;
            Vector3 treePos = new Vector3(
                PlayerPrefs.GetFloat(treeKey + "_x"),
                PlayerPrefs.GetFloat(treeKey + "_y"),
                PlayerPrefs.GetFloat(treeKey + "_z")
            );

            GameObject tree = Instantiate(treePrefab, treePos, Quaternion.identity, treeParent);
            trees.Add(tree);
        }

        Debug.Log("Voxel cube loaded successfully.");
    }

    public void ClearVoxelCube()
    {
        // Clear created voxels
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

        // Clear mountains
        foreach (GameObject mountain in mountains)
        {
            if (mountain != null)
            {
                DestroyImmediate(mountain);
            }
        }
        mountains.Clear();

        // Clear swamps
        foreach (GameObject swamp in swamps)
        {
            if (swamp != null)
            {
                DestroyImmediate(swamp);
            }
        }
        swamps.Clear();

        // Clear cities
        foreach (GameObject city in cities)
        {
            if (city != null)
            {
                DestroyImmediate(city);
            }
        }
        cities.Clear();
    }
}
