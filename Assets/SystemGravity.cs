using UnityEngine;

public class SystemGravity : MonoBehaviour
{
    [SerializeField] GravitySource[] sources;
    void Start()
    {
        sources = new GravitySource[GameObject.FindObjectsByType<GravitySource>(FindObjectsSortMode.None).Length];
        sources = GameObject.FindObjectsByType<GravitySource>(FindObjectsSortMode.None);
    }

    public Vector2 GetGravityAt(Vector2 position)
    {
        GravitySource best = null;
        float bestDistance = float.MaxValue;
        foreach (GravitySource source in sources)
        {
            if (source == null) continue;
            float distance = (source.position - position).magnitude;
            if (distance < bestDistance && distance < source.soiRadius)
            {
                bestDistance = distance;
                best = source;
            }
        }
        if (best == null)
            return Vector2.zero;

        return best.GetGravity(position);
    }
}
