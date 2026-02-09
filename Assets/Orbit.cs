using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Vector2 barryCenter;
    public float orbitSpeed = 1f;
    //Vector2 StartingPos => GameObject.FindWithTag("Player").GetComponent<FloatingOrigin>().originOffset;
    Vector2 StartingPos;

    float angle = 0f;
    float radius;
    
    bool initialized = false;
    Vector3 worldRootPos;
    GameObject worldRoot;
    float mDist;
    public void init()
    {
        worldRoot = GameObject.FindWithTag("WorldRoot");
        barryCenter = BarryCenter();
        if (GetComponent<MeshScript>().bodyType != MeshScript.BodyType.Star && GetComponent<MeshScript>().bodyType != MeshScript.BodyType.Moon)
        {
            Vector2 currentPos = transform.position;
            Vector2 radial = currentPos - barryCenter; // vector from barycenter to object
            float dist = radial.magnitude;
            float extra = barryCenter.magnitude; // add this distance to keep objects away from stars

            if (dist > 0.0001f)
            {
                Vector2 newPos = barryCenter + radial.normalized * (dist + extra);
                transform.position = newPos;
            }
            else
            {
                // fallback: nudge out along X if exactly at barycenter
                transform.position = (Vector3)(barryCenter + new Vector2(extra, 0f));
            }
        }

        StartingPos = transform.position;
        mDist = Vector2.Distance(StartingPos, (Vector2)transform.parent.position);
        Vector2 dir = StartingPos - barryCenter;
        angle = Mathf.Atan2(dir.x, dir.y);
        initialized = true;
        if (GetComponent<OrbitalLine>() != null)
            GetComponent<OrbitalLine>().init();
    }

    void Update()
    {
        if (!initialized)
            return;
        //if (GetComponent<MeshScript>().bodyType == MeshScript.BodyType.Moon)
        //    return;

        angle += orbitSpeed * Time.deltaTime;
        if (GetComponent<MeshScript>().bodyType == MeshScript.BodyType.Moon)
        {
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * mDist;
            transform.position = (Vector2)transform.parent.position + offset;
        }
        else
        {
            Vector2 offset = (transform.parent == null) ? new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Vector2.Distance(StartingPos, barryCenter) : new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Vector2.Distance(StartingPos, (Vector2)transform.parent.position);
            transform.position = (transform.parent == null) ? barryCenter + offset : (Vector2)transform.parent.position + offset;
        }

        worldRootPos = worldRoot.transform.position;
    }

    Vector2 BarryCenter()
    {
        GameObject star = null;
        GameObject otherStar = null;
        MeshScript[] meshes = FindObjectsByType<MeshScript>(FindObjectsSortMode.None);
        foreach (MeshScript mesh in meshes)
        {
            if (mesh.bodyType != MeshScript.BodyType.Star)
                continue;
            if (star == null)
            {
                star = mesh.gameObject;
                continue;
            }
            else if (otherStar == null)
            {
                otherStar = mesh.gameObject;
                break;
            }
        }
        if (otherStar == null)
        {
            return Vector2.zero;
        }

        Vector2 barryCenter = (star.transform.position * star.GetComponent<GravitySource>().mass + otherStar.transform.position * otherStar.GetComponent<GravitySource>().mass) / (star.GetComponent<GravitySource>().mass + otherStar.GetComponent<GravitySource>().mass);
        return barryCenter;
    }
    public void originShift(Vector2 delta)
    {
        StartingPos -= delta;
        barryCenter -= delta;

        Material material = GetComponent<MeshRenderer>().material;
        material.SetVector("_FloatingOrigin", worldRootPos);
        GetComponent<MeshRenderer>().material = material;
    }
}