using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public static class DensityCalculator
{
    public static int seed;
    public static float size;
    public static float fallOffScale = 2500f;
    public static float rotation = 45f;
    //public float innerFalloff;
    //public float outerFalloff;

    public static void DistributeDensity<T>(List<Quadtree<T>> children, float density)
    {
        var QuadtreeArray = new (float distance, float weight)[children.Count];
        Debug.Log(QuadtreeArray);
        int i = 0;
        float sumWeight = 0;
        foreach (var child in children)
        {
            float distance = DistanceToDoubleSpiral(.5f, child.rect.center, rotation);
            //float weight = 1 / (distance + 1);
            float weight = Mathf.Exp(-distance / fallOffScale);
            QuadtreeArray[i] = (distance, weight);
            i++;
            sumWeight += weight;
        }
        i = 0;
        float sumWeightedDensity = 0f;
        foreach (var child in children)
        {
            float weightedDensity = density * (QuadtreeArray[i].weight / sumWeight);
            //Debug.Log(weightedDensity);
            child.density = weightedDensity;
            i++;
            sumWeightedDensity += weightedDensity;
        }
        Debug.Log($"Dynamic = {children[0].dynamic}, Density = {density}, sumWeightedDensity = {sumWeightedDensity}");
    }

    public static float GetDensity(Vector2 pos)
    {
        //float density = Mathf.Abs(1f - (DistanceToDoubleSpiral(.5f, pos) / (size/4))) * 500000f; -previous density calculation
        float density = Mathf.Exp(-DistanceToDoubleSpiral(.5f, pos, rotation) / fallOffScale);
        float falloff = GetFalloff(pos);
        return density * falloff * 500000f;
    }

    static float GetFalloff(Vector2 pos)
    {
        int hashInput = Mathf.RoundToInt(pos.x * 1000f) * 73856093 ^ Mathf.RoundToInt(pos.y * 1000f) * 19349663;
        float angle = Mathf.Atan2(pos.y, pos.x);
        int ia = Mathf.FloorToInt((angle + Mathf.PI) * 10000f);
        angle = Mathf.Rad2Deg * angle;

        float posMag = pos.magnitude;
        float innerFalloff = (0.8f * size/2f) + (Hash(seed, ia) * (0.2f * size/2f));
        float outerFalloff = innerFalloff + (Hash(seed, ia) * (innerFalloff - (0.8f * size/2f)));
        if (posMag < innerFalloff) { return 1; }
        if (posMag > outerFalloff) { return 0; }

        float t = (posMag - innerFalloff) / (outerFalloff - innerFalloff);
        t *= (posMag - innerFalloff) / (outerFalloff - innerFalloff);
        t *= 0.7f + Hash(seed, ia) * 0.3f;
        return Mathf.Lerp(1f, 0f, t);
    }

    static float Hash(int seed, int v)
    {
        unchecked
        {
            v ^= seed * 374761393;
            v ^= (v << 13);
            v ^= (v >> 17);
            v ^= (v << 5);

            return 1.0f - ((v * (v * v * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f;
        }
    }

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

    public static float DistanceToDoubleSpiral(float a, Vector2 point, float rotation)
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

            // Rotation
            float rx1 = x1 * Mathf.Cos(rotation) - y1 * Mathf.Sin(rotation);
            float ry1 = x1 * Mathf.Sin(rotation) + y1 * Mathf.Cos(rotation);

            float dx1 = rx1 - point.x;
            float dy1 = ry1 - point.y;
            float dist1 = Mathf.Sqrt(dx1 * dx1 + dy1 * dy1);

            // --- Spiral 2 ---
            float r2 = a * Mathf.Exp(k * theta);
            float x2 = r2 * Mathf.Cos(theta + Mathf.PI);
            float y2 = r2 * Mathf.Sin(theta + Mathf.PI);

            // Rotation
            float rx2 = x2 * Mathf.Cos(rotation) - y2 * Mathf.Sin(rotation);
            float ry2 = x2 * Mathf.Sin(rotation) + y2 * Mathf.Cos(rotation);

            float dx2 = rx2 - point.x;
            float dy2 = ry2 - point.y;
            float dist2 = Mathf.Sqrt(dx2 * dx2 + dy2 * dy2);

            // Keep smallest distance
            float dist = Mathf.Min(dist1, dist2);
            if (dist < minDistance)
                minDistance = dist;
        }

        return minDistance;
    }
}
