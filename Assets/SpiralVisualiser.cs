using UnityEngine;

public class SpiralVisualizer : MonoBehaviour
{
    [Header("Spiral Parameters")]
    public float a = 1f; // Scale factor
    public int segments = 4000; // How many spiral segments to draw
    public float thetaRange = 4 * Mathf.PI; // How far the spiral goes

    [Header("Test Point")]
    public Vector2 testPoint = new Vector2(3f, 2f);
    public float markerSize = 0.1f;

    [Header("Debug")]
    public Color spiralColor = Color.yellow;
    public Color pointColor = Color.red;
    public Color nearestColor = Color.cyan;

    void OnDrawGizmos()
    {
        // Golden ratio
        float phi = (1 + Mathf.Sqrt(5f)) / 2f;
        // Spiral growth rate
        float k = (2f * Mathf.Log(phi)) / Mathf.PI;

        Gizmos.color = spiralColor;

        // Draw the spiral
        Vector3 prev = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float t = Mathf.Lerp(-thetaRange, thetaRange, (float)i / segments);
            float r = a * Mathf.Exp(k * t);
            Vector3 pos = new Vector3(r * Mathf.Cos(t), r * Mathf.Sin(t), 0f);

            if (i > 0)
                Gizmos.DrawLine(prev, pos);

            prev = pos;
        }

        // Draw the test point
        Gizmos.color = pointColor;
        Gizmos.DrawSphere(new Vector3(testPoint.x, testPoint.y, 0f), markerSize);

        // Find nearest point on the spiral
        Vector2 nearest = FindNearestPointOnSpiral(a, k, testPoint, -thetaRange, thetaRange, 0.001f);

        // Draw line to nearest point
        Gizmos.color = nearestColor;
        Gizmos.DrawLine(new Vector3(testPoint.x, testPoint.y, 0f),
                        new Vector3(nearest.x, nearest.y, 0f));

        float distance = Vector2.Distance(testPoint, nearest);
        UnityEditor.Handles.Label(new Vector3(testPoint.x, testPoint.y + 0.2f, 0f),
            $"Distance = {distance:F3}");
    }

    Vector2 FindNearestPointOnSpiral(float a, float k, Vector2 point, float thetaMin, float thetaMax, float step)
    {
        float minDist = float.MaxValue;
        Vector2 nearest = Vector2.zero;

        for (float theta = thetaMin; theta <= thetaMax; theta += step)
        {
            float r = a * Mathf.Exp(k * theta);
            float x = r * Mathf.Cos(theta);
            float y = r * Mathf.Sin(theta);

            float dist = Vector2.Distance(point, new Vector2(x, y));
            if (dist < minDist)
            {
                minDist = dist;
                nearest = new Vector2(x, y);
            }
        }

        return nearest;
    }
}