using System;
using System.Collections.Generic;
using System.IO;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class SaveData : MonoBehaviour
{
    BuildManager buildManager;
    int slotIndex;
    void Awake() 
    {
        buildManager = GetComponent<BuildManager>();
    }

    public enum Type
    {
        Name,
        Ship
    }

    string path = "Assets/Resources/Building/BuildSaveData.txt";
    public void save(string data, Type t, int slot)
    {
        int slotIndex = LookUp(t, slot);
        List<string> lines = new List<string>(File.ReadAllLines(path));

        if (t == Type.Name)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                string text = "Name:" + data;

                lines[slotIndex + 1] = text;

                foreach (string line in lines)
                {
                    sw.WriteLine(line);
                }
            }
        }

        if (t == Type.Ship)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                string text = "Modules:" + data;

                lines[slotIndex + 2] = text;

                foreach (string line in lines)
                {
                    sw.WriteLine(line);
                }
            }
        }
    }

    public List<ModuleData> loadData(Type t, int slot) 
    {
        List<string> lines = new List<string>(File.ReadAllLines(path));
        int index = LookUp(t, slot);

        foreach (string line in lines)
        {
            //if (t == Type.Name && lines[index + 1].Contains("Name:"))
            //{
            //    string name = lines[index + 1].Replace("Name:", "");
            //    //buildManager.LoadName(name);
            //}
            if (t == Type.Ship && lines[slotIndex + 2].Contains("Modules:"))
            {
                string temp = lines[index + 2].Replace("Modules:", "");
                string[] modules = temp.Split("|");
                List<ModuleData> moduleData = new List<ModuleData>();
                foreach (string module in modules)
                {
                    string[] moduleInfo = module.Split(",");
                    ModuleData data = new ModuleData();
                    data.name = moduleInfo[0];
                    data.x = int.Parse(moduleInfo[1]);
                    data.y = int.Parse(moduleInfo[2]);
                    data.rotation = int.Parse(moduleInfo[3]);
                    moduleData.Add(data);
                }
                return moduleData;
            }
        }
        return null;
    }

    public class ModuleData
    {
        public string name;
        public int x;
        public int y;
        public int rotation;
    }

    int LookUp(Type t, int slot)
    {
        //int index = 0;
        string slotSearch = "Save" + slot.ToString() + "-";
        //switch (t)
        //{
        //    case Type.Name:
        //        typeSearch = "Slot:";
        //        break;
        //    case Type.Ship:
        //        typeSearch = "Modules:";
        //        break;
        //}

        using (StreamReader sr = new StreamReader(path))
        {
            string text;
            int lineNumber = 0;
            while ((text = sr.ReadLine()) != null)
            {
                lineNumber++;
                if (text.Contains(slotSearch)) { 
                    slotIndex = lineNumber-1;
                    break;
                }
            }
            sr.Close();
        }
        Debug.Log(slotIndex);
        return slotIndex;
    }
}


