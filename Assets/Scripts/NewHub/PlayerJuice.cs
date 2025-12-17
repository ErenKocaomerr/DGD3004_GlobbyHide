using Unity.Cinemachine;
using UnityEngine;
using DG.Tweening;

public class PlayerJuice : MonoBehaviour
{
    [Header("--- Zorunlu Referanslar ---")]
    [Tooltip("Ezip bükeceğimiz görsel obje (SpriteRenderer bunun içinde olmalı)")]
    public Transform visualContainer;
    public SpriteRenderer spriteRenderer;

    [Header("--- Squash & Stretch Ayarları ---")]
    public float jumpSquashX = 0.7f;
    public float jumpSquashY = 1.3f;
    public float landSquashX = 1.4f;
    public float landSquashY = 0.6f;
    public float animDuration = 0.15f;

    [Header("--- Hareket Efektleri ---")]
    public float runTiltAngle = 5f; // Koşarken eğilme açısı
    public float runBobAmount = 0.05f; // Koşarken hafif zıplama (Y ekseni)
    public float runBobSpeed = 10f;

    [Header("--- Dash Efekti ---")]
    public Color dashColor = Color.cyan; // Dash atınca karakter parlasın
    public float dashShakeForce = 1.5f;

    [Header("--- Camera Shake ---")]
    public CinemachineImpulseSource impulseSource;

    private Rigidbody2D rb;
    private Color originalColor;
    private Tween runBobTween;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void Update()
    {
        HandleRunJuice();
    }

    #region Zıplama & İniş (Jump & Land)
    public void PlayJumpEffect()
    {
        visualContainer.DOKill(); // Önceki animasyonları kes
        visualContainer.localScale = Vector3.one;

        // Karakter ince uzun olur (Uzama)
        visualContainer.DOScale(new Vector3(jumpSquashX, jumpSquashY, 1), animDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => ReturnToNormal());
    }

    public void PlayLandEffect(float impactVelocity)
    {
        visualContainer.DOKill();
        visualContainer.localScale = Vector3.one;

        // Karakter basık ve geniş olur (Ezilme)
        visualContainer.DOScale(new Vector3(landSquashX, landSquashY, 1), animDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => ReturnToNormal());

        // Yere sert vurduysa kamera sallansın
        if (Mathf.Abs(impactVelocity) > 15f)
        {
            ShakeCamera(0.5f);
        }
    }
    #endregion

    #region Dash Efekti
    public void PlayDashEffect()
    {
        // 1. Kamera Sarsıntısı
        ShakeCamera(dashShakeForce);

        // 2. Şekil Değiştirme (Hız hissi için arkaya uzama)
        visualContainer.DOKill();
        visualContainer.DOScale(new Vector3(1.4f, 0.6f, 1), 0.1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => ReturnToNormal());

        // 3. Renk Parlaması (Flash)
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
            spriteRenderer.color = dashColor; // Anında renk değişimi
            spriteRenderer.DOColor(originalColor, 0.2f); // Yavaşça eski rengine dön
        }
    }
    #endregion

    #region Yürüme Efekti (Tilt & Bob)
    private void HandleRunJuice()
    {
        // Hareket var mı?
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        // 1. Tilt (Eğilme)
        float targetTilt = 0;
        if (isMoving)
        {
            targetTilt = (rb.linearVelocity.x > 0) ? -runTiltAngle : runTiltAngle;
        }

        // Tilt animasyonu (Lerp ile yumuşak geçiş)
        Quaternion targetRot = Quaternion.Euler(0, 0, targetTilt);
        visualContainer.localRotation = Quaternion.Lerp(visualContainer.localRotation, targetRot, Time.deltaTime * 10f);

        // 2. Bobbing (Yürürken karakterin hafifçe sekmesi)
        if (isMoving)
        {
            if (runBobTween == null || !runBobTween.IsActive())
            {
                // Yukarı aşağı sonsuz döngü (Yoyo)
                runBobTween = visualContainer.DOLocalMoveY(runBobAmount, 1f / runBobSpeed)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        else
        {
            // Durunca pozisyonu sıfırla
            if (runBobTween != null && runBobTween.IsActive())
            {
                runBobTween.Kill();
                visualContainer.DOLocalMoveY(0, 0.1f);
            }
        }
    }
    #endregion

    // Helper: Her animasyon sonrası karakteri normal boyutuna döndür
    private void ReturnToNormal()
    {
        visualContainer.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutSine);
    }

    private void ShakeCamera(float force)
    {
        if (impulseSource != null)
            impulseSource.GenerateImpulse(Vector3.down * force); // Aşağı doğru vuruş hissi
    }
}
