using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

//[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class MeshScript : MonoBehaviour
{
    public int seed;

    public int resolution = 0;
    public float diameter;
    public float amplitude;
    public float speed;
    public float scale;

    public float orbitSpeed; //temp place to store value, going to make a data object at some point for all ts

    public byte color;

    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Mesh mesh;

    public bool noise;

    public float textureSeed;
    Texture2D texture;
    public bool generated;
    public Texture2D gradientTex;

    public bool shadowBehind;

    public int order;

    private void Awake()
    {
        resolution = SystemSettings.resolution;

        meshFilter = GetComponent<MeshFilter>();
        texture = new Texture2D(resolution, resolution);
        meshRenderer = GetComponent<MeshRenderer>();
        generated = false;

        if (bodyType == BodyType.Star || bodyType == BodyType.GasGiant)
            noise = true;
        else
            noise = false;
    }

    public enum BiomeType
    {
        EarthLike,
        Desert,
        Volcano,
        Rock,
    }
    public BiomeType biomeType;

    public enum BodyType
    {
        GasGiant,
        Planet,
        Asteroid,
        Moon,
        Star,
    }
    public BodyType bodyType;
    public float type;

    public void Generate()
    {
        meshFilter = GetComponent<MeshFilter>();
        vertices = new Vector3[resolution + 1];
        uv = new Vector2[vertices.Length];
        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(0.5f, 0.5f);
        triangles = new int[resolution * 3];
        Shader shader;
        Material material;
        Color color1;
        Color color2;
        Color color3;
        textureSeed = hash(seed, order + 1, 1000000);
        //test = hash(seed, (int)textureSeed, 10000)/ 10000f * 10f;
        switch (bodyType)
        {
            case BodyType.GasGiant:
                shader = Resources.Load<Shader>("Shaders/GasGiant");
                material = new Material(shader);
                //textureSeed = hash(seed, order + 1, 10000);
                material.SetColor("_BackColor", new Color32((byte)(hash(seed, (int)textureSeed, 255)), (byte)(hash(seed, (int)textureSeed + 1, 255)), (byte)(hash(seed, (int)textureSeed + 2, 255)), 255));
                material.SetColor("_FrontColor", new Color32((byte)(hash(seed, (int)textureSeed*2, 255)), (byte)(hash(seed, (int)textureSeed*2 + 1, 255)), (byte)(hash(seed, (int)textureSeed*2 + 2, 255)), 255));
                material.SetFloat("_Speed", (hash(seed, (int)textureSeed, 1000) / 1000f * 0.05f) + 0.01f);
                material.SetFloat("_xScale", hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f);
                material.SetFloat("_Scale", (hash(seed, (int)textureSeed, 1000) / 1000f * 20f) + 5f);
                material.SetFloat("_Range", hash(seed, (int)textureSeed, 1000) / 1000f * 1.4f);
                material.SetInt("_ShadowBehind", shadowBehind ? 1 : 0);
                material.SetFloat("_ShadowIntensity", shadowBehind ? (hash(seed, (int)textureSeed, 1000)/ 1000f * 0.5f) + 1f : (hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f) + .25f);
                meshRenderer.material = material;
                break;
            case BodyType.Planet:
                //choosing biome
                type = hash(seed, (int)textureSeed, 100);
                if (type >= 70) { biomeType = BiomeType.Volcano; }
                else if (type >= 50) { biomeType = BiomeType.EarthLike; }
                else if (type >= 30) { biomeType = BiomeType.Desert; }
                else { biomeType = BiomeType.Rock; }
                switch (biomeType)
                {
                    case BiomeType.EarthLike:
                        shader = Resources.Load<Shader>("Shaders/PlanetShader");
                        material = new Material(shader);
                        //textureSeed = hash(seed, order + 1, 10000);
                        color1 = new Color32((byte)(hash(seed, (int)textureSeed, 255)), (byte)(hash(seed, (int)textureSeed + 1, 255)), (byte)(hash(seed, (int)textureSeed + 2, 255)), 255);
                        color2 = new Color32((byte)(hash(seed, (int)textureSeed + 3, 255)), (byte)(hash(seed, (int)textureSeed + 4, 255)), (byte)(hash(seed, (int)textureSeed + 5, 255)), 255);
                        color3 = new Color32((byte)(hash(seed, (int)textureSeed + 6, 255)), (byte)(hash(seed, (int)textureSeed + 7, 255)), (byte)(hash(seed, (int)textureSeed + 8, 255)), 255);
                        gradientTex = gradientTexture(color1, color2, color3);
                        material.SetTexture("_GradientTexture", gradientTex);
                        material.SetFloat("_heightScale", (hash(seed, (int)textureSeed, 1000) / 1000f * 25f) + 10);
                        material.SetFloat("_seaLevel", (hash(seed, (int)textureSeed, 1000) / 1000f * 0.8f) + 0.2f);
                        material.SetFloat("_airScale", hash(seed, (int)textureSeed, 1000) / 1000f * 25f);
                        material.SetFloat("_airSpeed", hash(seed, (int)textureSeed, 1000) / 1000f * 0.2f);
                        material.SetVector("_heightSeed", new Vector2(hash(seed, (int)textureSeed, 10000), hash(seed, (int)textureSeed, 10000)));
                        material.SetInt("_ShadowBehind", shadowBehind ? 1 : 0);
                        material.SetFloat("_ShadowIntensity", shadowBehind ? (hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f) + 1f : (hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f) + .25f);
                        meshRenderer.material = material;
                        break;
                    case BiomeType.Rock:
                        shader = Resources.Load<Shader>("Shaders/RockPlanetShader");
                        material = new Material(shader);
                        color1 = new Color32((byte)(hash(seed, (int)textureSeed, 255)), (byte)(hash(seed, (int)textureSeed + 1, 255)), (byte)(hash(seed, (int)textureSeed + 2, 255)), 255);
                        color2 = new Color32((byte)(hash(seed, (int)textureSeed + 6, 255)), (byte)(hash(seed, (int)textureSeed + 7, 255)), (byte)(hash(seed, (int)textureSeed + 8, 255)), 255);
                        gradientTex = gradientTexture2(color1, color2);
                        material.SetTexture("_GradientTexture", gradientTex);
                        material.SetFloat("_heightScale", (hash(seed, (int)textureSeed, 1000) / 1000f * 30f) + 10);
                        material.SetVector("_heightSeed", new Vector2(hash(seed, (int)textureSeed, 10000), hash(seed, (int)textureSeed, 10000)));
                        material.SetInt("_ShadowBehind", shadowBehind ? 1 : 0);
                        material.SetFloat("_ShadowIntensity", shadowBehind ? (hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f) + 1f : (hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f) + .25f);
                        meshRenderer.material = material;
                        break;
                    case BiomeType.Desert:
                        shader = Resources.Load<Shader>("Shaders/DesertPlanetShader");
                        material = new Material(shader);
                        color1 = new Color32((byte)(hash(seed, (int)textureSeed, 255)), (byte)(hash(seed, (int)textureSeed + 1, 255)), (byte)(hash(seed, (int)textureSeed + 2, 255)), 255);
                        color2 = color1 * 0.8f;
                        gradientTex = gradientTexture2(color1, color2);
                        material.SetTexture("_GradientTexture", gradientTex);
                        material.SetFloat("_heightScale", (hash(seed, (int)textureSeed, 1000) / 1000f * 15f) + 10);
                        material.SetVector("_heightSeed", new Vector2(hash(seed, (int)textureSeed, 10000), hash(seed, (int)textureSeed, 10000)));
                        material.SetInt("_ShadowBehind", shadowBehind ? 1 : 0);
                        material.SetFloat("_ShadowIntensity", shadowBehind ? (hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f) + 1f : (hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f) + .25f);
                        meshRenderer.material = material;
                        break;
                    case BiomeType.Volcano:
                        shader = Resources.Load<Shader>("Shaders/VolcanoPlanetShader");
                        material = new Material(shader);
                        color2 = new Color32((byte)(hash(seed, (int)textureSeed, 255)), (byte)(hash(seed, (int)textureSeed + 1, 255)), (byte)(hash(seed, (int)textureSeed + 2, 255)), 255);
                        color1 = Color.red;
                        gradientTex = gradientTextureVol(color1, color2);
                        material.SetTexture("_GradientTexture", gradientTex);
                        material.SetFloat("_heightScale", (hash(seed, (int)textureSeed, 1000) / 1000f * 25f) + 10);
                        float seaLevel = hash(seed, (int)textureSeed, 1000) / 1000f * 0.4f;
                        if (seaLevel > 0.2f) { seaLevel *= 2.5f; }
                        material.SetFloat("_seaLevel", seaLevel);
                        material.SetFloat("_airScale", hash(seed, (int)textureSeed, 1000) / 1000f * 25f);
                        material.SetFloat("_airSpeed", hash(seed, (int)textureSeed, 1000) / 1000f * 0.1f);
                        material.SetVector("_heightSeed", new Vector2(hash(seed, (int)textureSeed, 10000), hash(seed, (int)textureSeed, 10000)));
                        material.SetInt("_ShadowBehind", shadowBehind ? 1 : 0);
                        material.SetFloat("_ShadowIntensity", shadowBehind ? (hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f) + 1f : (hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f) + .25f);
                        meshRenderer.material = material;
                        break;
                }
                break;
            case BodyType.Asteroid:
                shader = Resources.Load<Shader>("Shaders/MoonShader");
                material = new Material(shader);
                material.SetVector("_heightSeed", new Vector2(hash(seed, (int)textureSeed, 10000), hash(seed, (int)textureSeed, 10000)));
                material.SetFloat("_heightScale", (hash(seed, (int)textureSeed, 1000) / 1000f * 30f) + 10);
                material.SetInt("_ShadowBehind", shadowBehind ? 1 : 0);
                material.SetFloat("_ShadowIntensity", shadowBehind ? (hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f) + 1f : (hash(seed, (int)textureSeed, 1000) / 1000f * 0.4f) + .35f);
                meshRenderer.material = material;
                break;
            case BodyType.Moon:
                shader = Resources.Load<Shader>("Shaders/MoonShader");
                material = new Material(shader);
                material.SetVector("_heightSeed", new Vector2(hash(seed, (int)textureSeed, 10000), hash(seed, (int)textureSeed, 10000)));
                material.SetFloat("_heightScale", (hash(seed, (int)textureSeed, 1000) / 1000f * 30f) + 10);
                material.SetInt("_ShadowBehind", shadowBehind ? 1 : 0);
                material.SetFloat("_ShadowIntensity", shadowBehind ? (hash(seed, (int)textureSeed, 1000) / 1000f * 0.5f) + 1f : (hash(seed, (int)textureSeed, 1000) / 1000f * 0.4f) + .35f);
                meshRenderer.material = material;
                break;
            case BodyType.Star:
                for (int x = 0; x < resolution; x++)
                {
                    for (int y = 0; y < resolution; y++)
                    {
                        int center = resolution / 2;
                        int circleSize = (int)(resolution * 0.6f) / 2;
                        //texture.SetPixel(x, y, new Color32((byte)(255f * Mathf.Clamp(hash(seed, x + y, resolution), 0.75f, 1f)), (byte)(255f * Mathf.Clamp(hash(seed, x + y, resolution), 0.75f, 1f)), (byte)0f, (byte)(255f * Mathf.Clamp(hash(seed, x + y, resolution), 0.75f, 1f))));

                        if ((x - center) * (x - center) + (y - center) * (y - center) <= circleSize * circleSize)
                        {
                            texture.SetPixel(x, y, new Color32(200, 200, 0, 255));
                        }
                        else
                        {
                            texture.SetPixel(x, y, new Color32(255, 255, 0, 255));
                        }
                    }
                }
                texture.filterMode = FilterMode.Point;
                texture.Apply();
                meshRenderer.material.mainTexture = texture;
                break;
        }

        //SQUARE TEXTURE FOR TESTING (PASTE INTO AREA TO TEST)
        //if (x <= resolution * 0.5f && y <= resolution * 0.5f)
        //{
        //    texture.SetPixel(x, y, new Color(0, 0, 0, 255));
        //}
        //else
        //{
        //    texture.SetPixel(x, y, new Color32(255, 255, 0, 255));
        //}

        mesh = new Mesh();

        bool asteroid = false;
        //float offset = diameter;
        //float lerpLength = resolution - ((360f / (float)resolution) * 95f);
        if (bodyType == BodyType.Asteroid) { asteroid = true; }
        for (int i = 0; i < resolution; i++)
        {
            float k = (2 * Mathf.PI * i) / resolution;

            //offset = (asteroid) ? offset + ((hash(seed, i, 1000) / 750f) - 0.75f) / 5f : diameter;
            //float noise = 0f;
            //noise += Mathf.PerlinNoise(Mathf.Cos(k) * scale + (int)hash(seed, i, 1000000000), Mathf.Sin(k) * scale + (int)hash(seed, i, 1000000000));
            //noise += Mathf.PerlinNoise(Mathf.Cos(k) * scale * 2 + (int)hash(seed, i, 1000000000), Mathf.Sin(k) * scale * 2 + (int)hash(seed, i, 1000000000)) * 0.5f;
            //noise += Mathf.PerlinNoise(Mathf.Cos(k) * scale * 4 + (int)hash(seed, i, 1000000000), Mathf.Sin(k) * scale * 4 + (int)hash(seed, i, 1000000000)) * 0.25f;

            //noise /= 1.75f;
            //offset = (asteroid) ? offset + (noise - 0.5f) * amplitude : diameter;
            float baseNoise = Mathf.PerlinNoise(i * 0.08f + seed, seed);
            float jaggedNoise = Mathf.PerlinNoise(i * 0.6f + seed * 2, seed * 2);

            float radius = (asteroid) ? diameter + (baseNoise - 0.5f) * amplitude : diameter;
            radius = Mathf.Round(radius * 5f) / 5f;
            radius += (asteroid) ? (jaggedNoise - 0.5f) * amplitude * 0.35f : 0f;

            vertices[i + 1] = new Vector3(Mathf.Cos(k) * (radius), Mathf.Sin(k) * (radius), 0);

            uv[i + 1] = new Vector2(Mathf.Cos(k) * 0.5f + 0.5f, Mathf.Sin(k) * 0.5f + 0.5f);

            //offset = diameter;

            //if (i >= (360f / (float)resolution) * 95f)
            //{
            //    //offset = Mathf.Lerp(offset, diameter, (i - lerpLength) / (resolution - lerpLength));
            //    offset = Mathf.Lerp(offset, diameter, (float)(i - (resolution - lerpLength)) / (float)lerpLength);
            //}
        }

        for (int i = 0; i < resolution; i++)
        {
            int n = i * 3;
            triangles[n] = 0;
            triangles[n + 1] = i + 1;
            triangles[n + 2] = (i + 1) % resolution + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        generated = true;
    }

    private void Update()
    {
        if (noise && generated)
        {
            for (int i = 0; i < resolution; i++)
            {
                float k = (2 * Mathf.PI * i) / resolution;
                float noise = 0f;

                //octaves
                noise += Mathf.PerlinNoise(Mathf.Cos(k) * scale + (Time.time * speed), Mathf.Sin(k) * scale + (Time.time * speed));
                noise += Mathf.PerlinNoise(Mathf.Cos(k) * scale + (Time.time * speed), Mathf.Sin(k) * scale + (Time.time * speed)) * 1.5f;
                noise += Mathf.PerlinNoise(Mathf.Cos(k) * scale + (Time.time * speed), Mathf.Sin(k) * scale + (Time.time * speed)) * 2;
                //average of noises
                noise /= 3;

                //calculating radius and x,y coordinates
                float radius = diameter + noise * amplitude;
                float x = radius * Mathf.Cos(k);
                float y = radius * Mathf.Sin(k);
                vertices[i + 1] = new Vector3(x, y, 0);
            }
            for (int i = 0; i < resolution; i++)
            {
                int n = i * 3;
                triangles[n] = 0;
                triangles[n + 1] = i + 1;
                triangles[n + 2] = (i + 1) % resolution + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;

            //if (bodyType == BodyType.GasGiant)
            //{
            //    for (int x = 0; x < resolution; x++)
            //    {
            //        for (int y = 0; y < resolution; y++)
            //        {
            //            float noise = Mathf.PerlinNoise(textureSeed + x * 0.025f + Time.time * 0.1f * 2, textureSeed + y * 0.025f + Time.time * 0.1f);
            //            byte color = (byte)(noise * 255f);
            //            texture.SetPixel(x, y, new Color32(color, color, color, 255));
            //        }
            //    }

            //    texture.filterMode = FilterMode.Point;
            //    texture.Apply();
            //    meshRenderer.material.mainTexture = texture;
            //}
        }
    }
    //int hash(int seed, int i, int mod)
    //{
    //    uint change = (uint)(seed * 374761393 + i * 668265263);
    //    change = (change ^ (change >> 13)) * 1274126177;
    //    change ^= change >> 16;
    //    return (int)(change % mod);
    //}
    float hash(int seed, int x, int mod)
    {
        //const int BIT_NOISE1 = 0x68E31DA4;
        const int BIT_NOISE1 = 1759714724;
        //const int BIT_NOISE2 = 0x75297A4D;
        const int BIT_NOISE2 = 1965652557;
        //const int BIT_NOISE3 = 0x1B56C4E9;
        const int BIT_NOISE3 = 458671337;

        int mangled = x;
        mangled *= BIT_NOISE1;
        mangled += seed;
        mangled ^= (mangled >> 8);
        mangled += BIT_NOISE2;
        mangled ^= (mangled << 8);
        mangled *= BIT_NOISE3;
        mangled ^= (mangled >> 8);
        int raw = mangled;
        uint unsigned = (uint)raw;
        //return (float)unsigned / uint.MaxValue * mod;
        return (float)unsigned % mod;
    }

    Texture2D gradientTexture(Color c1, Color c2, Color c3)
    {
        //making the gradient
        var gradient = new Gradient();
        var alpha = new GradientAlphaKey[2];
        alpha[0] = new GradientAlphaKey(0.0f, 1.0f);
        alpha[1] = new GradientAlphaKey(1.0f, 1.0f);
        var colors = new GradientColorKey[6];
        colors[0] = new GradientColorKey(c1 / 1.5f, 0.0f);
        colors[1] = new GradientColorKey(c1, 0.45f);
        colors[2] = new GradientColorKey(c2, 0.5f);
        colors[3] = new GradientColorKey(c3, 0.55f);
        colors[4] = new GradientColorKey(c3 / 1.5f, 0.9f);
        colors[5] = new GradientColorKey(c3 / 2f, 1.0f);
        gradient.SetKeys(colors, alpha);

        Texture2D texture = new Texture2D(128, 1, TextureFormat.RGBA32, false);
        texture.anisoLevel = 0;
        for (int i = 0; i < texture.width; i++)
        {
            texture.SetPixel(i, 0, gradient.Evaluate((float)i / (texture.width - 1f)));
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();

        return texture;
    }

    Texture2D gradientTexture2(Color c1, Color c3)
    {
        //making the gradient
        var gradient = new Gradient();
        var alpha = new GradientAlphaKey[2];
        alpha[0] = new GradientAlphaKey(0.0f, 1.0f);
        alpha[1] = new GradientAlphaKey(1.0f, 1.0f);
        var colors = new GradientColorKey[5];
        colors[0] = new GradientColorKey(c1 / 1.5f, 0.0f);
        colors[1] = new GradientColorKey(c1, 0.45f);
        colors[2] = new GradientColorKey(c3, 0.55f);
        colors[3] = new GradientColorKey(c3 / 1.5f, 0.9f);
        colors[4] = new GradientColorKey(c3 / 2f, 1.0f);
        gradient.SetKeys(colors, alpha);

        Texture2D texture = new Texture2D(128, 1, TextureFormat.RGBA32, false);
        texture.anisoLevel = 0;
        for (int i = 0; i < texture.width; i++)
        {
            texture.SetPixel(i, 0, gradient.Evaluate((float)i / (texture.width - 1f)));
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();

        return texture;
    }

    Texture2D gradientTextureVol(Color c1, Color c3)
    {
        //making the gradient
        var gradient = new Gradient();
        var alpha = new GradientAlphaKey[2];
        alpha[0] = new GradientAlphaKey(0.0f, 1.0f);
        alpha[1] = new GradientAlphaKey(1.0f, 1.0f);
        var colors = new GradientColorKey[5];
        colors[0] = new GradientColorKey(c1 / 1.5f, 0.0f);
        colors[1] = new GradientColorKey(c1, 0.05f);
        colors[2] = new GradientColorKey(c3, 0.1f);
        colors[3] = new GradientColorKey(c3 / 1.5f, 0.75f);
        colors[4] = new GradientColorKey(c3 / 2f, 1.0f);
        gradient.SetKeys(colors, alpha);

        Texture2D texture = new Texture2D(128, 1, TextureFormat.RGBA32, false);
        texture.anisoLevel = 0;
        for (int i = 0; i < texture.width; i++)
        {
            texture.SetPixel(i, 0, gradient.Evaluate((float)i / (texture.width - 1f)));
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();

        return texture;
    }
}
