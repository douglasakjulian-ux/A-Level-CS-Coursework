using UnityEngine;

public class GravitySource : MonoBehaviour
{
    public Vector2 position => transform.position;
    public float soiRadius;
    public float gravityStrength;

    public float mass;
    float gravityScale;
    public void init()
    {
        MeshScript mesh = this.GetComponent<MeshScript>();
        //soiRadius = (mesh.bodyType == MeshScript.BodyType.Star) ? 50000f : mesh.diameter * 3f;
        soiRadius = (mesh.bodyType == MeshScript.BodyType.GasGiant) ? mesh.diameter * 6f : mesh.diameter * 3f;
        if (mesh.bodyType == MeshScript.BodyType.Star)
        {
            Destroy(this.GetComponent<GravitySource>());
        }
        gravityScale = (mesh.bodyType == MeshScript.BodyType.GasGiant) ? 1.0f : 1.0f;
        //mass = mesh.diameter / 20f;
        mass = Mathf.Pow(mesh.diameter, 2f);
        gravityStrength = mass * gravityScale;
    }

    public Vector2 GetGravity(Vector2 samplePosition)
    {
        Vector2 dir = position - samplePosition;
        float dist = dir.magnitude;
        
        if (dist > soiRadius)
        {
            return Vector2.zero;
        }
        float r = Mathf.Max(dist, this.GetComponent<MeshScript>().diameter * 0.4f);
        float t = dist / soiRadius;
        float compression = Mathf.Pow(1f - t, 3f);

        return dir.normalized * (gravityStrength / (r * r)) * 6f * compression;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, soiRadius);
    }
}
