using UnityEngine;

public class PlayController : MonoBehaviour
{
    public float moveSpeed = 5f;

    Rigidbody2D rb;
    Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Rigidbody2D ayarlarýný Inspector'dan: Gravity Scale = 0, Freeze Rotation = z
    }

    void Update()
    {
        // WASD / Ok tuþlarý + joystick (eski input)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized; // köþegenlerde hýz artmasýný önler
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        // Yönlendirme istersen (sprite yukarý bakýyorsa):
        if (movement.sqrMagnitude > 0.001f)
            transform.up = movement; // objeyi hareket yönüne döndürür
    }
}
