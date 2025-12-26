using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public static class SystemView
{
    public static ulong seed;
    static (Vector2 pos, float size, Color32 col)[][] moons;
    public static GameObject[] starObjects;
    public static GameObject[] planetObjects;
    public static GameObject[] moonObjects;
    public static GameObject systemRoot;

    public static void ClearSystem()
    {
        GameObject.Destroy(systemRoot);
        starObjects = null;
        planetObjects = null;
        moonObjects = null;
        moons = null;
    }   

    public static void DisplaySystem()
    {
        if (systemRoot != null)
            ClearSystem();

        systemRoot = new GameObject("SystemRoot");
        systemRoot.transform.position = new Vector3(0, 0, 0);

        var stars = Stars();
        var planets = Planets();

        starObjects = new GameObject[stars.Length];
        planetObjects = new GameObject[planets.Length];
        moonObjects = new GameObject[moons.Length];

        // put into functions later
        int i = 0;
        foreach (var star in stars)
        {
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.layer = LayerMask.NameToLayer("SystemView");
            s.transform.parent = systemRoot.transform;
            s.transform.position = new Vector3(star.pos.x, star.pos.y, 0);
            s.transform.localScale = new Vector3(star.size, star.size, star.size);
            s.GetComponent<Renderer>().material.color = star.col;
            starObjects[i] = s;
            i++;
        }
        i = 0;
        foreach (var planet in planets)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.layer = LayerMask.NameToLayer("SystemView");
            p.transform.parent = systemRoot.transform;
            p.transform.position = new Vector3(planet.pos.x, planet.pos.y, 0);
            p.transform.localScale = new Vector3(planet.size, planet.size, planet.size);
            p.GetComponent<Renderer>().material.color = planet.col;
            planetObjects[i] = p;
            i++;
        }
        i = 0;
        foreach (var planetIndex in moons)
        {
            foreach (var moon in planetIndex)
            {
                GameObject m = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m.layer = LayerMask.NameToLayer("SystemView");
                m.transform.parent = systemRoot.transform;
                m.transform.position = new Vector3(moon.pos.x, moon.pos.y, 0);
                m.transform.localScale = new Vector3(moon.size, moon.size, moon.size);
                m.GetComponent<Renderer>().material.color = moon.col;
                moonObjects[i] = m;
            }
        }
    }

    static int largestDiameter;
    static (Vector2 pos, float size, Color32 col)[] Stars()
    {
        int distance = 0;
        int staMod = (int)(seed % (ulong)2) + 1; //Change depending on starMod
        (Vector2 pos, float size, Color32 col)[] stars = new (Vector2 pos, float size, Color32 col)[staMod];
        // generate stars
        int[] radiuses = new int[staMod];
        Debug.Log(staMod);
        for (int i = 0; i < staMod; i++)
        {
            Debug.Log("star loop");
            if (staMod > 1)
            {
                largestDiameter = 0;
                for (int x = 0; x < staMod; x++)
                {
                    int radius = (int)(hash(seed, i) * 1000f) + 250;
                    if (radius > largestDiameter)
                    {
                        largestDiameter = radius;
                    }
                }
                distance = (int)(largestDiameter + hash(seed, i) * 500f + 250);
            }
            else
            {
                distance = 0;
            }

            float k = 0f;
            k = (2 * Mathf.PI * i) / staMod;

            Vector2 placement = new Vector2(Mathf.Cos(k) * distance, Mathf.Sin(k) * distance);
            int diameter = (int)(hash(seed, i) * 1000f) + 250;
            stars[i] = (placement, (float)diameter / 2500f, new Color32(255, 255, 0, 255));
        }
        return stars;
    }

    static int[] planetOrderM;
    static int[] gasOrderM;
    static (Vector2 pos, float size, Color32 col)[] Planets()
    {
        int plaMod = 3 + (int)(hash(seed, 1) * 1000f) % 10; //Change depending on planetMod
        int gasMod = 1 + (int)(hash(seed, 2) * 1000f) % 4; //Change depending on gasGiantMod
        int nPlanets = plaMod + gasMod;

        (Vector2 pos, float size, Color32 col)[] planets = new (Vector2 pos, float size, Color32 col)[nPlanets];

        // allocate jagged moons array so moons[i] can be assigned
        moons = new (Vector2 pos, float size, Color32 col)[nPlanets][];

        string[] planetOrder = new string[nPlanets];

        //create array of planets
        int tempPlaMod = plaMod;
        int tempGasMod = gasMod;
        for (int i = 0; i < nPlanets; i++)
        {
            if (tempPlaMod > 0)
            {
                planetOrder[i] = "Planet";
                tempPlaMod--;
            }
            else if (tempGasMod > 0)
            {
                planetOrder[i] = "Gas Giant";
                tempGasMod--;
            }
        }

        //"random" values for ints
        int[] ints = new int[nPlanets];
        for (int i = 0; i < nPlanets; i++)
        {
            ints[i] = (int)(hash(seed, i) * 500f);
        }

        //sort lists
        for (int i = 1; i < nPlanets; i++)
        {
            if (ints[i - 1] > ints[i])
            {
                int temp = ints[i - 1];
                string plaTemp = planetOrder[i - 1];
                ints[i - 1] = ints[i];
                ints[i] = temp;
                planetOrder[i - 1] = planetOrder[i];
                planetOrder[i] = plaTemp;
                i = 0;
            }
        }

        planetOrderM = new int[plaMod];
        for (int i = 0; i < planetOrderM.Length; i++)
        {
            planetOrderM[i] = (int)(hash(seed, i) * 1000f) % 3; //PlaModM
        }

        gasOrderM = new int[gasMod];
        for (int i = 0; i < gasOrderM.Length; i++)
        {
            gasOrderM[i] = (int)(hash(seed, i) * 1000f) % 8; //GasModM
        }

        float preDistance = 800 + largestDiameter * 2;

        int plaSelect = 0;
        int gasSelect = 0;
        float distance = 0;
        // generate planets
        int diameter = 0;
        Color32 col = new Color32(255, 255, 255, 255);
        for (int i = 0; i < nPlanets; i++)
        {
            distance = ((250f + (hash(seed, i)) * 500f + preDistance) / 2500f);
            float angle = (float)hash(seed, i) * 360f;
            float k = (2 * Mathf.PI * angle);
            Vector2 placement = new Vector2(Mathf.Cos(k) * (distance * 2), Mathf.Sin(k) * (distance * 2));
            //meshScript.order = i;
            int intSeed = Mathf.Abs((int)seed);
            float textureSeed = texHash(intSeed, i + 1, 1000000);
            if (planetOrder[i] == "Planet")
            {
                diameter = (int)((hash(seed, i) * 125) + 50);
                //int moonCount = planetOrderM[plaSelect];
                //if (moonCount == 0)
                //{
                //    if (predictBiome(Mathf.Abs((int)seed), i) == "EarthLike")
                //    {
                //        moonCount = 1;
                //    }
                //}
                //moons[i] = new (Vector2 pos, float size, Color32 col)[moonCount];
                moons[i] = MoonGeneration(plaSelect, "Planet", diameter, placement, i);
                col = new Color32((byte)(texHash(intSeed, (int)textureSeed + 6, 255)), (byte)(texHash(intSeed, (int)textureSeed + 7, 255)), (byte)(texHash(intSeed, (int)textureSeed + 8, 255)), 255);
                plaSelect++;
            }
            else if (planetOrder[i] == "Gas Giant")
            {
                diameter = (int)((hash(seed, i) * 500f) + 100);

                //Generate moons
                //int moonCount = gasOrderM[gasSelect];
                //moons[i] = new (Vector2 pos, float size, Color32 col)[moonCount];
                //moons[i] = MoonGeneration(gasSelect, "Gas", diameter, placement, i);
                col = new Color32((byte)(texHash(intSeed, (int)textureSeed, 255)), (byte)(texHash(intSeed, (int)textureSeed + 1, 255)), (byte)(texHash(intSeed, (int)textureSeed + 2, 255)), 255);
                gasSelect++;
            }

            planets[i] = (placement, (float)diameter / 2500f, col);
            preDistance = distance;
        }
        return planets;
    }

    static (Vector2 pos, float size, Color32 col)[] MoonGeneration(int arrayPosition, string bodyType, int bodyDiameter, Vector2 bodyPosition, int bodyOrder)
    {
        int moonCount = (bodyType == "Planet") ? planetOrderM[arrayPosition] : gasOrderM[arrayPosition];
        if (moonCount == 0)
        {
            if (predictBiome(Mathf.Abs((int)seed), bodyOrder) == "EarthLike")
            {
                moonCount = 1;
            }
        }
        moons[bodyOrder] = new (Vector2 pos, float size, Color32 col)[moonCount];

        (Vector2 pos, float size, Color32 col)[] moonList = new (Vector2 pos, float size, Color32 col)[moonCount];
        for (int i = 0; i < moonCount; i++)
        {
            int largestDiameter = 0;
            for (int x = 0; x < moonCount; x++)
            {
                int radius = (int)((hash(seed, i) * 1000f) % 50) + 10;
                if (radius > largestDiameter)
                {
                    largestDiameter = radius;
                }
            }
            float distance = (float)((float)largestDiameter * 2f + (((hash(seed, i) * 1000f) % 150)/2500f) + bodyDiameter);

            float k = 0f;
            k = (2 * Mathf.PI * i) / moonCount;

            Vector2 placement = new Vector2((Mathf.Cos(k) * distance) + bodyPosition.x, (Mathf.Sin(k) * distance) + bodyPosition.y);
            int diameter = (int)((hash(seed, i) * 1000f) % 50) + 10;
            diameter = (diameter > bodyDiameter / 2) ? diameter / 2 : diameter;

            moonList[i] = (placement, diameter / 2500f, new Color32(190,190,190,255));
        }
        return moonList;
    }

    static float hash(ulong seed, int x)
    {
        const ulong BIT_NOISE1 = 0xB5297A4DUL;
        const ulong BIT_NOISE2 = 0x68E31DA4UL;
        const ulong BIT_NOISE3 = 0x1B56C4E9UL;

        ulong mangled = (ulong)x;
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

    static float texHash(int seed, int x, int mod)
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

    static float predictHash(int seed, int x, int mod)
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

    static string predictBiome(int seed, int x)
    {
        float textureSeed = predictHash(seed, x + 1, 1000000);
        float type = predictHash(seed, (int)textureSeed, 100);
        string biome = "";
        if (type >= 70) { biome = "Volcano"; }
        else if (type >= 50) { biome = "EarthLike"; }
        else if (type >= 30) { biome = "Desert"; }
        else { biome = "Rock"; }
        return biome;
    }

}
