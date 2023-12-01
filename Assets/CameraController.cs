using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f;
    public float rotationSpeed = 100f;
    public float zoomSpeed = 10f;
    
    public float minY = 10f;
    public float maxY = 100f;

    public float boundaryRadius = 50f; // The radius of the boundary circle
    public Vector3 sceneCenter = new Vector3(0, 0, 0); // The center of the scene at ground level

    public Camera cam;

    private Vector3 panInput;
    private Vector3 panMovement;
    private float targetZoom;
    private float targetYaw;
    private float targetPitch = 32f; // Consistent downward angle

    // Smoothing times
    public float panSmoothTime = 0.2f;
    public float rotationSmoothTime = 0.2f;
    public float zoomSmoothSpeed = 0.5f;

    private Vector3 smoothPanVelocity;
    private float smoothZoomVelocity;
    private float smoothYawVelocity;

    private void Start()
    {
        targetZoom = cam.fieldOfView;
        targetYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
         if (Application.isFocused)
        {
            // Panning Movement
        panInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        panInput = Quaternion.Euler(0, targetYaw, 0) * panInput; // Make panning relative to camera's Y rotation
        panMovement = Vector3.SmoothDamp(panMovement, panInput, ref smoothPanVelocity, panSmoothTime);
        Vector3 newPosition = transform.position + panMovement * panSpeed * Time.deltaTime;

        // Boundary Checking
        Vector3 groundPosition = new Vector3(newPosition.x, 0, newPosition.z); // Disregard the camera's height when checking the boundary
        if ((groundPosition - sceneCenter).sqrMagnitude > boundaryRadius * boundaryRadius)
        {
            newPosition = sceneCenter + (groundPosition - sceneCenter).normalized * boundaryRadius;
            newPosition.y = transform.position.y; // Keep the original height
        }

        transform.position = newPosition;

        // Zooming
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        targetZoom -= zoomInput * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minY, maxY);
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetZoom, ref smoothZoomVelocity, zoomSmoothSpeed);

        // Rotating
        if (Input.GetKey(KeyCode.Q))
            targetYaw -= rotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E))
            targetYaw += rotationSpeed * Time.deltaTime;

        // Apply smooth rotation
        float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref smoothYawVelocity, rotationSmoothTime);
        transform.eulerAngles = new Vector3(targetPitch, smoothYaw, 0f);
        

        }
        
        // Toggle Isometric View - To be implemented
    }
}
