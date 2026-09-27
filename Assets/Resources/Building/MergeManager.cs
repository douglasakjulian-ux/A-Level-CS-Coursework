using UnityEngine;
using System.Collections.Generic;

public class MergeManager : MonoBehaviour
{
    public enum MergeType
    {
        None,
        Fuel
    }

    int[,] none =
    {
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 0, 0, 0 }
    };

    int[,] end =
    {
        { 0, 0, 0 },
        { 1, 0, 0 },
        { 0, 0, 0 }
    };

    int[,] straight =
    {
        { 0, 0, 0 },
        { 1, 0, 1 },
        { 0, 0, 0 }
    };

    int[,] corner =
    {
        { 0, 0, 0 },
        { 1, 0, 0 },
        { 0, 1, 0 }
    };

    int[,] fullCorner = 
    {
        { 0, 0, 0 },
        { 1, 0, 0 },
        { 1, 1, 0 }
    };

    int[,] full = 
    {
        { 1, 1, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 }
    };

    int[,] fullStraight = 
    {
        { 1, 1, 0 },
        { 1, 0, 0 },
        { 1, 1, 0 }
    };

    int[,] tJunction = 
    {
        { 0, 1, 0 },
        { 1, 0, 0 },
        { 0, 1, 0 }
    };

    int[,] tFull = 
    {
        { 0, 1, 1 },
        { 1, 0, 1 },
        { 0, 1, 1 }
    };

    int[,] cornerFull = 
    {
        { 0, 1, 1 },
        { 1, 0, 1 },
        { 0, 0, 0 }
    };

    int[,] cornerFullFlip = 
    {
        { 0, 0, 0 },
        { 1, 0, 1 },
        { 0, 1, 1 }
    };

    public class MergeData
    {
        public Merge merge;
        public MergeType type;
        public Vector2Int position;
    }

    public List<MergeData> mergeObjects = new List<MergeData>();

    public void AddMerge(Merge merge, MergeType type, Vector2Int position)
    {
        mergeObjects.Add(new MergeData { merge = merge, type = type, position = position });
        Debug.Log("position: " + position);
    }

    public void RemoveMerge(Merge merge)
    {
        mergeObjects.RemoveAll(x => x.merge == merge);
    }

    int GetRotation(int[,] connections, int[,] pattern)
    {
        for (int i = 0; i < 4; i++)
        {
            if (Match(connections, pattern))
                return i;

            pattern = Rotate(pattern);
        }

        return -1;
    }

    int[,] Rotate(int[,] matrix)
    {
        int[,] rotated = new int[3, 3];
        rotated = new int[,]
        {
            { matrix[2,0], matrix[1,0], matrix[0,0] },
            { matrix[2,1], 0, matrix[0,1] },
            { matrix[2,2], matrix[1,2], matrix[0,2] }
        };

        return rotated;
    }

    bool Match(int[,] a, int[,] b)
    {
        for (int i = 0; i < a.GetLength(0); i++)
        {
            for (int j = 0; j < a.GetLength(1); j++)
            {
                if (a[i, j] != b[i, j])
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void UpdateMerges()
    {
        foreach (var mergeData in mergeObjects)
        {
            int[,] connections = mergeData.merge.CheckConnections(GetComponent<BuildManager>().objGrid, mergeData.position);

            //full
            if (Match(connections, full) || Match(connections, Rotate(full)) || Match(connections, Rotate(Rotate(full))) || Match(connections, Rotate(Rotate(Rotate(full)))))
            {
                int rotation = GetRotation(connections, end);

                if (rotation != -1)
                {
                    mergeData.merge.SetSprite(4);
                    mergeData.merge.transform.rotation = Quaternion.Euler(0, 0, -rotation * 90);
                }
            }
            // t full
            else if (Match(connections, tFull) || Match(connections, Rotate(tFull)) || Match(connections, Rotate(Rotate(tFull))) || Match(connections, Rotate(Rotate(Rotate(tFull)))))
            {
                int rotation = GetRotation(connections, end);

                if (rotation != -1)
                {
                    mergeData.merge.SetSprite(7);
                    mergeData.merge.transform.rotation = Quaternion.Euler(0, 0, -rotation * 90);
                }
            }
            // full straight
            else if (Match(connections, fullStraight) || Match(connections, Rotate(fullStraight)) || Match(connections, Rotate(Rotate(fullStraight))) || Match(connections, Rotate(Rotate(Rotate(fullStraight)))))
            {
                int rotation = GetRotation(connections, end);

                if (rotation != -1)
                {
                    mergeData.merge.SetSprite(5);
                    mergeData.merge.transform.rotation = Quaternion.Euler(0, 0, -rotation * 90);
                }
            }
            // corner full
            else if (Match(connections, cornerFull) || Match(connections, Rotate(cornerFull)) || Match(connections, Rotate(Rotate(cornerFull))) || Match(connections, Rotate(Rotate(Rotate(cornerFull)))))
            {
                int rotation = GetRotation(connections, end);

                if (rotation != -1)
                {
                    mergeData.merge.SetSprite(9);
                    mergeData.merge.transform.rotation = Quaternion.Euler(0, 0, -rotation * 90);
                }
            }
            // corner full flip
            else if (Match(connections, cornerFullFlip) || Match(connections, Rotate(cornerFullFlip)) || Match(connections, Rotate(Rotate(cornerFullFlip))) || Match(connections, Rotate(Rotate(Rotate(cornerFullFlip)))))
            {
                int rotation = GetRotation(connections, end);

                if (rotation != -1)
                {
                    mergeData.merge.SetSprite(10);
                    mergeData.merge.transform.rotation = Quaternion.Euler(0, 0, -rotation * 90);
                }
            }
            // full corner
            else if (Match(connections, fullCorner) || Match(connections, Rotate(fullCorner)) || Match(connections, Rotate(Rotate(fullCorner))) || Match(connections, Rotate(Rotate(Rotate(fullCorner)))))
            {
                int rotation = GetRotation(connections, end);

                if (rotation != -1)
                {
                    mergeData.merge.SetSprite(3);
                    mergeData.merge.transform.rotation = Quaternion.Euler(0, 0, -rotation * 90);
                }
            }
            // t junction
            else if (Match(connections, tJunction) || Match(connections, Rotate(tJunction)) || Match(connections, Rotate(Rotate(tJunction))) || Match(connections, Rotate(Rotate(Rotate(tJunction)))))
            {
                int rotation = GetRotation(connections, end);

                if (rotation != -1)
                {
                    mergeData.merge.SetSprite(6);
                    mergeData.merge.transform.rotation = Quaternion.Euler(0, 0, -rotation * 90);
                }
            }
            // straight
            else if (Match(connections, straight) || Match(connections, Rotate(straight)) || Match(connections, Rotate(Rotate(straight))) || Match(connections, Rotate(Rotate(Rotate(straight)))))
            {
                int rotation = GetRotation(connections, end);

                if (rotation != -1)
                {
                    mergeData.merge.SetSprite(1);
                    mergeData.merge.transform.rotation = Quaternion.Euler(0, 0, -rotation * 90);
                }
            }
            // corner
            else if (Match(connections, corner) || Match(connections, Rotate(corner)) || Match(connections, Rotate(Rotate(corner))) || Match(connections, Rotate(Rotate(Rotate(corner)))))
            {
                int rotation = GetRotation(connections, end);

                if (rotation != -1)
                {
                    mergeData.merge.SetSprite(2);
                    mergeData.merge.transform.rotation = Quaternion.Euler(0, 0, -rotation * 90);
                }
            }
            // end
            else if (Match(connections, end) || Match(connections, Rotate(end)) || Match(connections, Rotate(Rotate(end))) || Match(connections, Rotate(Rotate(Rotate(end)))))
            {
                int rotation = GetRotation(connections, end);

                if (rotation != -1)
                {
                    mergeData.merge.SetSprite(0);
                    mergeData.merge.transform.rotation = Quaternion.Euler(0, 0, -rotation * 90);
                }
            }
            else
            {
                mergeData.merge.SetSprite(8);
            }
        }
    }
}
