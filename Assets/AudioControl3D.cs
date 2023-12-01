using UnityEngine;

public class AudioControl3D : MonoBehaviour
{
 public Camera parentCamera; // Reference to the parent camera
    public float minFov = 15.0f; // Minimum field of view value
    public float maxFov = 60.0f; // Maximum field of view value
    public float minDistance = 10.0f; // Minimum distance from the camera
    public float maxDistance = 50.0f; // Maximum distance from the camera
    public float exponent = 2.0f; // Exponent to control the rate of change

    private void Update()
    {
        // Check if the parentCamera is assigned
        if (parentCamera != null)
        {
            // Get the current FOV from the camera
            float currentFov = parentCamera.fieldOfView;

            // Normalize the FOV value between 0 and 1
            float normalizedFov = (currentFov - minFov) / (maxFov - minFov);

            // Apply an exponential function to the normalized FOV
            float adjustedFov = Mathf.Pow(normalizedFov, exponent);

            // Calculate the distance based on the adjusted FOV
            float distance = Mathf.Lerp(maxDistance, minDistance, adjustedFov);

            // Set the position of this object at the calculated distance from the camera
            transform.position = parentCamera.transform.position + parentCamera.transform.forward * distance;
        }
        else
        {
            Debug.LogError("Parent camera is not assigned!");
        }
    }
}