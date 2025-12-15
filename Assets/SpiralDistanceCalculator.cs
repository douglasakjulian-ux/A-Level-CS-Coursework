using System;
using UnityEngine;

public static class SpiralDistanceCalculator
{
    // Compute the minimum distance between a point (x0, y0) and the spiral
    public static float DistanceToSpiral(float a, Vector2 point)
    {
        // Golden ratio
        float phi = (1 + Mathf.Sqrt(5f)) / 2f;

        // k = (2 * ln(phi)) / π
        float k = (2f * Mathf.Log(phi)) / Mathf.PI;

        float minDistance = float.MaxValue;

        // Scan through θ values
        // (cover several turns to catch nearest part of spiral)
        for (float theta = -4 * Mathf.PI; theta <= 4 * Mathf.PI; theta += 0.001f)
        {
            float r = a * Mathf.Exp(k * theta);
            float x = r * Mathf.Cos(theta);
            float y = r * Mathf.Sin(theta);

            float dx = x - point.x;
            float dy = y - point.y;

            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            if (dist < minDistance)
                minDistance = dist;
        }

        return minDistance;
    }

    /// <summary>
    /// Computes the minimum distance between a point and a *double golden spiral*
    /// (two golden spirals rotated 180° apart).
    /// </summary>
    public static float DistanceToDoubleSpiral(float a, Vector2 point)
    {
        float phi = (1 + Mathf.Sqrt(5f)) / 2f;
        float k = (2f * Mathf.Log(phi)) / Mathf.PI;

        float minDistance = float.MaxValue;

        // Scan through θ values across multiple rotations
        for (float theta = -15 * Mathf.PI; theta <= 15 * Mathf.PI; theta += 0.01f)
        {
            // --- Spiral 1 ---
            float r1 = a * Mathf.Exp(k * theta);
            float x1 = r1 * Mathf.Cos(theta);
            float y1 = r1 * Mathf.Sin(theta);

            float dx1 = x1 - point.x;
            float dy1 = y1 - point.y;
            float dist1 = Mathf.Sqrt(dx1 * dx1 + dy1 * dy1);

            // --- Spiral 2 ---
            float r2 = a * Mathf.Exp(k * theta);
            float x2 = r2 * Mathf.Cos(theta + Mathf.PI);
            float y2 = r2 * Mathf.Sin(theta + Mathf.PI);

            float dx2 = x2 - point.x;
            float dy2 = y2 - point.y;
            float dist2 = Mathf.Sqrt(dx2 * dx2 + dy2 * dy2);

            // Keep smallest distance
            float dist = Mathf.Min(dist1, dist2);
            if (dist < minDistance)
                minDistance = dist;
        }

        return minDistance;
    }
}