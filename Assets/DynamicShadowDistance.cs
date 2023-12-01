using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DynamicShadowDistance : MonoBehaviour
{
    public Camera mainCamera; // Assign your main camera here

    // Set the FOV and shadow distance ranges
    private const float minFov = 3f;
    private const float maxFov = 75f;
    public float minShadowDistance = 300f;
    public float maxShadowDistance = 600f;

    void Update()
    {
        // Ensure the camera is assigned
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned.");
            return;
        }

        // Get the current FOV of the camera
        float currentFov = mainCamera.fieldOfView;

        // Clamp the FOV to the min and max values to avoid out-of-range results
        currentFov = Mathf.Clamp(currentFov, minFov, maxFov);

        // Calculate the t parameter for the lerp function
        float t = (currentFov - minFov) / (maxFov - minFov);

        // Linearly interpolate the shadow distance based on the current FOV
        float shadowDistance = Mathf.Lerp(maxShadowDistance, minShadowDistance, t);

        // Apply the calculated shadow distance to the URP settings
        UniversalRenderPipelineAsset urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            urpAsset.shadowDistance = shadowDistance;
        }
        else
        {
            Debug.LogError("The current Render Pipeline Asset is not URP.");
        }
    }
}
