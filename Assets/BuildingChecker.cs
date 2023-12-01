using UnityEngine;

public class BuildingChecker : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // Cast a ray straight down.
        RaycastHit hit;

        // You might want to adjust the length of the ray if needed
        float rayLength = 100.0f; 

        // Check if the ray hits any collider
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, rayLength))
        {
            // Check if the hit collider has the "Building" tag
            if (hit.collider.CompareTag("Building"))
            {
                Debug.Log("The collider below has the tag 'Building'");
                // Do something here when a building is found below
            }
            else
            {
                Debug.Log("The collider below is not a 'Building'");
                // Do something else here when the collider below is not a building
            }
        }
        else
        {
            Debug.Log("No collider found below");
            // Do something here if no collider is found below at all
        }
    }
}
