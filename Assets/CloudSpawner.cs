using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CloudSpawner : MonoBehaviour
{
    public List<GameObject> objectsToSpawn; // Assign this list in the inspector with your objects
    public List<Transform> spawnLocations; // Assign spawn locations in the inspector
    public GameObject island; // Assign the island GameObject in the inspector

    public GameObject cloudsParent;

    void Start()
    {
        StartCoroutine(SpawnRandomObjectEverySecond());
    }

    IEnumerator SpawnRandomObjectEverySecond()
    {
        // Check if the lists are set up correctly
        if (objectsToSpawn.Count == 0 || spawnLocations.Count == 0 || island == null)
        {
            Debug.LogError("Setup Error: Make sure objects and spawn locations are assigned and island is set.");
            yield break; // Exit the coroutine if setup is incorrect
        }

        while (true) // Infinite loop to keep spawning objects every second
        {
            SpawnRandomObjectAtRandomLocation();
            yield return new WaitForSeconds(.1f); // Wait for 1 second before spawning the next object
        }
    }

    void SpawnRandomObjectAtRandomLocation()
{
    // Choose a random object from the objects list
    int objectIndex = Random.Range(0, objectsToSpawn.Count);
    GameObject chosenObject = objectsToSpawn[objectIndex];

    // Choose a random location from the spawn locations list
    int locationIndex = Random.Range(0, spawnLocations.Count);
    Transform chosenLocation = spawnLocations[locationIndex];

    // Calculate a random offset for the Y position
    float randomYOffset = Random.Range(-250f, -50f);

    // Instantiate the object at the chosen spawn location with the random Y offset
    Vector3 spawnPosition = chosenLocation.position + new Vector3(0, randomYOffset, 0);
    GameObject spawnedObject = Instantiate(chosenObject, spawnPosition, Quaternion.identity, cloudsParent.transform);

    // Store initial Z rotation
    float initialZRotation = spawnedObject.transform.eulerAngles.z;

    // Make the spawned object look at the island
    spawnedObject.transform.LookAt(island.transform.position);

    // Correct the rotation by adding an additional 90 degrees around the Y axis without changing the Z rotation
    Vector3 correctedRotation = spawnedObject.transform.eulerAngles;
    correctedRotation.y += 90; // Adjust Y rotation if necessary
    correctedRotation.z = initialZRotation; // Reset Z rotation to its initial value

    // Apply the corrected rotation to the spawned object
    spawnedObject.transform.eulerAngles = correctedRotation;
}


}
