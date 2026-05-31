using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class BuildManager : MonoBehaviour
{
    public int gridSize = 50;
    bool[,] grid;
    int gridWidth;
    int gridHeight;
    GameObject selectedObj;
    GameObject ghost;
    InputActions inputActions;
    float originX;
    float originY;
    float cellSizeX = 1f;
    float cellSizeY = 1f;
    Vector2 mousePos => Camera.main.ScreenToWorldPoint(inputActions.Player.MousePos.ReadValue<Vector2>());
    void Awake()
    {
        gridWidth = gridSize;
        gridHeight = gridSize;
        originX = -(gridWidth * cellSizeX) / 2f;
        originY = -(gridHeight * cellSizeY) / 2f;
        grid = new bool[gridWidth, gridHeight];
        inputActions = new InputActions();
        inputActions.Enable();
        selectedObj = null;
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = false;
            }
        }
    }

    void Update()
    {
        if (selectedObj != null)
        {
            int cellX = WorldToCellX(mousePos.x);
            int cellY = WorldToCellY(mousePos.y);

            ghost.transform.position = new Vector2(
                originX + cellX * cellSizeX + cellSizeX / 2f,
                originY + cellY * cellSizeY + cellSizeY / 2f
            );

            if (inputActions.Player.LMB.triggered)
            {
                Build((int)cellX, (int)cellY, selectedObj);
            }
        }
    }

    public void Build(int x, int y, GameObject obj)
    {
        if (IsValidBuild(x, y))
        {
            grid[x, y] = true;
            // Instantiate building prefab at the corresponding position
            Vector2 position = new Vector2(
                originX + x * cellSizeX + cellSizeX / 2f,
                originY + y * cellSizeY + cellSizeY / 2f
            ); 
            Instantiate(obj, position, Quaternion.identity);

            selectedObj = null; // Clear selection after building
            Destroy(ghost); // Destroy the ghost object
        }
    }

    bool IsValidBuild(int x, int y)
    {
        Vector2 position = new Vector2(
            originX + x * cellSizeX + cellSizeX / 2f,
            originY + y * cellSizeY + cellSizeY / 2f
        ); 
        int X = WorldToCellX(position.x); X = Mathf.Clamp(X, 0, gridWidth-1);
        int Y = WorldToCellY(position.y); Y = Mathf.Clamp(Y, 0, gridHeight-1);
        bool occupied = false;
        for (x = X; x <= X; x++)
        {
            for (y = Y; y <= Y; y++)
            {
                if (grid[x, y]) { occupied = true; break; } 
            }
            if (occupied) { break; }
        }
        if (!occupied)
        {
            return true; // Valid build

        }
        else
        {
            return false; // Invalid build, space is occupied
        }
    }

    public void Selected(GameObject obj)
    {
        if (selectedObj != null)
        {
            selectedObj = null;
        }
        if (ghost != null)
        {
            Destroy(ghost);
            ghost = null;
        }
        selectedObj = obj;
        ghost = Instantiate(selectedObj, Camera.main.ScreenToWorldPoint(inputActions.Player.MousePos.ReadValue<Vector2>()), Quaternion.identity);
    }

    //int WorldToCellX(float x) => (int)((x) / cellSizeX);
    //int WorldToCellY(float y) => (int)((y) / cellSizeY);

    int WorldToCellX(float x) { return Mathf.FloorToInt((x - originX) / cellSizeX); }
    int WorldToCellY(float y) { return Mathf.FloorToInt((y - originY) / cellSizeY); }
}
