using DG.Tweening;
using UnityEngine;

public class BossHand : MonoBehaviour
{
    [Header("--- Ayarlar ---")]
    public int damage = 1; // Player'ýn can sistemi int olduðu için int yaptým

    [Header("--- Görsel Efekt ---")]
    public float shakeStrength = 0.5f;
    public int shakeVibrato = 20;
    public float trackingSmoothness = 0.1f;

    private Vector3 initialPosition;
    private SpriteRenderer spriteRenderer;
    private Collider2D col; // Collider referansý
    private Tween idleTween;

    void Start()
    {
        initialPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void StartFloating()
    {
        // Eðer zaten süzülüyorsa tekrar baþlatma
        if (idleTween != null && idleTween.IsActive()) return;

        // Bulunduðu yerden 0.5 birim yukarý aþaðý sürekli git gel
        // Random.Range ekledik ki iki el senkronize robot gibi durmasýn
        float randomDelay = Random.Range(0f, 0.5f);

        idleTween = transform.DOMoveY(initialPosition.y + 0.5f, 1f)
            .SetDelay(randomDelay)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo); // Sonsuz döngü
    }

    public void StopFloating()
    {
        if (idleTween != null) idleTween.Kill();
    }

    // --- YENÝ: COLLIDER DURUMUNU DEÐÝÞTÝRME ---
    // isAttackMode = TRUE ise -> IsTrigger olur (Hasar verir, içinden geçilir)
    // isAttackMode = FALSE ise -> Solid olur (Üstüne basýlabilir, hasar vermez)
    public void SetAttackMode(bool isAttackMode)
    {
        if (col != null)
        {
            col.isTrigger = isAttackMode;
        }
    }

    // --- HAREKET FONKSÝYONLARI ---
    public void TrackPlayer(Vector3 targetPos)
    {
        StopFloating();
        transform.DOMove(targetPos, trackingSmoothness).SetEase(Ease.OutQuad);
    }

    public Tween ShakeWarning(float duration)
    {
        spriteRenderer.color = Color.red;
        return transform.DOShakePosition(duration, shakeStrength, shakeVibrato, 90, false, true);
    }

    public void SmashDown(float groundY, float duration)
    {
        spriteRenderer.color = Color.white;
        transform.DOMoveY(groundY, duration).SetEase(Ease.InExpo).OnComplete(() =>
        {
            // YERE VURDUÐUNDA:

            // 1. Ekraný Salla
            if (CameraShaker.instance != null) 
            {
                CameraShaker.instance.Shake(0.75f);
                Debug.Log("EL YERE VURDU, EKRAN SALLANIYOR!");
            }
        });
    }

    public void ReturnToIdle(float duration)
    {
        spriteRenderer.color = Color.white;
        // Yerine dönünce süzülmeye baþla
        transform.DOMove(initialPosition, duration).SetEase(Ease.InOutSine)
            .OnComplete(StartFloating);

        transform.rotation = Quaternion.identity;
    }

    public void SweepMove(float targetX, float duration)
    {
        StopFloating();
        spriteRenderer.color = Color.white;
        transform.DOMoveX(targetX, duration).SetEase(Ease.Linear);
    }

    // --- HASAR MANTIÐI ---

    // 1. Trigger Modu (Saldýrý Aný): Hasar Ver
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("EL OYUNCUYA VURDU (Trigger)!");

            // DEÐÝÞÝKLÝK BURADA: PlayerHealth scriptini arýyoruz
            PlayerHealth health = other.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    // 2. Collision Modu (Idle Aný): Hasar Verme (Sadece fiziksel çarpýþma)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Burada hasar kodu YOK. Oyuncu üstüne basabilir.
        // Ýstersen oyuncuyu elin hareketiyle ezmemek için basit bir kontrol koyabilirsin.
    }
}
