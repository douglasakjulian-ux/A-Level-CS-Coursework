using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static MeshScript;

public class MapGeneration : MonoBehaviour
{
    public ulong seed;
    GameObject[] bodies;
    Transform worldRoot;

    public bool testing;
    void Awake()
    {
        worldRoot = GameObject.FindWithTag("WorldRoot").transform;
        if (testing == true)
        {
            CreateSystem();
        }
    }

    public void CreateSystem()
    {
        Generate();
        StartCoroutine(WaitForGenerated());
    }

    void Generate()
    {
        StartCoroutine(WaitForGenerated());

        GameObject mesh = SystemSettings.mesh;

        SystemData system = SystemGenerator.Generate(seed);
        var currentBody = system.Bodies[0];
        GameObject currentObj = null;
        bodies = new GameObject[system.Bodies.Count];
        int i = 0;
        foreach (var body in system.Bodies)
        {
            GameObject bodyObject = Instantiate(mesh, body.position, Quaternion.identity);
            bodyObject.transform.parent = worldRoot;
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
            meshScript.orbitSpeed = body.speed;
            meshScript.shadowBehind = body.shadowBehind;

            //hash later:
            //meshScript.noise = 
            //meshScript.amplitude = 
            //meshScript.speed = 
            //meshScript.scale = 

            meshScript.seed = body.seed;
            meshScript.bodyType = body.type;
            meshScript.Generate();
            bodies[i] = bodyObject;
            i++;
        }
    }

    IEnumerator WaitForGenerated()
    {
        if (bodies == null)
            yield break;
        bool allReady;
        do {
            allReady = true;
            foreach (GameObject bodyObject in bodies)
            {
                MeshScript meshScript = bodyObject.GetComponent<MeshScript>();
                if (!meshScript.generated)
                {
                    allReady = false;
                    break;
                }
            }
            if (!allReady)
                yield return null;
        } while (!allReady);
        InitVariables();
    }
    void InitVariables()
    {
        foreach (GameObject bodyObject in bodies)
            bodyObject.GetComponent<GravitySource>().init();

        foreach (GameObject bodyObject in bodies)
        {
            bodyObject.GetComponent<Orbit>().orbitSpeed = bodyObject.GetComponent<MeshScript>().orbitSpeed;
            bodyObject.GetComponent<Orbit>().init();
        }
    }
}