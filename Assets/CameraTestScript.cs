using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraTestScript : MonoBehaviour
{
    bool lowRes;
    public float scrollSensitivity;
    public float moveSpeed;

    public int targetWidth;
    public int targetHeight;
    RenderTexture lowResRT;

    InputActions inputActions;

    Camera gameCam;

    void Awake()
    {
        lowRes = true;
        inputActions = new InputActions();
        inputActions.Enable();

        lowResRT = new RenderTexture(targetWidth, targetHeight, 16);
        //lowResRT.filterMode = FilterMode.Point;
        //SetResolution(1920, 1080);

        gameCam = GameObject.FindWithTag("GameCamera").GetComponent<Camera>();
    }

    void Update()
    {
        Vector2 scroll = inputActions.Player.Zoom.ReadValue<Vector2>();
        scroll *= Mathf.Clamp(Camera.main.orthographicSize / 250f, 1, 4);

        Camera.main.orthographicSize += scroll.y * scrollSensitivity;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 5f, Mathf.Infinity);
        gameCam.orthographicSize = Camera.main.orthographicSize;

        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();
        transform.position += new Vector3(move.x * moveSpeed * (Camera.main.orthographicSize / 5f) * Time.deltaTime, move.y * moveSpeed * (Camera.main.orthographicSize / 5f) * Time.deltaTime, 0);

        //if (inputActions.Player.Resolution.triggered)
        //{
        //    if (lowRes == false)
        //    {
        //        //SetResolution(320, 180);
        //        Camera.main.targetTexture = lowResRT;
        //        lowRes = !lowRes;
        //    }
        //    else if (lowRes == true)
        //    {
        //        //SetResolution(1920, 1080);
        //        Camera.main.targetTexture = default;
        //        lowRes = !lowRes;
        //    }
        //}
    }

    void SetResolution(int width, int height)
    {
        if (lowResRT != null)
        {
            lowResRT.Release();
            Destroy(lowResRT);
        }

        lowResRT = new RenderTexture(width, height, 16);
        lowResRT.filterMode = FilterMode.Point;
        Camera.main.targetTexture = lowResRT;
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Camera.main.targetTexture, ScaleMode.StretchToFill, false);
    }

    public Vector3 GetMouseWorldPosition(Vector2 mouse)
    {
        float scaleX = (float)Camera.main.targetTexture.width / Screen.width;
        float scaleY = (float)Camera.main.targetTexture.height / Screen.height;

        Vector2 scaledMouse = new Vector2(mouse.x * scaleX, mouse.y * scaleY);

        return Camera.main.ScreenToWorldPoint(
            new Vector3(scaledMouse.x, scaledMouse.y, -Camera.main.transform.position.z)
        );
    }
}
