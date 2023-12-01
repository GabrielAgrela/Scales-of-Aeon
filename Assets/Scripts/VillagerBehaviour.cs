using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEditor;
using System.Security.Cryptography;
using System.Collections;

public class VillagerBehaviour : MonoBehaviour
{
    private NavMeshAgent agent;
    public StateMachine<State> currSM;
    // Add other state machines as needed

    public Vector3 middlePosition;
    public Vector3 edgePosition;
    public Vector3 homePosition;

    public GameObject palace;

    public AudioClip AllahFX;

    public AudioClip SuicideFX;
    public AudioClip ShootFX;

    public GameObject ExplosionFX;
    public GameObject ShootPFX;
    public GameObject pianist;
    public GameObject dj;
    public GameObject DiscoBall;

    public bool pianistSpawned = false;
    public bool djSpawned = false;

    private AudioSource audioSource;

    public Category category;
    public string categoryString;

    public GridManager gridManager;

    public Vector3 targetPosition;
    public GameObject targetVillager;
    public State currState;
    public SM chooseSMType;
    private SM currSMType;

    public Vector3 geckoDancingPoint;

    private Vector3 GetRandomDancePosition(float radius) 
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += geckoDancingPoint;
        return new Vector3(randomDirection.x, transform.position.y, randomDirection.z);
    }


    void Start()
    {
        gridManager = GameObject.Find("GridSystem").GetComponent<GridManager>();
        palace = GameObject.Find("Palace");
        category = GameObject.Find(categoryString).GetComponent<Category>();
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();

        
        
        
        StartCoroutine(StateMachineUpdate());
    }
    IEnumerator StateMachineUpdate()
    {
        yield return new WaitForSeconds(1f);
        switch (chooseSMType)
        {
            case SM.Wander:
                currSM = createWanderSM();
                break;
            case SM.Shoot:
                currSM = createShootEveryoneSM();
                break;
            case SM.Suicide:
                currSM = createSuicideSM();
                break;
            case SM.Allah:
                currSM = createAllahSM();
                break;
        }
        currSMType=chooseSMType;
        while (true)
        {

            yield return new WaitForSeconds(0.3f);
            if (currSMType != chooseSMType)
            {
                switch (chooseSMType)
                {
                    case SM.Wander:
                        currSM = createWanderSM();
                        break;
                    case SM.Shoot:
                        currSM = createShootEveryoneSM();
                        break;
                    case SM.Suicide:
                        currSM = createSuicideSM();
                        break;
                    case SM.Allah:
                        currSM = createAllahSM();
                        break;
                    case SM.DanceDisease:
                        currSM = createDanceDiseaseSM();
                        break;
                }
                currSMType=chooseSMType;
            }
            
            currSM.Update();
            currState = currSM.currentState;

             // Adjust this value to change the update frequency
        }
    }

    public StateMachine<State> createShootEveryoneSM()
    {
        agent.speed=5;
        currSM = new StateMachine<State>(State.ChooseDestination);

        currSM.SetStateAction(State.ChooseDestination, ChooseClosestVillager);
        currSM.SetStateAction(State.IsTargetAlive, () => print("Istargetalive"));
        currSM.SetStateAction(State.Moving, () => agent.SetDestination(targetVillager.transform.position));
        currSM.SetStateAction(State.Shoot,  () => 
        {
            try
            {
                Shoot();
            }
            catch (System.Exception)
            {
                print("Shot by another fella");
            }
        });

        currSM.AddTransition(State.ChooseDestination, State.IsTargetAlive,State.ChooseDestination, () => true);
        currSM.AddTransition(State.IsTargetAlive, State.Moving, State.ChooseDestination, () => CheckTargetAlive());
        currSM.AddTransition(State.Moving, State.Shoot,State.ChooseDestination, () => IsAtPositionDynamic(targetVillager, 5));
        currSM.AddTransition(State.Shoot, State.ChooseDestination,State.ChooseDestination, () => true);

        currSM.FirstState();
        return currSM;
    }


    public StateMachine<State> createWanderSM()
    {
        
        currSM = new StateMachine<State>(State.ChooseDestination);

        currSM.SetStateAction(State.ChooseDestination, ChooseRandomDestination);
        currSM.SetStateAction(State.Moving, () => agent.SetDestination(targetPosition));
        currSM.SetStateAction(State.ReachedDestination, ChooseRandomDestination);

        currSM.AddTransition(State.ChooseDestination, State.Moving,State.ChooseDestination, () => targetPosition != Vector3.zero);
        currSM.AddTransition(State.Moving, State.ReachedDestination,State.Moving, () => IsAtPosition(targetPosition, 5));
        currSM.AddTransition(State.ReachedDestination, State.ChooseDestination,State.ReachedDestination, () => true);

        currSM.FirstState();
        return currSM;
    }

    

    public StateMachine<State> createSuicideSM()
    {
        agent.speed=5f;
        currSM = new StateMachine<State>(State.WalkingToMiddle);

        currSM.SetStateAction(State.WalkingToMiddle, () => agent.SetDestination(middlePosition));
        currSM.SetStateAction(State.AtMiddle, () => Pianist());
        currSM.SetStateAction(State.WalkingToEdge, () => agent.SetDestination(edgePosition));
        currSM.SetStateAction(State.JumpOff, () => Suicide());
        // Define other state actions

        // Define transitions
        currSM.AddTransition(State.WalkingToMiddle, State.AtMiddle,State.WalkingToMiddle, () => IsAtPosition(middlePosition));
        currSM.AddTransition(State.AtMiddle, State.WalkingToEdge, State.AtMiddle, () => true); // Immediate transition
        currSM.AddTransition(State.WalkingToEdge, State.JumpOff, State.WalkingToEdge, () => IsAtPosition(edgePosition));

        currSM.FirstState();
        return currSM;
    }

    public StateMachine<State> createDanceDiseaseSM()
    {
        agent.speed = 5;
        currSM = new StateMachine<State>(State.WalkingToDancingPoint);

        currSM.SetStateAction(State.WalkingToDancingPoint, () => {
            if (IsAtPosition(geckoDancingPoint, 5)) {
                targetPosition = GetRandomDancePosition(5); // 5 is the radius
                agent.SetDestination(targetPosition);
                currState = State.MovingToRandomDancePosition; // Directly set the current state
            } else {
                agent.SetDestination(geckoDancingPoint);
            }
        });

        currSM.SetStateAction(State.AtDancingPoint, () => Dj());

        currSM.SetStateAction(State.MovingToRandomDancePosition, () => {
            agent.SetDestination(targetPosition);
            if (IsAtPosition(targetPosition, 1)) {
                currState = State.Dancing; // Directly set the current state
            }
        });

        currSM.SetStateAction(State.Dancing, StartDancing);

        // Add transitions as necessary based on your state machine's design
        currSM.AddTransition(State.WalkingToDancingPoint, State.AtDancingPoint, State.WalkingToDancingPoint, () => IsAtPosition(geckoDancingPoint));
        currSM.AddTransition(State.AtDancingPoint,State.MovingToRandomDancePosition,State.AtDancingPoint, () => true);
        currSM.AddTransition(State.MovingToRandomDancePosition, State.Dancing, State.MovingToRandomDancePosition, () => true);

        currSM.FirstState();
        return currSM;
    }

    public StateMachine<State> createAllahSM()
    {
        // Initialize state machines
        currSM = new StateMachine<State>(State.RunToPalace);
        currSM.SetStateAction(State.RunToPalace, () => agent.SetDestination(homePosition));
        currSM.SetStateAction(State.Explode, () => Explode());
        
         // Define transitions
        currSM.AddTransition(State.RunToPalace, State.Explode, State.RunToPalace, () => IsAtPosition(palace.transform.position, 10f));

        currSM.FirstState();
        return currSM;
    }

    public void Pianist()
    {

        //  search for gameobject "Pianist"
        if (GameObject.Find("Pianist(Clone)") == null)
        {
            pianistSpawned = true;
            Instantiate(pianist, pianist.transform.position, pianist.transform.rotation);
        }
    }

    public void Dj()
    {
        if (GameObject.Find("GeckoDJWithTable(Clone)") == null)
        {
            djSpawned = true;
            Instantiate(dj, dj.transform.position, dj.transform.rotation);
            Instantiate(DiscoBall, DiscoBall.transform.position, DiscoBall.transform.rotation);
        }
    }

    public bool CheckTargetAlive()
    {
        if (targetVillager == null)
        {
            print("Target is dead");
            return false;
        }
        else
        {
            print("Target is alive");
            return true;
        }
    }

    public void ChooseClosestVillager()
    {
        GameObject[] villagers = GameObject.FindGameObjectsWithTag("Villager");
        GameObject closestVillager = null;
        float closestDistance = float.MaxValue;

        foreach (var villager in villagers)
        {
            try
            {
                if (villager == gameObject || villager.GetComponent<VillagerBehaviour>().category.StringToCategoryType(villager.GetComponent<VillagerBehaviour>().category.Type)=="Military") continue;
            }
            catch (System.Exception)
            {
                
                print("Categories still not attributed");
            }
            

            float distance = Vector3.Distance(villager.transform.position, transform.position);
            if (distance < closestDistance)
            {
                closestVillager = villager;
                closestDistance = distance;
            }
        }

        targetVillager = closestVillager;
    }


    public void Shoot()
    {
        GameObject.Find("GameOver").transform.GetChild(0).gameObject.SetActive(true);
        ShootPFX.SetActive(false);
        audioSource.clip = ShootFX;
        audioSource.Play();
        ShootPFX.SetActive(true);
        targetVillager.GetComponent<Animator>().applyRootMotion = false;
        targetVillager.GetComponent<NavMeshAgent>().enabled = false;

        // look at target
        transform.LookAt(targetVillager.transform);
        targetVillager.transform.LookAt(transform);

        Rigidbody rb = targetVillager.GetComponent<Rigidbody>();
        targetVillager.GetComponent<Animator>().SetBool("falling", true);

        // force should be in the direction of the target
        Vector3 direction = (targetVillager.transform.position - transform.position).normalized;
        //rb.AddTorque(new Vector3(-1, -1, -1) * 200, ForceMode.Impulse);
        rb.AddForce(direction * 2, ForceMode.Impulse);
        StartCoroutine(DestroyTargetAfterDelay(2f,targetVillager));
    }

    private IEnumerator DestroyTargetAfterDelay(float delay,GameObject targetVillagerT)
    {
        yield return new WaitForSeconds(delay);
        Destroy(targetVillagerT);
    }

    private void ChooseRandomDestination()
    {
        Vector2Int randomGridCell = gridManager.GetRandomCellInCategory(category);
        targetPosition = gridManager.GridToWorld(randomGridCell.x, randomGridCell.y);
    }

    public void Suicide()
    {
        GameObject.Find("GameOver").transform.GetChild(0).gameObject.SetActive(true);
        audioSource.clip = SuicideFX;
        audioSource.Play();
        Animator animator = GetComponent<Animator>();
        animator.SetBool("falling",true);

        Vector3 directionToMiddle = middlePosition - transform.position;
        directionToMiddle.y = -.5f; // Remove vertical component to ensure horizontal push
        Vector3 pushDirection = -directionToMiddle.normalized; // Opposite direction

        // render push direction as a red line for debugging
        //pushDirection = new Vector3(pushDirection.x, 45, pushDirection.z);
        Debug.DrawRay(transform.position, pushDirection * 20, Color.red, 5f);


        Rigidbody rb = GetComponent<Rigidbody>();
        GetComponent<Animator>().applyRootMotion = false;
        GetComponent<NavMeshAgent>().enabled = false;
        

        if (rb != null)
        {
            rb.AddForce(pushDirection, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Rigidbody is not attached to the GameObject.");
        }
    }

    public void Explode()
    {
        GameObject.Find("GameOver").transform.GetChild(0).gameObject.SetActive(true);
        audioSource.clip = AllahFX;
        audioSource.Play();
        GetComponent<Animator>().SetBool("falling", true);
        Invoke("ExplodeFX", 1f); // Call PrintExplode after 1 second
    }

    void ExplodeFX()
    {
        GameObject explosion = Instantiate(ExplosionFX, transform.position, Quaternion.identity);
        // add rigidbody if doesnt exist to palace
        Rigidbody rb = palace.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = palace.AddComponent<Rigidbody>();
        }

        Vector3 direction = (rb.transform.position - explosion.transform.position).normalized;
        Vector3 force = direction + Vector3.up;
        rb.AddTorque(new Vector3(-1, -1, -1) * 200, ForceMode.Impulse);
        rb.AddForce(force * 100, ForceMode.Impulse);

        // Add rotation
        

        Destroy(gameObject);
    }


    

    private bool IsAtPosition(Vector3 targetPosition, float distanceOffset = 0.0f)
    {
        return !agent.pathPending && agent.remainingDistance <= (agent.stoppingDistance + distanceOffset) && agent.hasPath;
    }

    private bool IsAtPositionDynamic(GameObject target, float distanceOffset = 0.0f)
    {
        if (target == null)
        {
            return true;
        }
        agent.SetDestination(target.transform.position);
        return !agent.pathPending && agent.remainingDistance <= (agent.stoppingDistance + distanceOffset) && agent.hasPath;
    }

    private void StartDancing() {
        GameObject.Find("GameOver").transform.GetChild(0).gameObject.SetActive(true);
        
        GetComponent<Animator>().SetBool("Dancing", true);
    }


    // State machine class
    


    public enum State
    {
        WalkingToMiddle,
        AtMiddle,
        WalkingToEdge,
        JumpOff,
        GoHome,
        GrabWeapon,
        RunToPalace,
        Explode,
        ChooseDestination,
        Moving,
        ReachedDestination,
        Shoot,
        IsTargetAlive,
        WalkingToDancingPoint,
        MovingToRandomDancePosition,
        AtDancingPoint,
        Dancing
        // Add other states as needed
    }

    public enum SM
    {
        Wander,
        Shoot,
        Suicide,
        Allah,
        DanceDisease
    }
}
