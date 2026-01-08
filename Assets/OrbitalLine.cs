using TMPro;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitalLine : MonoBehaviour
{
    public int segments;
    public float radius;
    LineRenderer line;
    public Vector2 targetPos;

    public void init()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.white;
        line.endColor = line.startColor;
        line.positionCount = segments;
        targetPos = (transform.parent != null) ? transform.parent.position : Vector2.zero;
        float radius = ((Vector2)transform.position - targetPos).magnitude;
        Vector2 offset = (transform.parent != null) ? targetPos : Vector2.zero;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector2(x, y) + offset);
        }
        line.loop = true;
        line.sortingOrder = -1;
        if (transform.parent != null)
            line.transform.SetParent(transform.parent, worldPositionStays: false);
    }

    void Update()
    {
        if (line == null) { return; }
        line.startWidth = Camera.main.orthographicSize * 0.005f;
        line.endWidth = line.startWidth;
    }
}
