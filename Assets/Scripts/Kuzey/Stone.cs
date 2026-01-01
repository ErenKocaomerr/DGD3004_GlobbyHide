using UnityEngine;

public class Stone : MonoBehaviour
{
    Rigidbody2D rb;
    Collider2D col;

    [HideInInspector] public bool isGrabbed = false;
    [HideInInspector] public RoundManager roundManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void Init(float size, float gravityScale = 1f)
    {
        transform.localScale = Vector3.one * size;
        rb.mass = Mathf.Max(0.1f, size * size);
        rb.gravityScale = gravityScale;
    }

    public void OnGrab()
    {
        isGrabbed = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void OnRelease(Vector2 impulse)
    {
        isGrabbed = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Girl"))
        {
            NotifyGirlHit();
            Debug.Log("AAAAA");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Girl"))
        {
            NotifyGirlHit();
        }
    }

    void NotifyGirlHit()
    {
        if (roundManager != null)
            roundManager.OnGirlHit(this);
        else
            Debug.LogWarning("Stone: roundManager null, cannot notify girl hit.");
    }

    void OnBecameInvisible()
    {
        if (transform.position.y < Camera.main.transform.position.y - 25f && gameObject != null)
            Destroy(gameObject);
    }
}
