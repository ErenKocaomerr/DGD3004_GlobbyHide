using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    public SpriteRenderer spriteRenderer;
    public List<Sprite> possibleSprites;
    public int maxHealth = 5;
    public int currentHealth;
    public AudioSource audioSource;
    public AudioClip hitClip;
    public AudioClip missClip;
    public GameObject succesPanel;

    [Header("UI")]
    public Slider healthSlider;
    public TMP_Text healthText; // opsiyonel: can sayýsý göstermek için

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateHealthUI();
    }

    public void OnSuccessfulHit(HitResult result)
    {
        // Perfect/Good için farklý efektler koyabilirsin. Burada her baþarýlý vuruda -1 hp:
        currentHealth = Mathf.Max(0, currentHealth - 1);

        if (hitClip != null && audioSource != null)
            audioSource.PlayOneShot(hitClip);

        ChangeSpriteRandom();
        UpdateHealthUI();

        Debug.Log("Enemy hit! Result: " + result + " HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            OnEnemyDefeated();
        }
    }

    public void OnMissedHit()
    {
        // kaçýrma durumunda düþmene deðil enemy'nin caný artsýn (kullanýcýnýn isteði)
        currentHealth = Mathf.Min(maxHealth, currentHealth + 1);
        if (missClip != null && audioSource != null)
            audioSource.PlayOneShot(missClip);

        Debug.Log("Missed! Enemy healed. HP: " + currentHealth);
        ChangeSpriteRandom();
        UpdateHealthUI();
    }

    void ChangeSpriteRandom()
    {
        if (possibleSprites == null || possibleSprites.Count == 0) return;
        Sprite s = possibleSprites[Random.Range(0, possibleSprites.Count)];
        spriteRenderer.sprite = s;
    }

    IEnumerator Flash() 
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = new Color(205, 205, 205);
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }

    void OnEnemyDefeated()
    {
        Debug.Log("Enemy defeated!");

        succesPanel.SetActive(true);
        audioSource.enabled = false;
        GameManager.instance.UnlockAbility("DoubleJump");
        Time.timeScale = 0f;
    }

    public void TurnTown() 
    {
        Time.timeScale = 1f;
        GameManager.instance.isReturningToHub = true;
        SceneManager.LoadScene("NewHub");
    }
}
