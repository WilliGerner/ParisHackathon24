using System;
using Unity.VisualScripting;
using UnityEngine;

public class EarthManager : MonoBehaviour
{
    // Public variables to control the rotation
    public Transform earthCore;  // The center (Earthcore) around which the world will rotate
    public float rotationSpeed = 10f;  // Rotation speed
    public bool rotate;
    public int rotationCount;

    private float totalRotation;
   // public Action<object> onCompleteRotation;
    
    // Update is called once per frame
    void Update()
    {
        // Rotate the world around the Earthcore
       if(rotate) RotateAroundCore();
    }

    // Function to rotate the object around the Earthcore
    private void RotateAroundCore()
    {
        if (earthCore != null)  // Check if the Earthcore is assigned
        {
            // Rotate the object around the Earthcore at the specified speed and axis
            transform.RotateAround(earthCore.position, Vector3.up, rotationSpeed * Time.deltaTime);
            
            // Update total rotation
            totalRotation += rotationSpeed * Time.deltaTime;
            
            // Check for complete rotations

            if(totalRotation >= 360f)
            {
                totalRotation -= 360f;
                rotationCount++;
                Debug.Log("Total rotations: " + rotationCount);
                Debug.LogError($"Happy new Year. Welcome to {2432 + rotationCount}!!!!111eleven");

                // Invoke the event
             //   onCompleteRotation.Invoke(this);
            }
        }
        else
        {
            Debug.LogWarning("Earthcore is not assigned to EarthManager!");
        }
    }
}
