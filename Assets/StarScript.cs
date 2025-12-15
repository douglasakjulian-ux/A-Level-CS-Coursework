using UnityEngine;

public class StarScript : MonoBehaviour
{
    Material mat;
    bool change;
    SpriteRenderer sr;
    [SerializeField] private float intensity;
    public float maxIntensity;
    public float offset;
    public float speed;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        change = false;
        intensity = offset;
        Shader shader = Resources.Load<Shader>("Shaders/SpriteGlow");
        mat = new Material(shader);
        mat.SetColor("_Color", Color.white * intensity);
        sr.material = mat;
    }

    void Update()
    {
        intensity = (change) ? intensity + (Time.deltaTime * speed) : intensity - (Time.deltaTime * speed);

        if (intensity < 2.5f) { change = true; }
        else if (intensity > maxIntensity) { change = false; }

        mat.SetColor("_Color", Color.white * intensity);
    }
}
