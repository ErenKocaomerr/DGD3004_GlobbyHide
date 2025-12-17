using UnityEngine;

public class Note : MonoBehaviour
{
    [Header("Note Settings")]
    public Direction direction;
    public float speed = 5f;
    public bool isHit = false;

    [Header("Sprites")]
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateSpriteByDirection();
    }

    void Update()
    {
        // Aþaðý doðru hareket (y ekseninde)
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }

    public void OnHit()
    {
        if (isHit) return;
        isHit = true;
        Destroy(gameObject);
    }

    // Yön deðiþtiðinde doðru sprite'ý uygular
    public void UpdateSpriteByDirection()
    {
        if (spriteRenderer == null) return;

        switch (direction)
        {
            case Direction.Up:
                spriteRenderer.sprite = upSprite;
                break;
            case Direction.Down:
                spriteRenderer.sprite = downSprite;
                break;
            case Direction.Left:
                spriteRenderer.sprite = leftSprite;
                break;
            case Direction.Right:
                spriteRenderer.sprite = rightSprite;
                break;
        }
    }
}
