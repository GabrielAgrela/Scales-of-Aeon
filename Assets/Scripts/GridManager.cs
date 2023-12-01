using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridWidth;
    public int gridHeight;
    public float cellSize;
    public Vector3 gridOrigin; // The origin of the grid in world space
    public GameObject[,] grid;

    public GameObject LargeBuilding;
    public GameObject BuildingsParent;
    public LayerMask groundLayer; // Assign this in the inspector to match your ground layer

    public List<Category> categoriesList = new List<Category>();

    void Awake()
    {
        GameObject[] categoryObjects = GameObject.FindGameObjectsWithTag("Category");
        foreach (GameObject categoryObject in categoryObjects)
        {
            Category categoryComponent = categoryObject.GetComponent<Category>();
            if (categoryComponent != null)
            {
                categoriesList.Add(categoryComponent);
                categoryComponent.OnBuildingAdded += HandleBuildingAdded;
            }
            else
            {
                Debug.LogWarning("GameObject tagged as 'Category' does not have a Category component.", categoryObject);
            }
        }
    }


    void Start()
    {
        // Initialize the grid
        grid = new GameObject[gridWidth, gridHeight];
        //GridToWorld(-50,-50);
        // Place a dummy building at the center of the grid
        //PlaceObjectOnGrid(LargeBuilding, gridWidth / 2, gridHeight / 2);
        // StartCoroutine(Spawn());
        //OnDrawGizmos();


    }

    void OnDestroy()
    {
        foreach (var category in categoriesList)
        {
            category.OnBuildingAdded -= HandleBuildingAdded;
        }
    }

    private void HandleBuildingAdded(Category category)
    {
        SpawnBuildingForCategoryCoroutine(LargeBuilding, category);
    }

    // public IEnumerator Spawn()
    // {
    //     foreach (var category in categoriesList.Where(c => c.Type == CategoryType.Military))
    //     {
    //         StartCoroutine(SpawnBuildingForCategoryCoroutine(LargeBuilding, category));
    //         yield return new WaitForSeconds(0.1f);
    //     }
    // }


    // Test routine
    public void SpawnBuildingForCategoryCoroutine(GameObject buildingPrefab, Category category)
    {
        
        SpawnBuildingForCategory(buildingPrefab, category);
    }

    // Convert world position to grid coordinates
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x - gridOrigin.x) / cellSize);
        int y = Mathf.RoundToInt((worldPosition.z - gridOrigin.z) / cellSize); // Using z for 3D space
        return new Vector2Int(x, y);
    }

    // Convert grid coordinates to world position
    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(
            x * cellSize + gridOrigin.x,
            gridOrigin.y,
            y * cellSize + gridOrigin.z
        );
    }


    public void PlaceObjectOnGrid(GameObject obj, int x, int y, Category category)
    {
        if (x >= 0 && y >= 0 && x < gridWidth && y < gridHeight)
        {
            grid[x, y] = obj;

            // Random rotation on Y-axis
            Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            Vector3 tempPosition = GridToWorld(x, y);
            Vector3 groundCheckPosition = tempPosition + new Vector3(0, 5, 0); 
            //get hit point
            RaycastHit hit;
            Physics.Raycast(groundCheckPosition, Vector3.down, out hit, 100f, groundLayer);
            category.unconstructedBuildingsList.Add(Instantiate(obj, new Vector3(tempPosition.x + Random.Range(-.5f, .5f), hit.point.y, tempPosition.z + Random.Range(-.5f, .5f)), rotation, BuildingsParent.transform));
        }
        else
        {
            Debug.LogError("Attempted to place object out of bounds.");
        }
    }


    // Call this method to get the object at a grid position
    public GameObject GetObjectAtGridPosition(int x, int y)
    {
        if (grid != null && x >= 0 && y >= 0 && x < gridWidth && y < gridHeight)
        {
            return grid[x, y];
        }
        else
        {
            Debug.LogError("Attempted to get object out of bounds.");
            return null;
        }
    }

    public Vector2Int GetRandomCellInCategory(Category category, bool filled = true)
    {
        List<Vector2Int> availableCells = new List<Vector2Int>();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsCellInCategoryTerritory(x, y, category) && IsCellOccupied(x, y))
                {
                    availableCells.Add(new Vector2Int(x, y));
                }
            }
        }

        if (availableCells.Count > 0)
        {
            int randomIndex = Random.Range(0, availableCells.Count);
            return availableCells[randomIndex];
        }

        return new Vector2Int(-1, -1); // Return an invalid position if no available cells
    }

    // Convert world position to grid coordinates
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.z / cellSize); // Using z for 3D space
        return new Vector2Int(x, y);
    }

    // Check if the grid cell is occupied
    public bool IsCellOccupied(int x, int y)
    {
        return GetObjectAtGridPosition(x, y) != null;
    }

    // Visualize the grid (optional)
    void OnDrawGizmos()
    {
        if (grid != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var cellCenterWorld = GridToWorld(x, y);
                    Gizmos.color = grid[x, y] != null ? Color.red : Color.green;
                    Gizmos.DrawCube(cellCenterWorld, new Vector3(cellSize, 0.1f, cellSize));
                }
            }
        }
    }

    

    public bool SpawnBuildingForCategory(GameObject buildingPrefab, Category category)
    {
        // Calculate the center of the whole grid
        Vector2Int gridCenter = new Vector2Int(gridWidth / 2, gridHeight / 2);

        // Create a list for available cells within the category's territory
        List<Vector2Int> availableCells = new List<Vector2Int>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Check for ground one unit below the cell
                Vector3 cellPosition = GridToWorld(x, y);
                Vector3 groundCheckPosition = cellPosition + new Vector3(0, 1, 0); // 1 unit above the ground to start the raycast
                bool isGroundBelow = Physics.Raycast(groundCheckPosition, Vector3.down, 100f, groundLayer);

                // Visualize the raycast in the editor
                if (isGroundBelow)
                {
                    Debug.DrawRay(groundCheckPosition, Vector3.down * 1f, Color.green, 5f); // Draws a green line if ground is detected
                }
                else
                {
                    Debug.DrawRay(groundCheckPosition, Vector3.down * 1f, Color.red, 5f); // Draws a red line if no ground is detected
                }

                if (IsCellInCategoryTerritory(x, y, category) && !IsCellOccupied(x, y) && isGroundBelow)
                {
                    availableCells.Add(new Vector2Int(x, y));
                }
            }
        }

        // Shuffle the list to randomize the order
        availableCells = ShuffleList(availableCells);

        // Find the closest available cell to the center
        Vector2Int bestPosition = availableCells.OrderBy(cell => Vector2Int.Distance(cell, gridCenter)).FirstOrDefault();

        // If a position was found, place the building there
        if (availableCells != null && availableCells.Count > 0)
        {
            Vector3 buildPosition = GridToWorld(bestPosition.x, bestPosition.y);
            
            PlaceObjectOnGrid(buildingPrefab, bestPosition.x, bestPosition.y, category);
            return true;
        }
        else
        {
            Debug.LogError($"No available space for category {category.name}.");
            return false;
        }
    }


    // Updated `IsCellInCategoryTerritory` to exclude specific cells for each category
    private bool IsCellInCategoryTerritory(int x, int y, Category category)
    {
        // Determine the center cells based on grid size (assumed to be odd, e.g., 33x33)
        int centerX = gridWidth / 2;
        int centerY = gridHeight / 2;

        // Exclude the specific cells for each category
        if (category.Type == CategoryType.People && (x == centerX || x == centerX - 1) && (y == centerY || y == centerY - 1)) return false;
        if (category.Type == CategoryType.Economy && (x == centerX || x == centerX - 1) && (y == centerY || y == centerY - 1)) return false;
        if (category.Type == CategoryType.Religion && (x == centerX || x == centerX - 1) && (y == centerY || y == centerY - 1)) return false;
        if (category.Type == CategoryType.Military && (x == centerX || x == centerX - 1) && (y == centerY || y == centerY - 1)) return false;

        // Original territory checks
        bool isInHorizontalHalf = (category.Type == CategoryType.People || category.Type == CategoryType.Economy) ? x >= gridWidth / 2 : x < gridWidth / 2;
        bool isInVerticalHalf = (category.Type == CategoryType.People || category.Type == CategoryType.Religion) ? y >= gridHeight / 2 : y < gridHeight / 2;
        return isInHorizontalHalf && isInVerticalHalf;
    }


    //Randomizer de rotação dos edificios
    private List<Vector2Int> ShuffleList(List<Vector2Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector2Int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }

    public void AddBuildingsForCategory(CategoryType categoryType, int amount)
    {
        Category targetCategory = categoriesList.Find(c => c.Type == categoryType);
        for (int i = 0; i < amount; i++)
        {
            if (SpawnBuildingForCategory(LargeBuilding, targetCategory))
            {
                targetCategory.AddBuildings(1);
            }
        }
    }

    // To retrieve the count of buildings for a Category:
    public int GetBuildingCountForCategory(CategoryType categoryType)
    {
        return categoriesList.Find(c => c.Type == categoryType)?.BuildingCount ?? 0;
    }

}
