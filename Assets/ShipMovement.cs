using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ShipMovement : MonoBehaviour
{
    SystemGravity systemGravity;

    //move, y axis = thrust, x axis = turn
    Vector2 velocity;
    public float maxVelocity;
    InputActions inputActions;
    public float thrust;
    public float turnSpeed;

    GameObject camObj;

    public Vector2 test;

    void Awake()
    {
        systemGravity = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SystemGravity>();
        camObj = GameObject.FindGameObjectWithTag("MainCamera");
        velocity = new Vector2(0f, 0f);
        inputActions = new InputActions();
        inputActions.Enable();
    }

    void Update()
    {
        test = systemGravity.GetGravityAt((Vector2)transform.position);
        if (inputActions.Player.Esc.triggered)
        {
            StartCoroutine(GalaxyView());
        }

        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();
        velocity += (Vector2)transform.up * (move.y * thrust * Time.deltaTime);
        velocity += (systemGravity.GetGravityAt((Vector2)transform.position) * Time.deltaTime);
        velocity = Vector2.ClampMagnitude(velocity, maxVelocity);

        Vector3 rotation = transform.eulerAngles;
        rotation.z -= move.x * turnSpeed * Time.deltaTime;
        transform.eulerAngles = rotation;

        transform.position += (Vector3)velocity * Time.deltaTime;

        camObj.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }

    IEnumerator GalaxyView()
    {
        if (!SceneManager.GetSceneByName("Galaxy").isLoaded)
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync("Galaxy", LoadSceneMode.Additive);

            while (!loadOp.isDone)
                yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Galaxy"));

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            root.SetActive(true);
        }

        Scene scene = SceneManager.GetSceneByName("SolarSystem");
        if (!scene.isLoaded) yield break;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            root.SetActive(false);
        }

        Camera galaxyCam = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            galaxyCam = root.GetComponentInChildren<Camera>();
            if (galaxyCam != null) break;
        }
        galaxyCam.transform.position = GameObject.FindWithTag("Galaxy").GetComponent<GalaxyVisualiser>().bestStar.position;
    }
}
