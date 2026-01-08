using UnityEngine;

public class Orbit : MonoBehaviour
{
    [SerializeField] Vector2 barryCenter;
    public float orbitSpeed = 1f;
    Vector2 StartingPos;

    void Awake()
    {
        StartingPos = transform.position;
    }

    float angle = 0f;
    float radius;
    void Start()
    {
        Vector2 dir = StartingPos - barryCenter;
        angle = Mathf.Atan2(dir.x, dir.y);
        barryCenter = BarryCenter();
    }

    void Update()
    {
        if (GetComponent<MeshScript>().bodyType == MeshScript.BodyType.Moon)
            return;

        angle += orbitSpeed * Time.deltaTime;
        Vector2 offset = (transform.parent == null) ? new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Vector2.Distance(StartingPos, barryCenter) : new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Vector2.Distance(StartingPos, (Vector2)transform.parent.position);
        transform.position = (transform.parent == null) ? barryCenter + offset : (Vector2)transform.parent.position + offset;
    }

    Vector2 BarryCenter()
    {
        GameObject star = null;
        GameObject otherStar = null;
        MeshScript[] meshes = FindObjectsByType<MeshScript>(FindObjectsSortMode.None);
        foreach (MeshScript mesh in meshes)
        {
            if (mesh.bodyType == MeshScript.BodyType.Star && mesh.gameObject != star.gameObject)
            {
                otherStar = mesh.gameObject;
            }
            else
            {
                star = mesh.gameObject;
            }
        }
        if (otherStar == null)
        {
            return Vector2.zero;
        }

        float barryCenterDistance = Vector2.Distance(star.transform.position, otherStar.transform.position) * (star.GetComponent<GravitySource>().mass / (star.GetComponent<GravitySource>().mass * otherStar.GetComponent<GravitySource>().mass));
        Vector2 barryCenter = (star.transform.position - otherStar.transform.position).normalized * barryCenterDistance;
        return barryCenter;
    }
}
