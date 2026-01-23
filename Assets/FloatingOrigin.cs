using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    public Transform player;
    public Transform worldRoot;
    public float threshold = 2000f;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        worldRoot = GameObject.FindWithTag("WorldRoot").transform;
    }

    void LateUpdate()
    {
        if (player.position.magnitude > threshold)
        {
            originShift();
        }
    }

    void originShift()
    {
        Vector3 delta = player.position;
        worldRoot.position -= delta;
        player.position = Vector3.zero;

        foreach (var o in FindObjectsByType<Orbit>(FindObjectsSortMode.None))
            o.originShift((Vector2)delta);
    }
}
