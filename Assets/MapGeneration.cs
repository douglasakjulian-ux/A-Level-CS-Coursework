using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static MeshScript;

public class MapGeneration : MonoBehaviour
{
    public ulong seed;
    GameObject[] bodies;
    Transform worldRoot;

    public bool testing;
    void Awake()
    {
        worldRoot = GameObject.FindWithTag("WorldRoot").transform;
        if (testing == true)
        {
            CreateSystem();
        }
    }

    public void CreateSystem()
    {
        Generate();
        StartCoroutine(WaitForGenerated());
    }

    void Generate()
    {
        StartCoroutine(WaitForGenerated());

        GameObject mesh = SystemSettings.mesh;

        SystemData system = SystemGenerator.Generate(seed);
        var currentBody = system.Bodies[0];
        GameObject currentObj = null;
        bodies = new GameObject[system.Bodies.Count];
        int i = 0;
        foreach (var body in system.Bodies)
        {
            if (body.belt != null)
            {
                var a = 0;
                foreach (var asteroidPos in body.belt.positions)
                {
                    GameObject asteroid = new GameObject("Asteroid");
                    asteroid.transform.position = asteroidPos;
                    SpriteRenderer sr = asteroid.AddComponent<SpriteRenderer>();

                    asteroid.transform.parent = worldRoot;

                    int size = 64; // or 64 for more detail
                    Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

                    Color[] pixels = new Color[size * size];

                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = (x - size / 2f) / (size / 2f);
                            float dy = (y - size / 2f) / (size / 2f);
                            float dist = dx * dx + dy * dy;

                            float angle = Mathf.Atan2(dy, dx);
                            float nx = Mathf.Cos(angle);
                            float ny = Mathf.Sin(angle);

                            float radialNoise = Mathf.PerlinNoise(
                                nx * 2f + body.belt.seeds[a],
                                ny * 2f + body.belt.seeds[a]
                            );
                            float edge = 0.7f + (radialNoise - 0.5f) * 0.4f;
                            if (Mathf.Sqrt(dist) <= edge)
                            {
                                pixels[y * size + x] = Color.white;
                            }
                            else
                            {
                                pixels[y * size + x] = Color.clear;
                            }
                        }
                    }

                    tex.SetPixels(pixels);
                    tex.filterMode = FilterMode.Point; // IMPORTANT for pixel look
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.Apply();
                    //SpriteRenderer sr = asteroid.GetComponent<SpriteRenderer>();

                    sr.sprite = Sprite.Create(
                        tex,
                        new Rect(0, 0, size, size),
                        new Vector2(0.5f, 0.5f),
                        8f // pixels per unit
                    );

                    bool shadowBehind = body.belt.shadowBehind[a];
                    Shader shader = Resources.Load<Shader>("Shaders/AsteroidShader");
                    Material material = new Material(shader);
                    material.SetVector("_heightSeed", new Vector2((float)(body.belt.seeds[a] % 1000) / 1000f, (float)(body.belt.seeds[a] % 1000) / 1000f));
                    material.SetFloat("_heightScale", ((float)(body.belt.seeds[a] % 1000) / 1000f * 30f) + 10);
                    material.SetInt("_ShadowBehind", shadowBehind ? 1 : 0);
                    material.SetFloat("_ShadowIntensity", shadowBehind ? ((float)(body.belt.seeds[a] % 1000) / 1000f * 0.5f) + 1f : ((float)(body.belt.seeds[a] % 1000) / 1000f * 0.4f) + .35f);
                    asteroid.GetComponent<SpriteRenderer>().material = material;
                    
                    asteroid.tag = "Asteroid";
                    asteroid.AddComponent<PolygonCollider2D>();

                    a++;
                }
            }
            else
            {
                GameObject bodyObject = Instantiate(mesh, body.position, Quaternion.identity);
                if (bodyObject == null)
                    continue;
                bodyObject.transform.parent = worldRoot;
                if (body.moons != 0)
                {
                    currentBody = body;
                    currentObj = bodyObject;
                }
                else if (currentObj != null)
                {
                    if (currentBody.moons == 0)
                    {
                        currentBody = null;
                        currentObj = null;
                    }
                    else
                    {
                        bodyObject.transform.parent = currentObj.transform;
                        currentBody.moons--;
                    }
                }
                MeshScript meshScript = bodyObject.GetComponent<MeshScript>();
                meshScript.order = body.order;
                meshScript.diameter = body.diameter;
                meshScript.orbitSpeed = body.speed;
                meshScript.shadowBehind = body.shadowBehind;

                //hash later:
                //meshScript.noise = 
                //meshScript.amplitude = 
                //meshScript.speed = 
                //meshScript.scale = 

                meshScript.seed = body.seed;
                meshScript.bodyType = body.type;
                meshScript.Generate();
                bodies[i] = bodyObject;
                i++;
            }
        }
    }

    IEnumerator WaitForGenerated()
    {
        if (bodies == null)
            yield break;
        bool allReady;
        do {
            allReady = true;
            foreach (GameObject bodyObject in bodies)
            {
                if (bodyObject == null)
                    continue;
                MeshScript meshScript = bodyObject.GetComponent<MeshScript>();
                if (meshScript != null && !meshScript.generated && meshScript.bodyType != BodyType.Asteroid)
                {
                    allReady = false;
                    break;
                }
            }
            if (!allReady)
                yield return null;
        } while (!allReady);
        InitVariables();
    }
    void InitVariables()
    {
        foreach (GameObject bodyObject in bodies)
        {
            if (bodyObject == null)
                continue;
            bodyObject.GetComponent<GravitySource>().init();
        }

        foreach (GameObject bodyObject in bodies)
        {
            if (bodyObject == null)
                continue;
            bodyObject.GetComponent<Orbit>().orbitSpeed = bodyObject.GetComponent<MeshScript>().orbitSpeed;
            bodyObject.GetComponent<Orbit>().init();
        }
    }
}