using UnityEngine;
using UnityEngine.InputSystem;

public class ShipMovement : MonoBehaviour
{
    //move, y axis = thrust, x axis = turn
    Vector3 velocity;
    public float maxVelocity;
    InputActions inputActions;
    public float thrust;
    public float turnSpeed;

    GameObject camObj;

    void Awake()
    {
        camObj = GameObject.FindGameObjectWithTag("MainCamera");
        velocity = new Vector3(0f, 0f, 0f);
        inputActions = new InputActions();
        inputActions.Enable();
    }

    void Update()
    {
        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();
        velocity += transform.up * (move.y * thrust * Time.deltaTime);
        velocity = Vector3.ClampMagnitude(velocity, maxVelocity);

        Vector3 rotation = transform.eulerAngles;
        rotation.z -= move.x * turnSpeed * Time.deltaTime;
        transform.eulerAngles = rotation;

        transform.position += velocity * Time.deltaTime;

        camObj.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }
}
