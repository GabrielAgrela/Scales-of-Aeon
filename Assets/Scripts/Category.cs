using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[System.Serializable]
public class Category : MonoBehaviour
{
    [SerializeField]
    private Sprite[] _backgroundImages;
    public Sprite[] backgroundImages { get => _backgroundImages; }

    public GridManager gridManager;

    [SerializeField]
    private CategoryType _type;
    public CategoryType Type { get => _type; }

    [SerializeField]
    private int _buildingCount;
    public int BuildingCount { get => _buildingCount; private set => _buildingCount = Mathf.Max(0, value); }

    [SerializeField]
    private float _points;
    public float Points { get => _points; set => _points = value; }

    [SerializeField]
    private int _maxPoints;
    public int MaxPoints { get => _maxPoints; }

    [SerializeField]
    private int _minPoints;
    public int MinPoints { get => _minPoints; set => _minPoints = value; }

    [SerializeField]
    private float _pointsPerTurn;

    public float PointsPerTurn { get => _pointsPerTurn; set => _pointsPerTurn = value; }

    public GameObject[] villagerPrefab;

    public int villagerCount = 0;

    public GameObject[] buildingPrefab;

    public GameObject destroyFX;

    /// <summary>
    /// Event triggered when a building is added to a category.
    /// </summary>
    /// <param name="category">The category to which a building is added.</param>
    public event Action<Category> OnBuildingAdded;

    public List<GameObject> constructedBuildingsList = new List<GameObject>();
    public List<GameObject> unconstructedBuildingsList = new List<GameObject>();

    public float spawnRate = 1f;

    public string StringToCategoryType(CategoryType type)
    {
        switch (type)
        {
            case CategoryType.People:
                return "People";
            case CategoryType.Religion:
                return "Religion";
            case CategoryType.Economy:
                return "Economy";
            case CategoryType.Military:
                return "Military";
            default:
                return "Error";
        }
    }
    public GameObject getRandomBuilding()
    {
        return buildingPrefab[UnityEngine.Random.Range(0, buildingPrefab.Length)];
    }

    public GameObject getRandomVillager()
    {
        return villagerPrefab[UnityEngine.Random.Range(0, villagerPrefab.Length)];
    }

    public Category(CategoryType type, int buildingCount, float points, int maxPoints, int minPoints, float pointsPerTurn)
    {
        _type = type;
        _buildingCount = buildingCount;
        _points = points;
        _maxPoints = maxPoints;
        _minPoints = minPoints;
        _pointsPerTurn = pointsPerTurn;
    }

    /// <summary>
    /// Adds or removes points to the current total, ensuring the total stays within a specified range.
    /// </summary>
    /// <param name="amount">The amount to add or remove from the points total.</param>
    public void AddRemovePoints(int amount)
    {
        Points += amount;
        Points = Mathf.Clamp(Points, _minPoints, _maxPoints);
    }

    public void AddBuildings(int count)
    {
        StartCoroutine(SpawnBuildingsCoroutine(count));
    }
    private IEnumerator SpawnBuildingsCoroutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (count > 1)
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, .5f));
            else
                yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
            OnBuildingAdded?.Invoke(this);
            CheckAndSpawnVillagers();


        }
    }

    public void RemoveBuildings(int count)
    {
        StartCoroutine(DestroyBuildingsCoroutine(count));

        BuildingCount -= count;
        if (BuildingCount < 0)
        {
            BuildingCount = 0; // Ensure the count doesn't go negative
        }
    }

    private IEnumerator DestroyBuildingsCoroutine(int count)
    {
        for (int i = 0; i < count && constructedBuildingsList.Count > 0; i++)
        {
            int lastIndex = constructedBuildingsList.Count - 1; // Get the index of the last element
            GameObject buildingToRemove = constructedBuildingsList[lastIndex];
            gridManager.grid[gridManager.WorldToGrid(buildingToRemove.transform.position).x, gridManager.WorldToGrid(buildingToRemove.transform.position).y] = null;
            constructedBuildingsList.RemoveAt(lastIndex);
            Instantiate(destroyFX, buildingToRemove.transform.position, Quaternion.identity);

            buildingToRemove.GetComponent<PlayRandomSound>().PlaySoundRandom();
            yield return new WaitForSeconds(0.2f);
            Destroy(buildingToRemove.transform.GetChild(0).gameObject);
            Destroy(buildingToRemove.transform.GetChild(1).gameObject);
            yield return new WaitForSeconds(UnityEngine.Random.Range(.5f, 1f));
            Destroy(buildingToRemove);
        }
    }

    // Method to check and spawn villagers
    private void CheckAndSpawnVillagers()
    {
        // divide points by 3, if villagercount is less than 30, and villagercount is less than points/3 spawn a villager
        if ((Points / 3 > villagerCount && villagerCount < 30) || villagerCount == 0)
        {
            SpawnVillager();
        }

    }

    // Method to spawn a villager
    private void SpawnVillager()
    {
        if (!IsValid()) return;

        GameObject villagers = GameObject.Find("Villagers");
        if (villagers == null)
        {
            Debug.LogError("Could not find GameObject 'Villagers'.");
            return;
        }

        int randomIndex = GetRandomIndex();
        if (villagerPrefab[randomIndex] != null)
        {
            InstantiateVillager(villagers, randomIndex);
            villagerCount++;
        }
        else
        {
            Debug.LogError("Villager prefab is not assigned for category: " + _type.ToString());
        }
    }

    private bool IsValid()
    {
        if (villagerPrefab == null || villagerPrefab.Length == 0)
        {
            return false;
        }

        if (constructedBuildingsList == null || constructedBuildingsList.Count == 0)
        {
            return false;
        }

        return true;
    }

    private int GetRandomIndex()
    {
        return UnityEngine.Random.Range(0, villagerPrefab.Length);
    }

    private void InstantiateVillager(GameObject villagers, int randomIndex)
    {
        Vector3 spawnLocation = constructedBuildingsList[UnityEngine.Random.Range(0, constructedBuildingsList.Count)].transform.position;
        Instantiate(villagerPrefab[randomIndex], spawnLocation, Quaternion.identity, villagers.transform);
    }

    // Placeholder for a method to calculate the villager spawn location
    private Vector3 CalculateVillagerSpawnLocation()
    {
        return new Vector3(UnityEngine.Random.Range(-35, 35), 0, UnityEngine.Random.Range(-35, 35));
    }
}

public enum CategoryType
{
    People,
    Religion,
    Economy,
    Military
}
