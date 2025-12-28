using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MeshScript;

public class MapGeneration : MonoBehaviour
{
    public ulong seed;
    void Awake()
    {
        CreateSystem();
    }

    public void CreateSystem()
    {
        GameObject mesh = SystemSettings.mesh;

        SystemData system = SystemGenerator.Generate(seed);
        foreach (var body in system.Bodies)
        {
            GameObject bodyObject = Instantiate(mesh, body.position, Quaternion.identity);
            MeshScript meshScript = bodyObject.GetComponent<MeshScript>();
            meshScript.order = body.order;
            meshScript.diameter = body.diameter;
            meshScript.shadowBehind = body.shadowBehind;
            //hash later:
            //meshScript.noise = 
            //meshScript.amplitude = 
            //meshScript.speed = 
            //meshScript.scale = 

            meshScript.bodyType = body.type;
            meshScript.Generate();
        }
    }
}