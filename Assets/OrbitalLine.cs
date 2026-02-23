using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitalLine : MonoBehaviour
{
    public int segments;
    public float radius;
    LineRenderer line;
    public Vector2 targetPos;
    Transform parent = null;
    Vector2[] positions;
    bool moon = false;
    Vector2 offset = Vector2.zero;

    //Orbit MUST BE INITIALIZED before this is called
    bool initialized = false;
    GameObject worldRoot;
    public void init()
    {
        worldRoot = GameObject.FindWithTag("WorldRoot");
        moon = false;
        GameObject lineOBJ = Instantiate(Resources.Load<GameObject>("OrbitLine"));
        lineOBJ.transform.SetParent(GameObject.FindWithTag("OrbitLines").transform, false);
        lineOBJ.name = "OrbitLine_Planet_" + gameObject.GetComponent<MeshScript>().order + "_" + GetComponent<Orbit>().barryCenter;
        line = lineOBJ.GetComponent<LineRenderer>();
        line.sortingLayerName = "OrbitLine";
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.white;
        line.endColor = line.startColor;
        line.positionCount = segments;
        positions = new Vector2[segments];
        line.useWorldSpace = false;
        targetPos = GetComponent<Orbit>().barryCenter;
        //targetPos = Vector2.zero;
        if (gameObject.GetComponent<MeshScript>().bodyType == MeshScript.BodyType.Moon)
        {
            lineOBJ.name = "OrbitLine_Moon_" + gameObject.GetComponent<MeshScript>().order;
            //line.transform.SetParent(transform.parent, worldPositionStays: false);
            parent = transform.parent;
            targetPos = parent.position;
            //center = targetPos - (Vector2)parent.position;
            radius = ((Vector2)transform.localPosition).magnitude;
            moon = true;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                positions[i] = new Vector2(x, y);

                line.SetPosition(i, positions[i] + targetPos);
            }
            line.loop = true;
            initialized = true;
            return;
        }
        if (parent == null)
        {
            Debug.Log("OrbitalLine: Parent is null for " + gameObject.name);
        }
        radius = ((Vector2)transform.localPosition).magnitude;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            //line.SetPosition(i, new Vector2(x, y) + targetPos - (Vector2)GetComponent<Orbit>().barryCenter);
            line.SetPosition(i, new Vector2(x, y));
        }
        line.loop = true;
        line.sortingOrder = -1;
        initialized = true;
    }

    float t = 0;
    void Update()
    {
        if (!initialized) { return; }
        line.startWidth = Camera.main.orthographicSize * 0.005f;
        line.endWidth = line.startWidth;

        if (moon)
        {
            targetPos = parent.position;
            if (t >= 0.05f)
            {
                RecalculatePosition();
                t = 0;
            }
            t += Time.deltaTime;
        }
    }

    void RecalculatePosition()
    {
        for (int i = 0; i < segments; i++)
        {
            line.SetPosition(i, positions[i] + (Vector2)transform.parent.position - ((Vector2)worldRoot.transform.position - Vector2.zero));
        }
    }
}
