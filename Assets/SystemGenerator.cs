using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static MeshScript;

public static class SystemGenerator
{
    public static SystemData Generate(ulong seed)
    {
        seed = seed == 0 ? 1UL : seed; // Prevent div by 0
        return new SystemData(seed);
    }
}

public class BodyData
{
    public BodyType type;
    public Vector2 position;
    public float diameter;
    public float speed;
    public int moons = 0;

    public int order;
    public int seed;

    public bool shadowBehind;
}

public class SystemData
{
    public ulong seed { get; }
    List<BodyData> data = new List<BodyData>();

    public IReadOnlyList<BodyData> Bodies => data;

    public SystemData(ulong seed)
    {
        this.seed = seed;
        Generate();
    }

    int planetMod = SystemSettings.planetMod;
    int asteroidMod = SystemSettings.asteroidMod;
    int starMod = SystemSettings.starMod;
    int gasGiantMod = SystemSettings.gasGiantMod;
    int planetMoonMod = SystemSettings.planetMoonMod;
    int gasGiantMoonMod = SystemSettings.gasGiantMoonMod;

    string[] planetOrder;
    int[] planetOrderM;
    int[] gasOrderM;

    void Generate()
    {
        int plaMod = 3 + (int)(hash(seed, 1) * 1000f) % planetMod;
        int astMod = (int)((seed % (ulong)asteroidMod) * 15) + 10;
        int staMod = (int)(seed % (ulong)starMod) + 1;
        int gasMod = 1 + (int)(hash(seed, 2) * 1000f) % gasGiantMod;

        //number of planets
        int nPlanets = plaMod + gasMod;
        planetOrder = new string[nPlanets];

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
            planetOrderM[i] = (int)(hash(seed, i) * 1000f) % planetMoonMod;
        }

        gasOrderM = new int[gasMod];
        for (int i = 0; i < gasOrderM.Length; i++)
        {
            gasOrderM[i] = (int)(hash(seed, i) * 1000f) % gasGiantMoonMod;
        }

        int distance = 0;

        // generate stars
        int[] radiuses = new int[staMod];
        int largestDiameter = 0;
        Debug.Log(staMod);
        int starDistance = 0;
        for (int i = 0; i < staMod; i++)
        {
            Debug.Log("star loop");
            if (staMod > 1)
            {
                largestDiameter = 0;
                for (int x = 0; x < staMod; x++)
                {
                    int radius = (int)(hash(seed, i) * 2500f) + 1000;
                    if (radius > largestDiameter)
                    {
                        largestDiameter = radius;
                    }
                }
                distance = (int)(largestDiameter + hash(seed, i) * 2500f + 1250);
            }
            else
            {
                distance = 0;
            }
            starDistance = distance;
            float k = 0f;
            k = (2 * Mathf.PI * i) / staMod;

            Vector2 placement = new Vector2(Mathf.Cos(k) * distance, Mathf.Sin(k) * distance);
            //GameObject star = Instantiate(mesh, placement, Quaternion.identity);
            //MeshScript meshScript = star.GetComponent<MeshScript>();
            int diameter = (int)(hash(seed, i) * 2500f) + 1000;

            BodyData bodyData = new BodyData();
            bodyData.type = BodyType.Star;
            bodyData.position = placement;
            bodyData.diameter = diameter;
            bodyData.speed = 2f;

            data.Add(bodyData);
        }

        int preDistance = starDistance + largestDiameter * 2;

        int plaSelect = 0;
        int gasSelect = 0;
        // generate planets
        for (int i = 0; i < nPlanets; i++)
        {
            float distanceHash = ((hash(seed, i)) * 4000f);
            distance = (int)(5000 + (1 + ((distanceHash / 2500f) / 2f) * distanceHash + preDistance));
            float angle = (float)hash(seed, i) * 360f;
            float k = (2 * Mathf.PI * angle * Mathf.Deg2Rad);
            Vector2 placement = new Vector2(Mathf.Cos(k) * (distance * 2), Mathf.Sin(k) * (distance * 2));
            //GameObject planet = Instantiate(mesh, placement, Quaternion.identity);
            //MeshScript meshScript = planet.GetComponent<MeshScript>();
            if (planetOrder[i] == "Planet")
            {
                int diameter = (int)((hash(seed, i) * 200) + 100);
                BodyData bodyData = new BodyData();
                bodyData.type = BodyType.Planet;
                bodyData.position = placement;
                bodyData.diameter = diameter;
                bodyData.speed = 0.005f + (hash(seed, i) * 2f) / (placement.magnitude) / 100f;
                bodyData.moons = planetOrderM[plaSelect];
                bodyData.order = i;
                bodyData.seed = Mathf.Abs((int)seed);

                bool shadowBehind = false;
                if ((hash(seed, i) * 2f) > 1f)
                    shadowBehind = true;
                else
                    shadowBehind = false;
                bodyData.shadowBehind = shadowBehind;

                data.Add(bodyData);

                moonGeneration(plaSelect, "Planet", diameter, placement, i, shadowBehind);
                plaSelect++;
            }
            else if (planetOrder[i] == "Gas Giant")
            {
                int diameter = (int)((hash(seed, i) * 1500f) + 500);
                BodyData bodyData = new BodyData();
                bodyData.type = BodyType.GasGiant;
                bodyData.position = placement;
                bodyData.diameter = diameter;
                bodyData.moons = gasOrderM[gasSelect];
                bodyData.order = i;
                bodyData.seed = Mathf.Abs((int)seed);
                bodyData.speed = 0.005f + (hash(seed, i) * 2f) / (placement.magnitude) / 100f;

                bool shadowBehind = false;
                if ((hash(seed, i) * 2f) > 1f)
                    shadowBehind = true;
                else
                    shadowBehind = false;
                bodyData.shadowBehind = shadowBehind;

                //meshScript.scale = 15f;
                //meshScript.amplitude = 8f;
                //meshScript.speed = 1f;
                data.Add(bodyData);

                //Generate moons
                moonGeneration(gasSelect, "Gas", diameter, placement, i, shadowBehind);
                gasSelect++;
            }

            preDistance = distance;
        }
    }

    void moonGeneration(int arrayPosition, string bodyType, int bodyDiameter, Vector2 bodyPosition, int bodyOrder, bool shadowBehind)
    {
        int moonCount = (bodyType == "Planet") ? planetOrderM[arrayPosition] : gasOrderM[arrayPosition];
        if (moonCount == 0)
        {
            if (predictBiome(Mathf.Abs((int)seed), bodyOrder) == "EarthLike")
            {
                moonCount = 1;
            }
        }
        int preDist = 0;
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
            int distance = (int)(largestDiameter * 2 + ((hash(seed, i) * 2000f) % 650) + bodyDiameter + 500);

            float angle = (float)hash(seed + (ulong)largestDiameter, i) * 360f;
            float k = (2 * Mathf.PI * angle * Mathf.Deg2Rad);

            Vector2 placement = new Vector2((Mathf.Cos(k) * (distance + preDist)) + bodyPosition.x, (Mathf.Sin(k) * (distance + preDist)) + bodyPosition.y);
            int diameter = (int)((hash(seed, i) * 1000f) % 50) + 10;
            diameter = (diameter > bodyDiameter / 2) ? diameter / 2 : diameter;

            BodyData bodyData = new BodyData();
            bodyData.type = BodyType.Moon;
            bodyData.position = placement;
            bodyData.diameter = diameter;
            bodyData.order = i;
            bodyData.seed = Mathf.Abs((int)seed);
            bodyData.speed = 0.005f + (hash(seed, i) * 3f) / ((bodyPosition.magnitude - placement.magnitude)/ 5000f) / 10f;
            bodyData.speed = 0.05f;
            bodyData.shadowBehind = shadowBehind;

            data.Add(bodyData);
            preDist += distance;
        }
    }

    float hash(ulong seed, int x)
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
        return (float)(uint)mangled / uint.MaxValue;
    }

    float predictHash(int seed, int x, int mod)
    {
        const int BIT_NOISE1 = 1759714724;
        const int BIT_NOISE2 = 1965652557;
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
        return (float)unsigned % mod;
    }

    string predictBiome(int seed, int x)
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