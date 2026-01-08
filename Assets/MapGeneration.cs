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
        var currentBody = system.Bodies[0];
        GameObject currentObj = null;
        foreach (var body in system.Bodies)
        {
            GameObject bodyObject = Instantiate(mesh, body.position, Quaternion.identity);
            if (body.moons != 0)
            {
                currentBody = body;
                currentObj = bodyObject;
            }
            else if (currentObj != null)
            {
                if (currentBody.moons == 0)
                {
                    currentBody = null;
                    currentObj = null;
                }
                else
                {
                    bodyObject.transform.parent = currentObj.transform;
                    currentBody.moons--;
                }
            }
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

            bodyObject.GetComponent<GravitySource>().init();
            bodyObject.GetComponent<OrbitalLine>().init();
        }
    }
}