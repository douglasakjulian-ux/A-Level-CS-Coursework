using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Merge : MonoBehaviour
{
    public enum MergeType
    {
        None,
        Fuel
    }

    public int[,] CheckConnections(GameObject[,] objGrid, Vector2Int position)
    {
        int[,] connections = new int[3,3];
        connections = new int[,]
        {
            {
                objGrid[position.x - 1, position.y + 1] != null && objGrid[position.x - 1, position.y + 1].name == this.name ? 1 : 0,
                objGrid[position.x    , position.y + 1] != null && objGrid[position.x, position.y + 1].name == this.name ? 1 : 0,
                objGrid[position.x + 1, position.y + 1] != null && objGrid[position.x + 1, position.y + 1].name == this.name ? 1 : 0,
            },
            {
                objGrid[position.x - 1, position.y] != null && objGrid[position.x - 1, position.y].name == this.name ? 1 : 0,
                0,
                objGrid[position.x + 1, position.y] != null && objGrid[position.x + 1, position.y].name == this.name ? 1 : 0
            },
            {
                objGrid[position.x - 1, position.y - 1] != null && objGrid[position.x - 1, position.y - 1].name == this.name ? 1 : 0,
                objGrid[position.x,     position.y - 1] != null && objGrid[position.x, position.y - 1].name == this.name ? 1 : 0,
                objGrid[position.x + 1, position.y - 1] != null && objGrid[position.x + 1, position.y - 1].name == this.name ? 1 : 0
            }
        };

        Debug.Log(
            connections[0, 0] + " " + connections[0, 1] + " " + connections[0, 2] + "\n" +
            connections[1, 0] + " " + connections[1, 1] + " " + connections[1, 2] + "\n" +
            connections[2, 0] + " " + connections[2, 1] + " " + connections[2, 2]
        );
        return connections;
    }

    public void SetSprite(int spriteIndex)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Building/Modules/FuelMergeSheet");

        foreach (Sprite sprite in sprites)
        {
            if (sprite.name == $"Fuel_merge_{spriteIndex}")
            {
                GetComponent<SpriteRenderer>().sprite = sprite;
                return;
            }
        }

        Debug.LogError($"Couldn't find Fuel_merge_{spriteIndex}");
    }
}
