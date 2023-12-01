using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera mainCamera;

    void Update()
    {
        if (mainCamera == null)
            mainCamera = Camera.main; // Find the main camera if not set

        // Makes the text face the camera
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);
    }
}
