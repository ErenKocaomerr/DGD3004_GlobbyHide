using UnityEngine;

public class BasicTopdownController : MonoBehaviour
{
     public float moveSpeed = 5f;
    public Transform body; // Player'ýn altýndaki sprite veya child objesi (örneðin "Body")

    Rigidbody2D rb;
    Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        UpdateFacingDirection();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void UpdateFacingDirection()
    {
        if (body == null) return;

        // Dikey veya yatay yönde bir hareket varsa dön
        if (movement.sqrMagnitude > 0.1f)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                // Yatay hareket baskýnsa
                if (movement.x > 0)
                    body.localRotation = Quaternion.Euler(0, 0, 0); // Saða bak
                else
                    body.localRotation = Quaternion.Euler(0, 180, 0); // Sola bak
            }
            else
            {
                // Dikey hareket baskýnsa
                if (movement.y > 0)
                    body.localRotation = Quaternion.Euler(0, 0, 90); // Yukarý bak
                else
                    body.localRotation = Quaternion.Euler(0, 0, -90); // Aþaðý bak
            }
        }
    }
}
