using UnityEngine;

public class AsteroidDestructionTest : MonoBehaviour
{
    InputActions inputActions;
    FloatingOrigin floatingOrigin;

    void Awake()
    {
        inputActions = new InputActions();
        floatingOrigin = FindFirstObjectByType<FloatingOrigin>();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.LMB.performed += OnClick;
    }

    void OnDisable()
    {
        inputActions.Player.LMB.performed -= OnClick;
        inputActions.Disable();
    }

    void OnClick(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        DestroyAsteroid();
    }

    void DestroyAsteroid()
    {
        Vector2 mouseScreen = inputActions.Player.MousePos.ReadValue<Vector2>();
        Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(mouseScreen);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Asteroid"))
        {
            SpriteRenderer sr = hit.collider.GetComponent<SpriteRenderer>();
            Sprite sprite = sr.sprite;
            Texture2D tex = sprite.texture;

            // world → local
            Vector2 localPos = sr.transform.InverseTransformPoint(hit.point);

            // local → pixel space
            float ppu = sprite.pixelsPerUnit;
            Vector2 texSize = sprite.rect.size;

            Vector2 pixelPos = localPos * ppu + texSize / 2f;

            int x = Mathf.FloorToInt(pixelPos.x);
            int y = Mathf.FloorToInt(pixelPos.y);

            // safety clamp
            x = Mathf.Clamp(x, 0, tex.width - 1);
            y = Mathf.Clamp(y, 0, tex.height - 1);

            Color pixel = tex.GetPixel(x, y);

            Debug.Log($"Pixel: {x},{y} Color: {pixel}");

            tex.SetPixel(x, y, Color.clear);
            tex.Apply();

            Destroy(hit.collider);
            sr.gameObject.AddComponent<PolygonCollider2D>();
        }
    }
}
