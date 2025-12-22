using DG.Tweening;
using UnityEngine;

public class BossHand : MonoBehaviour
{
    [Header("--- Ayarlar ---")]
    public int damage = 1;

    [Header("--- Görsel Efekt ---")]
    public float shakeStrength = 0.5f;
    public int shakeVibrato = 20;
    public float trackingSmoothness = 0.1f;

    private Vector3 initialPosition;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Tween idleTween;

    [Header("--- Ses Efektleri ---")]
    public AudioClip warningSFX;
    public AudioClip smashImpactSFX;
    public AudioClip sweepSFX;
    public AudioSource audioSource;

    public bool isDamaging = false;

    void Start()
    {
        initialPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void StartFloating()
    {
        if (idleTween != null && idleTween.IsActive()) return;

        float randomDelay = Random.Range(0f, 0.5f);

        idleTween = transform.DOMoveY(initialPosition.y + 0.5f, 1f)
            .SetDelay(randomDelay)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopFloating()
    {
        if (idleTween != null) idleTween.Kill();
    }

    // --- YENÝ EKLENEN FONKSÝYON: HER ÞEYÝ DURDUR ---
    // Bu fonksiyon sadece süzülmeyi deðil, "Yerine dönme" hareketini de iptal eder.
    // Böylece "Yerine dönünce süzülmeye baþla" emri devreye giremez.
    public void StopEverything()
    {
        transform.DOKill(); // Objeye baðlý tüm DOTween hareketlerini (Move, Rotate, Shake) öldür.
        if (idleTween != null) idleTween.Kill();
    }
    // ------------------------------------------------

    public void SetAttackMode(bool isAttackMode)
    {
        if (col != null) col.isTrigger = isAttackMode;
    }

    public void TrackPlayer(Vector3 targetPos)
    {
        StopFloating();
        isDamaging = false;
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
        isDamaging = true;
        transform.DOMoveY(groundY, duration).SetEase(Ease.InExpo).OnComplete(() =>
        {
            if (smashImpactSFX) audioSource.PlayOneShot(smashImpactSFX);
            if (CameraShaker.instance != null)
            {
                CameraShaker.instance.Shake(0.75f);
            }

            isDamaging = false;
        });
    }

    public void ReturnToIdle(float duration)
    {
        spriteRenderer.color = Color.white;
        // OnComplete(StartFloating) yüzünden sorun çýkýyordu.
        // StopEverything() bu zinciri kýracak.
        transform.DOMove(initialPosition, duration).SetEase(Ease.InOutSine)
            .OnComplete(StartFloating);

        transform.rotation = Quaternion.identity;
    }

    public void SweepMove(float targetX, float duration)
    {
        StopFloating();
        spriteRenderer.color = Color.white;

        // Süpürme boyunca hasar AÇIK
        isDamaging = true;

        transform.DOMoveX(targetX, duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            // Hareket bitince hasar KAPALI
            isDamaging = false;
        });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ARTIK SADECE "isDamaging" TRUE ÝSE HASAR VERÝYORUZ
        if (isDamaging && other.CompareTag("Player"))
        {
            Debug.Log("EL OYUNCUYA VURDU (Attack Mode)!");

            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) { }
}
