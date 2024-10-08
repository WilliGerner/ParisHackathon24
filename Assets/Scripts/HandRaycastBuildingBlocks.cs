using Unity.VisualScripting;
using UnityEngine;

public class HandRaycastBuildingBlocks : MonoBehaviour
{
    public Transform leftHand; // The Transform component of the left hand
    public float rayDistance = 10f; // Length of the Raycast
    public LayerMask hitLayers; // Layers to interact with
    public Color rayColor = Color.red; // Color of the ray shown in Scene View


   public bool tree,city, swamp,water, voxel;
    // Reference to the OVRHand component for detecting gestures
    public OVRHand leftOvrHand;

    void Update()
    {
        // Check if left hand is detected and tracked
        if (leftOvrHand != null && leftOvrHand.IsTracked)
        {
            // Check if pinch gesture is detected
            bool isPinching = leftOvrHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

            // Draw Ray from left hand (always shown)
            Ray ray = new Ray(leftHand.position, leftHand.forward);
            Debug.DrawRay(leftHand.position, leftHand.forward * rayDistance, rayColor);

            // If pinching, perform the Raycast
            if (isPinching)
            {
                Debug.Log("Pinch erkannt!");

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, rayDistance, hitLayers))
                {
                    //Debug.Log("Getroffenes Objekt: " + hit.collider.gameObject.name);

                    if (hit.collider.gameObject.tag == "Meteor")
                    {
                        DestroyImmediate(hit.collider.gameObject);
                    }

                        // Check if the hit object is a voxel
                        if (hit.collider.gameObject.tag == "Voxel")
                    {
                        Debug.Log("Hit Voxel");

                        // Calculate the direction from the earth core to the hit point
                        Vector3 directionFromCore = (hit.point - VoxelCube.Instance.earthCore.position).normalized;

                        // Call the PlaceTree method on the VoxelCube Singleton
                      if(tree)  VoxelCube.Instance.PlaceTree(hit.point, hit.normal, directionFromCore);
                      if(city) VoxelCube.Instance.PlaceCity(hit.point, hit.normal, directionFromCore);
                      if(swamp) VoxelCube.Instance.PlaceSwamp(hit.point, hit.normal, directionFromCore);
                      if(water) VoxelCube.Instance.PlaceWater(hit.point, hit.normal, directionFromCore);
                    }
                }
                else
                {
                    Debug.Log("Kein Treffer");
                }
            }
            else
            {
                Debug.Log("Kein Pinch erkannt.");
            }
        }
        else
        {
            Debug.Log("Hand tracking not available or hand not tracked.");
        }
    }

    public void SetMode(string mode)
    {
        // Set all to false initially
        tree = false;
        city = false;
        swamp = false;
        water = false;
        voxel = false;

        // Based on the input, set the specific mode to true
        switch (mode.ToLower())
        {
            case "tree":
                tree = true;
                break;
            case "city":
                city = true;
                break;
            case "swamp":
                swamp = true;
                break;
            case "water":
                water = true;
                break;
            case "voxel":
                voxel = true;
                break;
            default:
                Debug.LogWarning("Invalid mode specified: " + mode);
                break;
        }

        // Optionally log the active mode for debugging
        Debug.Log("Mode set to: " + mode);
    }

}
