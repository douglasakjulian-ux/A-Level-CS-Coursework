using System.Collections;
using System.ComponentModel;
using Unity.VisualScripting;
using TMPro;
using UnityEngine;

public class SystemView : MonoBehaviour
{
    public ulong seed;
    GameObject systemRoot;
    public GameObject cameraObj;

    void Awake()
    {
        systemRoot = Instantiate(new GameObject("SystemRoot"));
    }

    public void ClearSystem()
    {
        foreach (Transform child in systemRoot.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void DisplaySystem()
    {
        ClearSystem();
        GameObject mesh = SystemSettings.mesh;

        SystemData system = SystemGenerator.Generate(seed);

        int nBodies = 0;
        int nPlanets = 0;
        int nGasGiants = 0;
        int nStars = 0;

        float min = 0f;
        float max = 0f;
        foreach (var body in system.Bodies)
        {
            if (body.position.x/2500f < min) min = body.position.x/2500f;
            if (body.position.y/2500f < min) min = body.position.y/2500f;
            if (body.position.x/2500f > max) max = body.position.x/2500f;
            if (body.position.y/2500f > max) max = body.position.y/2500f;

            GameObject bodyObject = Instantiate(mesh, body.position/2500f, Quaternion.identity);
            bodyObject.transform.parent = systemRoot.transform;
            bodyObject.layer = LayerMask.NameToLayer("SystemView");
            MeshScript meshScript = bodyObject.GetComponent<MeshScript>();
            meshScript.order = body.order;
            meshScript.diameter = body.diameter/1250f;

            //hash later:
            //meshScript.noise = 
            //meshScript.amplitude = 
            //meshScript.speed = 
            //meshScript.scale = 

            meshScript.bodyType = body.type;
            meshScript.Generate();

            switch (body.type)
            {
                case MeshScript.BodyType.Star:
                    nStars++;
                    break;
                case MeshScript.BodyType.Planet:
                    nPlanets++;
                    break;
                case MeshScript.BodyType.GasGiant:
                    nGasGiants++;
                    break;
            }
            if (body.type != MeshScript.BodyType.Moon)
                nBodies++;
        }
        cameraObj.GetComponent<Camera>().orthographicSize = Mathf.Max(Mathf.Abs(min), Mathf.Abs(max)) * 1.1f;

        GameObject systemInfo = GameObject.FindWithTag("SystemInfo");
        TMP_Text infoText = systemInfo.GetComponent<TMP_Text>();
        infoText.richText = true;
        infoText.text = $"<size=60><b>{GameObject.FindWithTag("Galaxy").GetComponent<GalaxyVisualiser>().name}</b></size>\n<size=30>Bodies: {nBodies.ToString()}\nStars: {nStars.ToString()}\nGas giants: {nGasGiants.ToString()}\nPlanets: {nPlanets.ToString()}\n \nSeed: {system.seed}</size>";
    }
}
