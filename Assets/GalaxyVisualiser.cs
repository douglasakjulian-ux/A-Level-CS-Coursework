using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GalaxyVisualiser : MonoBehaviour
{
    //public int depth; // for inserting quadTree
    public int seed;
    public int initialDepth;
    public float size;
    Quadtree<string> quadtree;
    Quadtree<string> galaxytree;
    public Material galaxyMat;
    public int depth;
    public float rotation;

    GameObject highlight = null;
    GameObject camObj;
    Camera cam;
    GameObject point;
    Vector2 camObjGridPos = Vector2.zero;

    Quadtree<string>[] rootTrees;

    InputActions inputActions;

    string[] alphabet = new string[]
    {
        "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"
    };
    string[] consonants = new string[]
    {
        "b","c","d","f","g","h","j","k","l","m","n","p","q","r","s","t","v","w","x","y","z",
        "br","cr","dr","fr","gr","pr","tr",
        "bl","cl","fl","gl","pl","sl",
        "ch","sh","th",
        "sc","sk","sm","sn","sp","st","sw"
    };
    string[] vowels = new string[]
    {
        "a","e","i","o","u",
        "ae","ai","ao","au",
        "ea","ei","eu",
        "ia","io","iu",
        "oa","oi","ou",
        "ua","ui","uo"
    };

    private void Start()
    {
        inputActions = new InputActions();
        inputActions.Enable();
        rootTrees = new Quadtree<string>[9];
        camObj = GameObject.FindWithTag("MainCamera");
        cam = camObj.GetComponent<Camera>();
        //point = GameObject.FindWithTag("EditorOnly");
        //pointLastPos = new Vector2(point.transform.position.x - 1f, point.transform.position.y); // offset to force update

        DensityCalculator.seed = seed;
        DensityCalculator.size = size;
        DensityCalculator.rotation = rotation;
        Rect originRect = new Rect(-(size / 2f) + gameObject.transform.position.x, -(size / 2f) + gameObject.transform.position.y, size, size);
        galaxytree = new Quadtree<string>(originRect, 4, 0, initialDepth, false);
        quadtree = new Quadtree<string>(originRect, 4, 0, depth, true);
        //quadtree.density = 225000000f;

        Mesh mesh = QuadtreeMesh(galaxytree);

        var filter = GetComponent<MeshFilter>();
        filter.mesh = mesh;
        var renderer = GetComponent<MeshRenderer>();
        renderer.material = galaxyMat;
    }

    private void FixedUpdate()
    {
        if (cam.orthographicSize > 40) { return; }
        float x = camObj.transform.position.x;
        float y = camObj.transform.position.y;
        float rectSize = size / (float)(1 << depth);
        //if ((Vector2)point.transform.position != pointLastPos && quadtree.rect.Contains(point.transform.position))
        //{
        //    pointLastPos = (Vector2)point.transform.position;
        //    rootTrees[i] = quadtree.InsertQuadtree(point.transform.position, depth);
        //}
        if (camObjGridPos != new Vector2(Mathf.FloorToInt(x / (rectSize / 2f)), Mathf.FloorToInt(y / (rectSize / 2f))))
        {
            Vector2 bl = new Vector2(x - rectSize, y - rectSize); // bottom left
            Vector2 ml = new Vector2(x - rectSize, y); // middle left
            Vector2 tl = new Vector2(x - rectSize, y + rectSize); // top left
            Vector2 mt = new Vector2(x, y + rectSize); // middle top
            Vector2 tr = new Vector2(x + rectSize, y + rectSize); // top right
            Vector2 mr = new Vector2(x + rectSize, y); // middle right
            Vector2 br = new Vector2(x + rectSize, y - rectSize); // bottom right
            Vector2 mb = new Vector2(x, y - rectSize); // middle bottom

            Insert(camObj.transform.position, 0);
            Insert(bl, 1);
            Insert(ml, 2);
            Insert(tl, 3);
            Insert(mt, 4);
            Insert(tr, 5);
            Insert(mr, 6);
            Insert(br, 7);
            Insert(mb, 8);
        }
        camObjGridPos = new Vector2(Mathf.FloorToInt(x / (rectSize / 2f)), Mathf.FloorToInt(y / (rectSize / 2f)));
    }

    void Insert(Vector2 pos, int i)
    {
        ////if ((Vector2)point.transform.position != pointLastPos && quadtree.rect.Contains(pos))
        ////{
        ////    pointLastPos = (Vector2)point.transform.position;
        ////    rootTrees[i] = quadtree.InsertQuadtree(pos, depth);
        ////}
        //if (quadtree.rect.Contains(pos))
        //{
        //    rootTrees[i] = quadtree.InsertQuadtree(pos, depth);
        //}
        //else
        //{
        //    rootTrees[i] = null;
        //}
        rootTrees[i] = quadtree.FindDeepest(pos, depth);
    }

    int preIndex;
    Star preStar;
    Quadtree<string> preRoot = null;
    public Star bestStar = default;
    private void Update()
    {
        //finding nearest star to clicked position
        if (inputActions.Player.LMB.triggered)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            Vector2 mousePos = cam.ScreenToWorldPoint(inputActions.Player.MousePos.ReadValue<Vector2>());
            float bestDist = float.MaxValue;
            bestStar = default;
            Quadtree<string> bestRoot = null;
            foreach (var root in rootTrees)
            {
                if (root == null) { return; }
                Star candidate = root.NearestStar(mousePos);
                float dist = Vector2.Distance(candidate.position, mousePos);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestStar = candidate;
                    bestRoot = root;
                }
            }
            if (preRoot != null)
            {
                preIndex = preStar.index * 4;
                Color[] preCols = preRoot.starMesh.colors;
                preCols[preIndex] = preStar.color;
                preCols[preIndex + 1] = preStar.color;
                preCols[preIndex + 2] = preStar.color;
                preCols[preIndex + 3] = preStar.color;
                preRoot.starMesh.SetColors(preCols);
            }
            string seedStr = bestStar.seed.ToString();
            string name = null;
            name += alphabet[int.Parse(seedStr[1].ToString()) % alphabet.Length];
            int nameLength = (int)(Mathf.Abs(hash((int)bestStar.seed, bestStar.index, 5)) + 3);
            for (int i = 0; i < nameLength; i++)
            {
                if (i % 2 == 0)
                {
                    name += vowels[int.Parse(seedStr[i].ToString()) % vowels.Length];
                }
                else
                {
                    name += consonants[int.Parse(seedStr[i].ToString()) % consonants.Length];
                }
            }
            name += "-" + seedStr.Substring(0, 3);
            bestStar.name = name;
            Debug.Log(name);

            bestStar.Clicked();
            preStar.Unclicked();

            //highlighting closest star
            int index = bestStar.index * 4;
            Color[] cols = bestRoot.starMesh.colors;
            cols[index] = Color.red;
            cols[index + 1] = Color.red;
            cols[index + 2] = Color.red;
            cols[index + 3] = Color.red;
            bestRoot.starMesh.SetColors(cols);

            Destroy(highlight);
            highlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(highlight.GetComponent<MeshCollider>());

            highlight.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"));

            highlight.transform.position = (Vector3)bestStar.position + new Vector3(0, 0, 0.5f);
            highlight.transform.localScale = new Vector2((bestStar.radius * 2f) + 0.25f, (bestStar.radius * 2f) + 0.25f);

            Debug.Log(bestStar.seed);
            preIndex = index;
            preStar = bestStar;
            preRoot = bestRoot;
        }

        foreach (var root in rootTrees)
        {
            if (root != null)
            {
                root.Draw();
                Color c = root.starMaterial.color;
                if (cam.orthographicSize > 40f)
                {
                    c.a = 0f;
                }
                else
                {
                    c.a = Mathf.Lerp(0f, 1f, Mathf.Clamp(40 - cam.orthographicSize, 0, 40) / 20f);
                }
                root.starMaterial.color = c;
            }
        }
    }

    Mesh QuadtreeMesh(Quadtree<string> tree)
    {
        float totalDensity = 0f;
        List<(Rect rect, float density)> rects = new List<(Rect, float)>();
        galaxytree.GetRects(rects);
        List<Vector3> verticies = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();

        int index = 0;
        foreach (var (rect, density) in rects)
        {
            totalDensity += density;
            //float t = 1 - (density / 5000f);
            float t = density / 500000f;
            Color color = Color.Lerp(new Color(0.1f, 0.2f, 1f), new Color(0.8f, 0.9f, 1f), t);
            color.a = Mathf.Lerp(0f, 1f, t * t);
            //Debug.Log($"Rect {rect.center} density={density} alpha={t}");
            //color.a = 0.5f;

            Vector2 bl = new Vector2(rect.xMin, rect.yMin);
            Vector2 br = new Vector2(rect.xMax, rect.yMin);
            Vector2 tl = new Vector2(rect.xMin, rect.yMax);
            Vector2 tr = new Vector2(rect.xMax, rect.yMax);

            verticies.Add(bl);
            verticies.Add(br);
            verticies.Add(tl);
            verticies.Add(tr);

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
            triangles.Add(index + 1);
            index += 4;
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(verticies);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();

        Debug.Log(totalDensity);
        return mesh;
    }

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

    public void OnButtonClick()
    {
        if (bestStar.seed == default) { return; }

        StartCoroutine(LoadSolarSystem());
    }

    IEnumerator LoadSolarSystem()
    {
        if (SceneManager.GetSceneByName("SolarSystem").isLoaded)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync("SolarSystem");
            while (!unloadOp.isDone)
                yield return null;
        }

        AsyncOperation loadOp = SceneManager.LoadSceneAsync("SolarSystem", LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("SolarSystem"));

        GameObject sceneMan = GameObject.FindWithTag("SceneManager");
        sceneMan.GetComponent<MapGeneration>().seed = bestStar.seed;
        sceneMan.GetComponent<MapGeneration>().CreateSystem();

        Scene scene = SceneManager.GetSceneByName("Galaxy");
        if (!scene.isLoaded) yield break;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            root.SetActive(false);
        }
    }
}

