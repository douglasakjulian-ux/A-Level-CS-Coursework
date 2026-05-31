using UnityEngine;
using UnityEngine.InputSystem;

public class BuildCameraScript : MonoBehaviour
{
    public float scrollSensitivity;
    public float moveSpeed;

    InputActions inputActions;

    void Awake()
    {
        inputActions = new InputActions();
        inputActions.Enable();
    }

    void Update()
    {
        Vector2 scroll = inputActions.Player.Zoom.ReadValue<Vector2>();
        scroll *= Mathf.Clamp(Camera.main.orthographicSize / 250f, 1, 4);

        Camera.main.orthographicSize += scroll.y * scrollSensitivity;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 5f, Mathf.Infinity);

        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();
        transform.position += new Vector3(move.x * moveSpeed * (Camera.main.orthographicSize / 5f) * Time.deltaTime, move.y * moveSpeed * (Camera.main.orthographicSize / 5f) * Time.deltaTime, 0);
    }
}
