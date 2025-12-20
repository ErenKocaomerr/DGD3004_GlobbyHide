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
    private bool hasEnteredScreen = false;

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

        CheckOutOfBounds();
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

    void CheckOutOfBounds()
    {
        // Objeyi kamera koordinatlarýna (Viewport) çevir
        // 0,0 = Sol Alt, 1,1 = Sað Üst
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);

        // Önce notun ekrana girdiðinden emin olalým
        // (0 ile 1 arasýndaysa ekrandadýr)
        if (viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1)
        {
            hasEnteredScreen = true;
        }

        // Eðer not bir kere ekrana girdiyse VE þu an dýþarý çýktýysa yok et
        if (hasEnteredScreen)
        {
            // Biraz pay býrakýyoruz (-0.1 ve 1.1) ki not tam sýnýrdan pýrt diye yok olmasýn
            if (viewPos.x < -0.2f || viewPos.x > 1.2f || viewPos.y < -0.2f || viewPos.y > 1.2f)
            {
                Destroy(gameObject);
            }
        }
    }
}
