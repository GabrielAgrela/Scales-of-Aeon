using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class DummyBuilderController : MonoBehaviour
{
    private NavMeshAgent agent;

    public Category category;

    public GameObject building;
    public bool waiting = false;
    public GameObject constructionFX;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        GetComponent<Animator>().SetBool("waiting", true);
    }

    private void Update()
    {
        try
        {
            if (agent.remainingDistance < agent.stoppingDistance + 1 && agent.hasPath)
            {
                if (!waiting)
                    StartCoroutine(BuildCouroutine());

            }
            else if (category.unconstructedBuildingsList.Count > 0)
            {
                
                agent.isStopped = false;
                GetComponent<Animator>().SetBool("walking", true);
                GetComponent<Animator>().SetBool("waiting", false);
                agent.SetDestination(category.unconstructedBuildingsList[0].transform.position);
            }

        }
        catch (System.Exception e)
        {

        }

    }

    private IEnumerator BuildCouroutine()
    {
        if (category.unconstructedBuildingsList.Count > 0)
        {
            GameObject targetBuilding = category.unconstructedBuildingsList[0];
            var newBuildingPosition = targetBuilding.transform.position;
            var newBuildingRotation = targetBuilding.transform.rotation;
            var newBuildingParent = targetBuilding.transform.parent;

            if (targetBuilding == null)
            {
                Debug.LogError("Target building is null.");
                yield break;
            }

            // Look at building
            transform.LookAt(targetBuilding.transform.position);
            GetComponent<Animator>().SetBool("walking", false);
            GetComponent<Animator>().SetBool("building", true);

            waiting = true;

            PlayRandomSound playSound = targetBuilding.GetComponent<PlayRandomSound>();
            if (playSound != null)
            {
                playSound.PlaySoundRandom();
            }
            else
            {
                Debug.LogError("PlayRandomSound component missing in the target building.");
            }

            Instantiate(constructionFX, targetBuilding.transform.position, targetBuilding.transform.rotation, targetBuilding.transform.parent);

            yield return new WaitForSeconds(1f);

            building = category.getRandomBuilding();

            // Ensure building prefab is not null
            if (building != null && category.unconstructedBuildingsList.Count > 0)
            {
                // Destroy target building
                Destroy(targetBuilding);

                // Instantiate new building after the target building is destroyed
                var newBuilding = Instantiate(building, newBuildingPosition, newBuildingRotation, newBuildingParent);
                category.constructedBuildingsList.Add(newBuilding);
                waiting = false;
            }
            else
            {
                Debug.LogError("The building prefab is null.");
            }

            // Remove the destroyed building from the list
            if (category.unconstructedBuildingsList.Count > 0)
            {
                category.unconstructedBuildingsList.RemoveAt(0);
            }

            agent.isStopped = true;
            agent.ResetPath();
            GetComponent<Animator>().SetBool("building", false);
        }
        else
        {
            Debug.LogError("No elements in unconstructedBuildingsList");
        }
    }

}
