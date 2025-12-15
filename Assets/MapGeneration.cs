using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MeshScript;

public class MapGeneration : MonoBehaviour
{
    //public string seedString;
    public ulong seed;

    public int planetMod;
    public int asteroidMod;
    public int starMod;
    public int gasGiantMod;
    public int planetMoonMod;
    public int gasGiantMoonMod;

    public GameObject mesh;

    string[] planetOrder;
    [SerializeField] int[] planetOrderM;
    [SerializeField] int[] gasOrderM;

    public int plaModM;
    public int gasModM;

    //private void Awake()
    //{
    //    if (!ulong.TryParse(seedString, out seed))
    //    {
    //        Debug.Log("Invalid seedString");
    //        seed = 1;
    //    }
    //}

    void Start()
    {
        //prevents division by 0
        if (seed == 0)
        {
            seed += 1;
        }

        int plaMod = 3 + (int)(hash(seed, 1) * 1000f) % planetMod;
        int astMod = (int)((seed % (ulong)asteroidMod) * 15) + 10;
        int staMod = (int)(seed % (ulong)starMod) + 1;
        int gasMod = 1 + (int)(hash(seed, 2) * 1000f) % gasGiantMod;
        //int plaModM = seed / (seed / 1 + i) % 5;
        //int gasModM = seed / (seed / 1 + i) % 10;
        //int plaModM = plaMod * 3; // change this to a certain hash
        //int gasModM = gasMod * 8;

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
        for (int i = 1;i < nPlanets; i++)
        {
            if (ints[i-1] > ints[i])
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
            planetOrderM[i] = (int)(hash(seed, i) * 1000f) % plaModM;
        }

        gasOrderM = new int[gasMod];
        for (int i = 0; i < gasOrderM.Length; i++)
        {
            gasOrderM[i] = (int)(hash(seed, i) * 1000f) % gasModM;
        }

        int distance = 0;

        // generate stars
        int[] radiuses = new int[staMod];
        int largestDiameter = 0;
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
            GameObject star = Instantiate(mesh, placement, Quaternion.identity);
            MeshScript meshScript = star.GetComponent<MeshScript>();
            int diameter = (int)(hash(seed, i) * 1000f) + 250;

            meshScript.diameter = diameter;
            meshScript.noise = true;
            meshScript.amplitude = 25f;
            meshScript.speed = 4f;
            meshScript.scale = 50f;
            meshScript.bodyType = MeshScript.BodyType.Star;

            meshScript.Generate();
        }

        int preDistance = 800 + largestDiameter * 2;

        int plaSelect = 0;
        int gasSelect = 0;
        // generate planets
        for (int i = 0; i < nPlanets; i++)
        {
            distance = (int)(250 + (hash(seed, i)) * 500f + preDistance);
            float angle = (float)hash(seed, i) * 360f;
            float k = (2 * Mathf.PI * angle);
            Vector2 placement = new Vector2(Mathf.Cos(k) * (distance * 2), Mathf.Sin(k) * (distance * 2));
            GameObject planet = Instantiate(mesh, placement, Quaternion.identity);
            MeshScript meshScript = planet.GetComponent<MeshScript>();
            meshScript.order = i;
            meshScript.seed = Mathf.Abs((int)seed);
            if (planetOrder[i] == "Planet")
            {
                int diameter = (int)((hash(seed, i)* 125) + 50);
                meshScript.diameter = diameter;
                meshScript.noise = false;
                meshScript.bodyType = MeshScript.BodyType.Planet;

                moonGeneration(plaSelect, "Planet", diameter, planet.transform.position, i);
                plaSelect++;
            }
            else if (planetOrder[i] == "Gas Giant")
            {
                int diameter = (int)((hash(seed, i) * 500f) + 100);
                meshScript.diameter = diameter;
                meshScript.noise = true;
                meshScript.scale = 15f;
                meshScript.amplitude = 8f;
                meshScript.speed = 1f;
                meshScript.bodyType = MeshScript.BodyType.GasGiant;

                //Generate moons
                moonGeneration(gasSelect, "Gas", diameter, planet.transform.position, i);
                gasSelect++;
            }
            meshScript.Generate();

            preDistance = distance;
        }
    }

    void moonGeneration(int arrayPosition, string bodyType, int bodyDiameter, Vector2 bodyPosition, int bodyOrder)
    {
        int moonCount = (bodyType == "Planet") ? planetOrderM[arrayPosition] : gasOrderM[arrayPosition];
        if (moonCount == 0)
        {
            if (predictBiome(Mathf.Abs((int)seed), bodyOrder) == "EarthLike")
            {
                moonCount = 1;
            }
        }

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
            int distance = (int)(largestDiameter * 2 + ((hash(seed, i) * 1000f) % 150) + bodyDiameter);

            float k = 0f;
            k = (2 * Mathf.PI * i) / moonCount;

            Vector2 placement = new Vector2((Mathf.Cos(k) * distance) + bodyPosition.x, (Mathf.Sin(k) * distance) + bodyPosition.y);
            GameObject moon = Instantiate(mesh, placement, Quaternion.identity);
            MeshScript meshScript = moon.GetComponent<MeshScript>();
            int diameter = (int)((hash(seed, i) * 1000f) % 50) + 10;
            diameter = (diameter > bodyDiameter / 2) ? diameter / 2 : diameter;

            meshScript.order = (bodyType == "Planet") ? arrayPosition + i : arrayPosition + 100 + i;
            meshScript.diameter = diameter;
            meshScript.noise = false;
            meshScript.bodyType = MeshScript.BodyType.Moon;

            meshScript.Generate();
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
        //ulong raw = mangled;
        //uint unsigned = (uint)raw;
        return (float)(uint)mangled / uint.MaxValue;
    }

    float predictHash(int seed, int x, int mod)
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

    //int hash(int seed, int i)
    //{
    //    long change = seed * 9443351 * (i + 1);
    //    change = (change << (int)(change%5));
    //    change = (change + (change % 500));
    //    change = ((change % 500) + 500) % 500;
    //    return (int)change;
    //}
}