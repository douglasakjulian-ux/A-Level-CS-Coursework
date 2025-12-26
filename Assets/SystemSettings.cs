using UnityEngine;

public static class SystemSettings
{
    //Generation Settings
    public static int planetMod = 10;
    public static int asteroidMod = 10;
    public static int starMod = 2;
    public static int gasGiantMod = 4;
    public static int planetMoonMod = 2;
    public static int gasGiantMoonMod = 8;
    public static int resolution = 100;

    //Mesh for bodies
    public static GameObject mesh = Resources.Load<GameObject>("Mesh");
}
