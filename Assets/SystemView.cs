using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class SystemView : MonoBehaviour
{
    public ulong seed;
    GameObject systemRoot;
    public GameObject cameraObj;

    public void ClearSystem()
    {
        if (systemRoot != null)
            Destroy(systemRoot);
        systemRoot = Instantiate(new GameObject("SystemRoot"));
    }

    public void DisplaySystem()
    {
        ClearSystem();
        GameObject mesh = SystemSettings.mesh;

        SystemData system = SystemGenerator.Generate(seed);
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
        }
        cameraObj.GetComponent<Camera>().orthographicSize = Mathf.Max(Mathf.Abs(min), Mathf.Abs(max)) * 1.1f;
    }
}
