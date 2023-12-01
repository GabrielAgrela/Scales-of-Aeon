using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameMaster : MonoBehaviour
{
    public float incrementInterval = 0.5f; // X seconds as the increment interval
    public string username = "testUser";
    public int day = 0;
    public int score = 0;
    public DataBase database;
    private List<Category> categoriesList;
    public Camera gameCamera; // Assign this in the inspector or initialize in Start/Awake

    public float smoothSpeed = 0.001f;
    public Vector3 offset;

    public float debug;
    public bool check = true;
    public GameObject dialogWindow;

    [SerializeField]
    private GameObject cardUI;

    public float timebetweenDilemmas = 15f;

    private GameBalanceManager gameBalanceManager;

    private void Awake()
    {
        if (cardUI == null)
        {
            cardUI = GameObject.FindWithTag("Card");
        }
        username = GameObject.Find("PlayerData")?.GetComponent<LoginSceneData>().username;
        Application.runInBackground = true;
        database.GetComponent<DataBase>();
        smoothSpeed = 0.001f;
        // Find all category components in the game
        categoriesList = new List<Category>(FindObjectsOfType<Category>());
        gameCamera = Camera.main;

        gameBalanceManager = gameObject.GetComponent<GameBalanceManager>();
        StartCoroutine(createCategoryIncrementsRoutines());
        StartCoroutine(changeOffSet());
        StartCoroutine(checkEvent());
        StartCoroutine(ShowCardUI());
    }

    private void Start()
    {
        cardUI.SetActive(false);

        StartCoroutine(Tutorial());
    }

    IEnumerator Tutorial()
    {
        yield return new WaitForSeconds(1f);
        DialogWindow("Aeonarch "+username + "#"+database.UI+"! Welcome to Scaleden!");
        yield return new WaitForSeconds(5f);
        dialogWindow.GetComponent<Animator>().SetBool("Close", true);
        yield return new WaitForSeconds(.5f);
        dialogWindow.SetActive(false);
        yield return new WaitForSeconds(.5f);
        dialogWindow.GetComponent<Animator>().SetBool("Close", false);
        DialogWindow("You are the ruler of this island. Your goal is to keep the island in balance.");
        yield return new WaitForSeconds(6f);
        dialogWindow.GetComponent<Animator>().SetBool("Close", true);
        yield return new WaitForSeconds(.5f);
        dialogWindow.SetActive(false);
        yield return new WaitForSeconds(.5f);
        dialogWindow.GetComponent<Animator>().SetBool("Close", false);
        DialogWindow("Use AWSD to move around the island, Q and E to rotate the camera, and the MOUSER WHEEL to zoom in and out.");
        yield return new WaitForSeconds(12f);
        dialogWindow.GetComponent<Animator>().SetBool("Close", true);
        yield return new WaitForSeconds(.5f);
        dialogWindow.SetActive(false);
        yield return new WaitForSeconds(.5f);
        DialogWindow("Good luck!");
        yield return new WaitForSeconds(2f);
        dialogWindow.GetComponent<Animator>().SetBool("Close", true);
        yield return new WaitForSeconds(1f);
        dialogWindow.SetActive(false);
    }

    void Update() {
        // Example trigger: Pressing the 'G' key
        if (Input.GetKeyDown(KeyCode.G)) {
            TriggerDanceDiseaseEvent();
        }
    }

    private void TriggerDanceDiseaseEvent() {
        GameObject[] geckos = GameObject.FindGameObjectsWithTag("Villager"); // Replace with the correct tag
        foreach (GameObject gecko in geckos) {
            VillagerBehaviour villagerBehaviour = gecko.GetComponent<VillagerBehaviour>();
            if (villagerBehaviour != null) {
                villagerBehaviour.chooseSMType = VillagerBehaviour.SM.DanceDisease;
            }
        }
    }

    IEnumerator checkEvent()
    {
        Category highestCategory = null;
        Category lowestCategory = null;
        while (check)
        {
            // if sum of category points is greater than 100, then event
            float sum = 0;
            foreach (var category in categoriesList)
            {
                sum += category.Points;
            }
            if (sum >= 400)
            {
                // event
                DialogWindow("Scaleden has reached its peak at day " + day + "! \nCongratulations!");
                database.SaveNewHighScore(score);
                check = false;
                GameObject.Find("GameOver").transform.GetChild(0).gameObject.SetActive(true);
            }

            // find highest and lowest category
            highestCategory = categoriesList[0];
            lowestCategory = categoriesList[0];
            foreach (var category in categoriesList)
            {
                if (category.Points > highestCategory.Points)
                {
                    highestCategory = category;
                }
                if (category.Points < lowestCategory.Points)
                {
                    lowestCategory = category;
                }
            }
            float loseEquation = 0;
            if (sum < 40)
{
    loseEquation = 5.0f;
}
else if (sum > 360)
{
    loseEquation = 8.0f;
}
else
{
    // Linear scaling from 5 to 10 for sums between 40 and 360
    loseEquation = 5.0f + (sum - 40) * (8.0f - 5.0f) / (360 - 40);
}


            debug = loseEquation;


            if (highestCategory.Points > Mathf.Ceil(lowestCategory.Points + loseEquation) && lowestCategory.Points > 0 && sum > 40)
            {
                GameObject.Find("PlayerData").GetComponent<AudioSource>().Pause();
                check = false;
                print("lost " + lowestCategory.Type.ToString());
                if (lowestCategory.Type.ToString() == "Economy")
                {
                    // look for every villager in the economy category
                    GameObject[] villagers = GameObject.FindGameObjectsWithTag("Villager");
                    foreach (var villager in villagers)
                    {
                        if (villager.GetComponent<VillagerBehaviour>().category.Type.ToString() == "Economy")
                        {
                            // move them to the highest category
                            villager.GetComponent<VillagerBehaviour>().chooseSMType = VillagerBehaviour.SM.Suicide;
                            FollowGameObjectWithCamera(FindClosestVillagerByCategory(new Vector3(100, 0, -100), lowestCategory.Type.ToString()));
                            DialogWindow("You Lost to Economy Faction at day " + day + "\n Your neglect towards them as led to a financial crisis... Let's see how they react.");
                            database.SaveNewHighScore(score, "Financial Crisis");
                        }
                    }
                }
                else if (lowestCategory.Type.ToString() == "Religion")
                {
                    // look for every villager in the economy category
                    GameObject[] villagers = GameObject.FindGameObjectsWithTag("Villager");
                    foreach (var villager in villagers)
                    {
                        if (villager.GetComponent<VillagerBehaviour>().category.Type.ToString() == "Religion")
                        {
                            // move them to the highest category
                            villager.GetComponent<VillagerBehaviour>().chooseSMType = VillagerBehaviour.SM.Allah;
                            smoothSpeed = 0.01f;
                            FollowGameObjectWithCamera(FindClosestVillagerByCategory(new Vector3(0, 0, 0), lowestCategory.Type.ToString()));
                            DialogWindow("You Lost to Religion Faction at day " + day + "\n Seems like they are not happy with your decisions... I wonder what actions they will take.");
                            database.SaveNewHighScore(score, "Martyrdom");
                            //FollowGameObjectWithCamera(GameObject.Find("PalacePosition"));
                        }
                    }
                }
                else if (lowestCategory.Type.ToString() == "Military")
                {
                    GameObject[] villagers = GameObject.FindGameObjectsWithTag("Villager");
                    foreach (var villager in villagers)
                    {
                        if (villager.GetComponent<VillagerBehaviour>().category.Type.ToString() == "Military")
                        {
                            villager.GetComponent<VillagerBehaviour>().chooseSMType = VillagerBehaviour.SM.Shoot;
                            // find closest villager with category "Military" to any other villager with a category thats not "military"

                            FollowGameObjectWithCamera(FindClosestVillagerByCategory(FindClosestMilitaryVillagerToNonMilitary().transform.position, lowestCategory.Type.ToString()));
                            DialogWindow("You Lost to Military Faction at day " + day + "\n They've had enough of your rule! I think they are angry...");
                            database.SaveNewHighScore(score, "Military Revolt");
                        }
                    }
                }
                else if (lowestCategory.Type.ToString()=="People")
                {
                    GameObject[] villagers = GameObject.FindGameObjectsWithTag("Villager");
                    foreach (var villager in villagers)
                    {
                        if (villager.GetComponent<VillagerBehaviour>().category.Type.ToString() == "People")
                        {
                            villager.GetComponent<VillagerBehaviour>().chooseSMType = VillagerBehaviour.SM.DanceDisease;
                            FollowGameObjectWithCamera(FindClosestVillagerByCategory(new Vector3(0,0,0), lowestCategory.Type.ToString()));
                            DialogWindow("You Lost to the People Faction at day " + day + "\n You've been such an awful Aeonarch the people turned crazy! ");
                            database.SaveNewHighScore(score, "Mass Hysteria");
                        }
                    }
                }
            }

            yield return new WaitForSeconds(.3f);
        }

    }

    // this should be generalized 
    GameObject FindClosestMilitaryVillagerToNonMilitary()
    {
        GameObject[] villagers = GameObject.FindGameObjectsWithTag("Villager");
        GameObject closestMilitaryVillager = null;
        float closestDistance = float.MaxValue;

        foreach (var villager in villagers)
        {
            if (villager.GetComponent<VillagerBehaviour>().category.Type.ToString() == "Military")
            {
                foreach (var otherVillager in villagers)
                {
                    if (otherVillager.GetComponent<VillagerBehaviour>().category.Type.ToString() != "Military")
                    {
                        float distance = Vector3.Distance(villager.transform.position, otherVillager.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestMilitaryVillager = villager;
                        }
                    }
                }
            }
        }

        return closestMilitaryVillager;
    }

    IEnumerator createCategoryIncrementsRoutines()
    {
        yield return new WaitForSeconds(0);

        foreach (var category in categoriesList)
        {
            // delay random
            yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
            StartCoroutine(IncrementPointsPerCategory(category));
        }
    }

    IEnumerator IncrementPointsPerCategory(Category category)
    {
        category.AddRemovePoints(1);
        category.AddBuildings(1);
        yield return new WaitForSeconds(incrementInterval + category.spawnRate);
        if (category.Points < category.MaxPoints && check)
        {
            StartCoroutine(IncrementPointsPerCategory(category));
        }

    }

    public void IncrementPointsPerCategoryPerDecision(string categoryType)
    {
        Debug.Log("Incrementing points for category: a" + categoryType + "a");
        Category category = categoriesList.Find(x => x.Type.ToString() == categoryType);
        if (category != null)
        {
            gameBalanceManager.ApplyIncrementPointsBalance(category);
        }
    }

    public void DecrementPointsPerCategoryPerDecision(string categoryType)
    {
        Category category = categoriesList.Find(x => x.Type.ToString() == categoryType);
        if (category != null)
        {
            gameBalanceManager.ApplyDecrementPointsBalance(category);
        }
        else
        {
            Debug.LogError("Category not found: " + categoryType);
        }
    }

    public void IncrementScore()
    {
        day++;
        //some combination of day and balance of points or some shit, days will do for now.
        score = day;
        database.SaveNewHighScore(score);

    }

    public GameObject FindClosestVillagerByCategory(Vector3 destination, string category)
    {
        GameObject[] villagers = GameObject.FindGameObjectsWithTag("Villager");
        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (var villager in villagers)
        {
            if (villager.GetComponent<VillagerBehaviour>().category.Type.ToString() == category)
            {
                float distance = Vector3.Distance(destination, villager.transform.position);
                if (distance < closestDistance)
                {
                    closest = villager;
                    closestDistance = distance;
                }
            }
        }
        return closest;
    }

    IEnumerator changeOffSet()
    {
        yield return new WaitForSeconds(0);
        while (true)
        {
            offset = new Vector3(UnityEngine.Random.Range(5, 15), UnityEngine.Random.Range(5, 20), UnityEngine.Random.Range(5, 15));
            yield return new WaitForSeconds(10);
        }
    }


    public void FollowGameObjectWithCamera(GameObject target)
    {
        GameObject.Find("Main Camera").GetComponent<CameraController>().enabled = false;
        // smooth gamecamera-fielofview to 60
        StartCoroutine(SmoothCameraFieldOfView(60));

        StartCoroutine(FollowTargetSmoothly(target));
    }



    private IEnumerator SmoothCameraFieldOfView(float targetFieldOfView)
    {
        while (gameCamera.fieldOfView != targetFieldOfView)
        {
            gameCamera.fieldOfView = Mathf.Lerp(gameCamera.fieldOfView, targetFieldOfView, smoothSpeed);
            yield return null;
        }
    }

    private IEnumerator FollowTargetSmoothly(GameObject target)
    {
        while (target != null)
        {
            Vector3 desiredPosition = target.transform.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(gameCamera.transform.position, desiredPosition, smoothSpeed);
            gameCamera.transform.position = smoothedPosition;

            gameCamera.transform.LookAt(target.transform);

            yield return null;
        }
    }

    public void DialogWindow(string text)
    {
        dialogWindow.SetActive(true);
        dialogWindow.transform.GetChild(0).transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = text;
    }

    private IEnumerator ShowCardUI()
    {
        float elapsedTime = 0f;
        while (true)
        {
            if (!cardUI.activeSelf)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= timebetweenDilemmas && check)
                {
                    GameObject.Find("PlayerData").GetComponent<AudioSource>().Pause();
                    cardUI.SetActive(true);
                    Time.timeScale = 0;
                    elapsedTime = 0f;

                    // Reset the time between dilemmas after a choice is made
                    timebetweenDilemmas = UnityEngine.Random.Range(15f, 30f);
                }
            }
            // The else part is removed because the time is now frozen
            yield return null;
        }
    }
}
