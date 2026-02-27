using UnityEngine;
using UnityEngine.Rendering;

public class GameCameraScript : MonoBehaviour
{
    Camera main;
    Camera cam;
    void Awake()
    {
        GameObject mainOBJ = GameObject.FindWithTag("MainCamera");
        main = mainOBJ.GetComponent<Camera>();
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        cam.orthographicSize = Camera.main.orthographicSize;
    }
}
