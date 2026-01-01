using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("--- Can Ayarlarý ---")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("--- Ölümsüzlük (Invincibility) ---")]
    public float invincibilityDuration = 1.5f;
    public float flickerInterval = 0.1f; // Yanýp sönme hýzý
    private bool isInvincible = false;

    [Header("--- UI Referanslarý ---")]
    public Image[] heartImages; // Canvas'taki 3 kalp resmini buraya sürükle
    public Sprite fullHeart;    // Opsiyonel: Dolu kalp resmi
    public Sprite emptyHeart;   // Opsiyonel: Boþ kalp resmi
    public GameObject PlayerVisual;

    [Header("--- Ses Efektleri (YENÝ) ---")]
    public AudioClip hurtSFX;
    public AudioClip deathSFX;
    public AudioClip laughSfx;
    public AudioSource audioSource;

    // Bileþenler
    private SpriteRenderer spriteRenderer;
    private AdvancedPlayerController playerMovement; // Ölünce hareketi kitlemek için

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerMovement = GetComponent<AdvancedPlayerController>();

        UpdateHealthUI();
    }

    // --- HASAR ALMA FONKSÝYONU ---
    // Düþmanlar artýk burayý çaðýracak
    public void TakeDamage(int damage)
    {
        // Ölümsüzsek veya Gizliysek (Stealth) hasar alma
        // PlayerController'dan Gizlilik durumunu okuyoruz
        if (isInvincible || (playerMovement != null && playerMovement.IsHidden)) return;

        currentHealth -= damage;
        Debug.Log($"Can Kaldý: {currentHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
            PlaySFX(deathSFX);
        }
        else
        {
            PlaySFX(hurtSFX);
            StartCoroutine(InvincibilityRoutine());
        }
    }

    // --- UI GÜNCELLEME ---
    private void UpdateHealthUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHealth)
            {
                // Canýmýz var
                heartImages[i].enabled = true;
                if (fullHeart != null) heartImages[i].sprite = fullHeart;
            }
            else
            {
                // Canýmýz yok
                // Ýstersen tamamen gizle:
                heartImages[i].enabled = false;

                // Ýstersen "Boþ Kalp" resmi koy:
                if (emptyHeart != null)
                {
                    heartImages[i].enabled = true;
                    heartImages[i].sprite = emptyHeart;
                }
            }
        }
    }

    // --- ÖLÜMSÜZLÜK EFEKTÝ ---
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // Süre bitene kadar yanýp sön
        for (float i = 0; i < invincibilityDuration; i += flickerInterval)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flickerInterval);
        }

        spriteRenderer.enabled = true; // Garantilemek için görünür yap
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("OYUNCU ÖLDÜ!");

        // Hareketi kilitle (PlayerController'ý kapat)
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

        // Animasyon oynatabilirsin...
        StartCoroutine(DieCorutine());

    }

    private IEnumerator DieCorutine() 
    {
        PlayerVisual.SetActive(false);
        audioSource.PlayOneShot(laughSfx);
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
