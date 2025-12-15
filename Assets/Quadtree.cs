using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

public class Quadtree<T>
{
    //int width;
    //int height;

    public bool dynamic;
    public float density;
    int capacity;
    int depth;
    int maxDepth;
    public Rect rect;
    List<(Vector2 position, T data)> points;
    public List<Quadtree<T>> children;
    public List<Rect> rectContainer;
    bool divided;
    bool highestLOD;
    public List<Star> stars = null;
    public Material starMaterial = null;
    public Mesh starMesh = null;
    float cellSizeX;
    float cellSizeY;

    public Quadtree(Rect rect, int capacity, int depth, int maxDepth, bool dynamic)
    {
        this.dynamic = dynamic;
        this.rect = rect;
        this.density = DensityCalculator.GetDensity(rect.center);
        Debug.Log($"Dynamic = {dynamic}, Density = {density}");
        this.depth = depth;
        this.maxDepth = maxDepth;
        this.capacity = capacity;
        //if (depth == 0) { density = 225000000f; }
        //Debug.Log(density);
        //if (depth <= 5)
        //{
        //    this.density = DensityCalculator.GetDensity(rect.center);
        //}
        //else
        //{
        //    Debug.Log(density);
        //}
        //this.density = density;
        this.points = new List<(Vector2 position, T)>();
        this.rectContainer = new List<Rect>();
        this.divided = false;
        this.stars = new List<Star>();
        if (depth < maxDepth && !dynamic)
        {
            Subdivide();
        }
        if (depth == maxDepth) 
        {
            stars = GenerateStars();
        }
    }

    //public Quadtree<T> InsertQuadtree(Vector2 position, int insertDepth)
    //{
    //    //if (!rect.Contains(position)) { return; }
    //    Quadtree<T> toDivide = null;
    //    int goingToDepth = insertDepth;
    //    if (insertDepth <= 0) 
    //    { 
    //        highestLOD = true;
    //        stars = GenerateStars();
    //        return this; 
    //    }
    //    if (!divided) { Subdivide(); }
    //    foreach (var child in children)
    //    {
    //        if (child.rect.Contains(position))
    //        {
    //            toDivide = child;
    //        }
    //        else
    //        {
    //            ClearToMaxDepth(child);
    //        }
    //    }
    //    if (toDivide == null) { return null; }
    //    return toDivide.InsertQuadtree(position, insertDepth - 1);
    //}

    //void ClearToMaxDepth(Quadtree<T> node)
    //{
    //    if (node.children == null || node.children.Count == 0) { return; }
    //    if (node.depth >= maxDepth)
    //    {
    //        node.children.Clear();
    //        node.divided = false;
    //        return;
    //    }

    //    foreach (var child in node.children)
    //    {
    //        ClearToMaxDepth(child);
    //    }
    //}

    public Star NearestStar(Vector2 pos)
    {
        float dist = 0f;
        float closestDist = float.MaxValue;
        Star closestStar = default;
        foreach (Star star in this.stars)
        {
            dist = Vector2.Distance(star.position, pos);
            if (dist < closestDist)
            {
                closestStar = star;
                closestDist = dist;
            }
        }
        return closestStar;
    }

    public Quadtree<T> FindDeepest(Vector2 pos, int depthTo)
    {
        if (!rect.Contains(pos)) { return null; }

        //if (this.depthTo == maxDepth && (stars == null || stars.Count == 0))
        //{
        //    stars = GenerateStars();
        //}

        if (depthTo == 0)
        {
            return this;
        }
        if (!divided)
        {
            Subdivide();
        }
        foreach (var child in children)
        {
            if (child.rect.Contains(pos))
            {
                return child.FindDeepest(pos, depthTo - 1);
            }
        }
        return this;
    }

    void Subdivide()
    {
        if (divided || depth >= maxDepth) { return; }
        divided = true;

        float width = rect.width / 2f;
        float height = rect.height / 2f;

        children = new List<Quadtree<T>>(4);

        children.Add(new Quadtree<T>(new Rect(rect.x, rect.y, width, height), capacity, depth + 1, maxDepth, dynamic));
        children.Add(new Quadtree<T>(new Rect(rect.x + width, rect.y, width, height), capacity, depth + 1, maxDepth, dynamic));
        children.Add(new Quadtree<T>(new Rect(rect.x, rect.y + height, width, height), capacity, depth + 1, maxDepth, dynamic));
        children.Add(new Quadtree<T>(new Rect(rect.x + width, rect.y + height, width, height), capacity, depth + 1, maxDepth, dynamic));

        if (depth >= 5)
        {
            DensityCalculator.DistributeDensity(children, density);
        }

        foreach (var child in children)
        {
            if (child.depth == maxDepth)
            {
                child.stars = child.GenerateStars();
            }
        }
    }

    public void GetRects(List<(Rect rect, float density)> rects)
    {
        if (children == null || children.Count == 0)
        {
            rects.Add((rect, density));
        }
        else
        {
            foreach (var child in children) { child.GetRects(rects); }
        }
    }

    List<Star> GenerateStars()
    {
        int iDensity = (int)density;
        Debug.Log($"Density = {density}, IDensity = {iDensity}, Dynamic = {dynamic}");
        List<Star> stars = new List<Star>(iDensity);
        float Range = rect.xMax - rect.xMin;
        Vector2[] positions = new Vector2[iDensity];
        int gridWidth = 75;
        int gridHeight = 75;
        bool[,] grid = new bool[gridWidth, gridHeight];
        cellSizeX = rect.width / (float)gridWidth;
        cellSizeY = rect.height / (float)gridHeight;
        int amountSkipped = 0;
        for (int i = 0; i < iDensity; i++)
        {
            bool occupied = false;
            Vector2 position;
            //int safety = 0;
            //do
            //{
            //    int safetyIncrement = i + safety * 23963;

            //    position = new Vector2(rect.xMin + (Mathf.Abs(Hash(DensityCalculator.seed, (int)(rect.center.x + rect.center.y) + safetyIncrement) * (rect.xMax - rect.xMin))), rect.yMin + Mathf.Abs(Hash(DensityCalculator.seed, (int)(rect.center.x + rect.center.y) + safetyIncrement + 983573) * (rect.xMax - rect.xMin)));
            //    safety++;
            //    if (safety > 10) { break; }
            //} while (!StarPosCheck(position, positions, i));
            int safetyIncrement = i * 23963;
            position = new Vector2(rect.xMin + (Mathf.Abs(Hash(DensityCalculator.seed, (int)(rect.center.x + rect.center.y) + safetyIncrement) * (rect.xMax - rect.xMin))), rect.yMin + Mathf.Abs(Hash(DensityCalculator.seed, (int)(rect.center.x + rect.center.y) + safetyIncrement + 983573) * (rect.xMax - rect.xMin)));
            ulong seed64 = SeedHash(DensityCalculator.seed, position.x, position.y);
            float radius = ((uHash(seed64, i) * 1000f) + 250) / 2500f;
            int minX = WorldToCellX(position.x - radius); minX = Mathf.Clamp(minX, 0, gridWidth - 1);
            int minY = WorldToCellY(position.y - radius); minY = Mathf.Clamp(minY, 0, gridHeight - 1);
            int maxX = WorldToCellX(position.x + radius); maxX = Mathf.Clamp(maxX, 0, gridWidth - 1);
            int maxY = WorldToCellY(position.y + radius); maxY = Mathf.Clamp(maxY, 0, gridHeight - 1);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (grid[x, y]) { occupied = true; break; }
                }
                if (occupied) { break; }
            }
            if (occupied) { amountSkipped++; continue; }
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    grid[x, y] = true;
                }
            }

            // could assign color here, for now will be plain white
            stars.Add(new Star(i - amountSkipped, position, radius, Color.white, seed64));
            positions[i] = position;
        }
        return stars;
    }
    int WorldToCellX(float x) => (int)((x - rect.xMin) / cellSizeX);
    int WorldToCellY(float y) => (int)((y - rect.yMin) / cellSizeY);

    Mesh StarMesh(List<Star> stars)
    {
        int Amount = stars.Count;
        Vector3[] verticies = new Vector3[Amount * 4];
        int[] triangles = new int[Amount * 6];
        Color[] colors = new Color[Amount * 4];
        Vector2[] uvs = new Vector2[Amount * 4];
        
        for (int i = 0; i < Amount; i++)
        {
            Star star = stars[i];
            int vi = i * 4;
            int ti = i * 6;
            float radius = star.radius;

            verticies[vi + 0] = new Vector3(star.position.x - radius, star.position.y + radius, 0); //tl
            verticies[vi + 1] = new Vector3(star.position.x + radius, star.position.y + radius, 0); //tr
            verticies[vi + 2] = new Vector3(star.position.x + radius, star.position.y - radius, 0); //br
            verticies[vi + 3] = new Vector3(star.position.x - radius, star.position.y - radius, 0); //bl

            triangles[ti + 0] = vi + 0;
            triangles[ti + 1] = vi + 1;
            triangles[ti + 2] = vi + 2;
            triangles[ti + 3] = vi + 2;
            triangles[ti + 4] = vi + 3;
            triangles[ti + 5] = vi + 0;

            uvs[vi + 0] = new Vector2(0, 1);
            uvs[vi + 1] = new Vector2(1, 1);
            uvs[vi + 2] = new Vector2(1, 0);
            uvs[vi + 3] = new Vector2(0, 0);

            colors[vi + 0] = star.color; // temporary value, will change based on seed or prior input
            colors[vi + 1] = star.color;
            colors[vi + 2] = star.color;
            colors[vi + 3] = star.color;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.uv = uvs;
        return mesh;
    }

    float Hash(int seed, int i)
    {
        unchecked
        {
            i ^= seed * 374761393;
            i ^= (i << 13);
            i ^= (i >> 17);
            i ^= (i << 5);

            return 1.0f - ((i * (i * i * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f;
        }
    }
    
    float uHash(ulong seed, int i)
    {
        const ulong BIT_NOISE1 = 0xB5297A4DUL;
        const ulong BIT_NOISE2 = 0x68E31DA4UL;
        const ulong BIT_NOISE3 = 0x1B56C4E9UL;

        ulong mangled = (ulong)i;
        mangled *= BIT_NOISE1;
        mangled += (seed % int.MaxValue);
        mangled ^= (mangled >> 8);
        mangled += BIT_NOISE2;
        mangled ^= (mangled << 8);
        mangled *= BIT_NOISE3;
        mangled ^= (mangled >> 8);
        //ulong raw = mangled;
        //uint unsigned = (uint)raw;
        return (float)(uint)mangled / uint.MaxValue;
    }

    ulong SeedHash(int seed, float fx, float fy)
    {
        unchecked
        {
            long ix = (long)(fx * 1000000f);
            long iy = (long)(fy * 1000000f);

            ulong h = 146527UL;
            h ^= (ulong)ix * 0x9E3779B97F4A7C15UL;
            h ^= (ulong)iy * 0xC2B2AE3D27D4EB4FUL;

            h ^= h >> 33;
            h *= 0xff51afd7ed558ccdUL;
            h ^= h >> 33;
            h *= 0xc4ceb9fe1a85ec53UL;
            h ^= h >> 33;

            return h;

            //ulong ux = (ulong)BitConverter.DoubleToInt64Bits((double)fx);
            //ulong uy = (ulong)BitConverter.DoubleToInt64Bits((double)fy);
            //ulong us = (ulong)(uint)seed;

            //ulong h = 146527 * 0x9E3779B97F4A7C15ul;
            //h ^= ux + 0x9E3779B97F4A7C15ul + (h << 12) + (h >> 4);
            //h ^= uy + 0xC2B2AE3D27D4EB4Ful + (h << 12) + (h >> 4);
            //h ^= us + 0x165667B19E3779F9ul + (h << 12) + (h >> 4);

            //h ^= h >> 30;
            //h *= 0xBF58476D1CE4E5B9ul;
            //h ^= h >> 27;
            //h *= 0x94D049BB133111EBul;
            //h ^= h >> 31;

            //return h;
        }
    }

    public void Draw()
    {
        if (stars != null && stars.Count > 0)
        {
            if (starMesh == null) { starMesh = StarMesh(stars); }
            if (starMaterial == null) { starMaterial = new Material(Resources.Load<Shader>("Galaxy/StarMaterial")); }
            Graphics.DrawMesh(starMesh, Matrix4x4.identity, starMaterial, 0);
        }
    }
}

public struct Star
{
    public Vector2 position;
    public float radius;
    public ulong seed;
    public Color color;
    public int index;

    public Star(int starIndex, Vector2 pos, float r, Color starColor, ulong seed64)
    {
        index = starIndex;
        position = pos;
        radius = r;
        color = starColor;
        seed = seed64;
    }
}

