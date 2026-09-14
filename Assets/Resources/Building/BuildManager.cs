using System.Data;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class BuildManager : MonoBehaviour
{
    public int gridSize = 50;
    public GameObject core;
    bool[,] grid;
    public GameObject[,] objGrid;
    int gridWidth;
    int gridHeight;
    GameObject selectedObj;
    GameObject ghost;
    InputActions inputActions;
    float originX;
    float originY;
    float cellSizeX = 1f;
    float cellSizeY = 1f;
    int rotations = 0;
    bool deleteMode = false;
    Vector2 mousePos => Camera.main.ScreenToWorldPoint(inputActions.Player.MousePos.ReadValue<Vector2>());
    void Awake()
    {
        gridWidth = gridSize;
        gridHeight = gridSize;
        originX = -(gridWidth * cellSizeX) / 2f;
        originY = -(gridHeight * cellSizeY) / 2f;
        grid = new bool[gridWidth, gridHeight];
        objGrid = new GameObject[gridWidth, gridHeight];
        inputActions = new InputActions();
        inputActions.Enable();
        selectedObj = null;
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = false;
                objGrid[x, y] = null;
            }
        }

        Build(gridWidth / 2, gridHeight / 2, core);
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

            ghost.transform.rotation = Quaternion.Euler(0, 0, rotations * 90);

            if (inputActions.Build.Rotate.triggered)
            {
                rotations += 1;
            }

            if (inputActions.Player.LMB.triggered)
            {
                Build((int)cellX, (int)cellY, selectedObj);
            }
        }

        //deselect:
        if (inputActions.Build.Delete.triggered)
        {
            if (selectedObj != null) {
                selectedObj = null;
                Destroy(ghost);
                ghost = null;
            }

            deleteMode = !deleteMode;
        }

        if (deleteMode) {
            int cellX = WorldToCellX(mousePos.x);
            int cellY = WorldToCellY(mousePos.y);

            GameObject selected = null;
            GameObject preSelected = selected;

            if (grid[cellX, cellY] == true)
            {
                if (objGrid[cellX, cellY].tag != "Core" && objGrid[cellX, cellY] != selected)
                {
                    selected = objGrid[cellX, cellY];
                    selected.GetComponent<SpriteRenderer>().color = new Color32(255, 200, 200, 255);
                }
            }
            if (selected != preSelected && preSelected != null)
            {
                preSelected.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
            }

            if (selected != null && inputActions.Player.LMB.triggered)
            {
                grid[cellX, cellY] = false;
                Destroy(selected);
                objGrid[cellX, cellY] = null;
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
            Instantiate(obj, position, Quaternion.Euler(0, 0, rotations * 90));
            objGrid[x, y] = obj;

            //selectedObj = null; // Clear selection after building
            //Destroy(ghost); // Destroy the ghost object
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
        if (deleteMode)
        {
            deleteMode = false;
        }
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
