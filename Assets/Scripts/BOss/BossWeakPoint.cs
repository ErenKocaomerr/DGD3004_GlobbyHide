using System.Collections;
using UnityEngine;

public class BossWeakPoint : MonoBehaviour
{
    [Header("--- Ayarlar ---")]
    public BossBrain bossBrain;
    public float bounceForce = 12f;

    [Header("--- Görsel ---")]
    public Animator balloonAnimator; // Balonun Animator'ý
    public AudioClip popSFX;         // Patlama sesi
    private AudioSource audioSource;
    private Collider2D col;

    public float popAnimationDuration = 0.5f;
    public bool isPopped = false;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        if (balloonAnimator == null) balloonAnimator = GetComponent<Animator>();
    }

    // BossBrain tarafýndan çaðrýlacak: Balonu sýfýrla ve aç
    public void ResetBalloon()
    {
        if (isPopped) return;

        gameObject.SetActive(true);
        col.enabled = true; // Collider'ý aç
        if (balloonAnimator) balloonAnimator.Play("Idle"); // Normal duruþa geç
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPopped) return;

        if (other.CompareTag("Player"))
        {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

            // Mario Kuralý: Oyuncu aþaðý düþüyorsa (Balona basýyorsa)
            if (playerRb != null)
            {
                // 1. Sesi Çal
                if (popSFX && audioSource) audioSource.PlayOneShot(popSFX);

                // 2. Oyuncuyu zýplat
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);

                // Double Jump Yenile
                AdvancedPlayerController pc = other.GetComponent<AdvancedPlayerController>();
                if (pc != null) pc.unlockDoubleJump = true;

                // 3. Patlama Ýþlemleri
                PopBalloon();

                if (this.gameObject != null) 
                {
                    StartCoroutine(HideAfterAnimation());
                }
            }
        }
    }

    void PopBalloon()
    {
        isPopped = true;
        col.enabled = false; // Artýk çarpýlamaz

        // Animasyonu Oynat (Animator'da "Pop" isminde bir trigger veya state olmalý)
        if (balloonAnimator) balloonAnimator.SetTrigger("Pop");

        // Boss'a "Ben vuruldum, hasar al!" de
        bossBrain.OnBalloonPopped();
    }

    IEnumerator HideAfterAnimation()
    {
        // Animasyonun süresi kadar bekle (Inspector'dan ayarla)
        yield return new WaitForSeconds(popAnimationDuration);

        // Objeyi tamamen gizle
        gameObject.SetActive(false);
    }
}
