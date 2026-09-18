using UnityEngine;

public class SaveData : MonoBehaviour
{
    public enum Type
    {
        Name,
        Ship
    }

    public void saveData(string data, Type t, int slot)
    {
        if (t == Type.Name)
        {
            //write name to slot
        }

        if (t == Type.Ship)
        {
            //write obj grid positions to slot
        }
    }
}
