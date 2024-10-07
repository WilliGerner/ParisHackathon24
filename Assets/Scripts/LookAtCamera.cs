using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    // Das Objekt, das zur Kamera schauen soll
    public GameObject targetObject;

    // Die Kamera, zu der das Objekt schauen soll
    public Camera targetCamera;

    // Booleans zum Sperren der Rotation auf bestimmten Achsen
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;

    // Aktivierung und Deaktivierung des LookAt-Verhaltens
    public bool isLookAtActive = true;

    // Zusätzliche Rotationswerte (in Grad) für jede Achse
    public Vector3 additionalRotation;

    private void LateUpdate()
    {
        if (!isLookAtActive || targetObject == null || targetCamera == null)
            return;

        // Zielrichtung zur Kamera berechnen
        Vector3 direction = targetCamera.transform.position - targetObject.transform.position;
        Vector3 lookAtDirection = direction;

        // Sperren der Achsen, wenn ausgewählt
        if (lockX) lookAtDirection.x = 0;
        if (lockY) lookAtDirection.y = 0;
        if (lockZ) lookAtDirection.z = 0;

        // Falls alle Achsen gesperrt sind, LookAt deaktivieren
        if (lookAtDirection == Vector3.zero)
            return;

        // Rotation in Richtung der Kamera
        Quaternion lookRotation = Quaternion.LookRotation(lookAtDirection);

        // Zusätzliche Rotation hinzufügen
        Quaternion additionalRotationQuaternion = Quaternion.Euler(additionalRotation);
        targetObject.transform.rotation = lookRotation * additionalRotationQuaternion;
    }
}
