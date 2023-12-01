using UnityEngine;

[System.Serializable]
public class SpawnObjectSettings
{
    public GameObject objectToSpawn;
    public int numberOfObjects = 10;
    public float minSize = 0.5f;
    public float maxSize = 2.0f;
    public float noiseScale = 0.05f; // Smaller for smoother, larger pockets
    public float spawnThreshold = 0.5f; // Adjust this to control the density of pockets
    public int seed = 12345; // Consistent seed for repeatable results
    
}

public class PerlinNoiseSpawner : MonoBehaviour
{
    public SpawnObjectSettings[] spawnSettings;
    public GameObject surface;
    public LayerMask groundLayer;
    public float exclusionRadius = 10f; // Radius to exclude spawning in the middle
    public GameObject vegetation;

    private void Start()
    {
        //make seed random
        spawnSettings[0].seed = Random.Range(0, 100000);
        Random.InitState(spawnSettings[0].seed); // Initialize with a consistent seed
        foreach (var settings in spawnSettings)
        {
            SpawnObjects(settings);
        }
    }

    void SpawnObjects(SpawnObjectSettings settings)
    {
        Renderer surfaceRenderer = surface.GetComponent<Renderer>();
        Vector3 surfaceBounds = surfaceRenderer.bounds.size;
        Vector3 center = new Vector3 (surface.transform.position.x-5, surfaceRenderer.bounds.max.y, surface.transform.position.z-11);

        for (int i = 0; i < settings.numberOfObjects; i++)
        {
            Vector3 randomPosition;
            do
            {
                randomPosition = new Vector3(
                    Random.Range(-surfaceBounds.x / 2, surfaceBounds.x / 2),
                    0,
                    Random.Range(-surfaceBounds.z / 2, surfaceBounds.z / 2)
                ) + center;
            } while (Vector3.Distance(randomPosition, center) < exclusionRadius);

            float perlinValue = Mathf.PerlinNoise(randomPosition.x * settings.noiseScale, randomPosition.z * settings.noiseScale);

            if (perlinValue > settings.spawnThreshold)
            {
                float groundY = GetGroundY(randomPosition);
                if (groundY != float.MinValue)
                {
                    randomPosition.y = groundY;
                    GameObject newObject = Instantiate(settings.objectToSpawn, randomPosition, Quaternion.Euler(0, Random.Range(0, 360), 0), vegetation.transform);
                    newObject.transform.localScale *= Random.Range(settings.minSize, settings.maxSize);
                }
            }
        }
    }

    float GetGroundY(Vector3 position)
{
    float rayLength = 50; // Length of the ray to cast downwards
    float sideRayDistance = 20; // Distance to cast side rays to check for edges
    float minHeightDifference = 1; // Minimum height difference to consider it an edge

    // Cast a ray straight down from the position.
    if (Physics.Raycast(position + Vector3.up * rayLength, Vector3.down, out RaycastHit hit, rayLength * 2, groundLayer))
    {
        // Cross check rays
        Vector3[] directions = new Vector3[]
        {
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back
        };

        foreach (Vector3 dir in directions)
        {
            if (!Physics.Raycast(position + dir * sideRayDistance + Vector3.up * rayLength, Vector3.down, out RaycastHit sideHit, rayLength * 2, groundLayer) ||
                Mathf.Abs(sideHit.point.y - hit.point.y) > minHeightDifference)
            {
                // If one of the side rays doesn't hit the ground or there's a significant height difference, it's too close to the edge.
                return float.MinValue;
            }
        }

        // All side checks passed, we can spawn at this point.
        return hit.point.y;
    }

    return float.MinValue; // If the downward ray didn't hit the ground, it's not a valid spawn point.
}


}
