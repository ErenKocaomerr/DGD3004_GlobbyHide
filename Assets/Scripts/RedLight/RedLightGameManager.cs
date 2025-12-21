using TMPro;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RedLightGameManager : MonoBehaviour
{
    public enum GameState { WaitingToStart, Green, Red, GameOver, Win }
    public GameState currentState;

    [Header("--- Ayarlar ---")]
    public float levelTime = 60f;
    public float movementTolerance = 0.35f;

    [Header("--- Zamanlama ---")]
    public float minGreenTime = 2f;
    public float maxGreenTime = 5f;
    public float minRedTime = 2f;
    public float maxRedTime = 4f;

    [Header("--- Referanslar ---")]
    public AdvancedPlayerController playerController;
    public Rigidbody2D playerRb;
    public Volume globalVolume;

    [Header("--- UI ---")]
    public TMP_Text timerText;
    public TMP_Text stateText;

    public GameObject startPanel;    // YENÝ: Baþlangýç Butonu Paneli
    public GameObject gameOverPanel; // Kaybetme Paneli
    public GameObject winPanel;      // YENÝ: Kazanma Paneli

    private float stateTimer;
    private float toleranceTimer;
    private Vignette vignette;

    public AudioClip greenLightSFX; // "Yeþil Iþýk / Düdük" sesi
    public AudioClip redLightSFX;   // "Kýrmýzý Iþýk / Alarm" sesi
    public AudioClip winSFX;        // Kazanma efekti
    public AudioClip loseSFX;       // Kaybetme efekti
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 1. Vignette Ayarý
        if (globalVolume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = 0f;
        }
        else
        {
            Debug.LogError("HATA: Global Volume içinde 'Vignette' override'ý yok!");
        }

        // 2. Baþlangýç Durumu Ayarlarý
        currentState = GameState.WaitingToStart; // Oyun hemen baþlamasýn

        // Panelleri ayarla
        startPanel.SetActive(true);   // Baþla butonu görünsün
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);

        stateText.text = "Are you Ready?";
        timerText.text = "Time: " + levelTime;

        // 3. OYUNCUYU KÝLÝTLE (Hareket edemesin)
        LockPlayer(true);
    }

    // --- BUTONA BAÐLANACAK FONKSÝYON ---
    public void StartGameButton()
    {
        startPanel.SetActive(false); // Paneli kapat
        LockPlayer(false);           // Oyuncuyu serbest býrak

        SwitchToGreen();             // Oyunu baþlat
    }

    void Update()
    {
        // Oyun baþlamadýysa, bittiyse veya kazanýldýysa Update çalýþmasýn
        if (currentState == GameState.WaitingToStart || currentState == GameState.GameOver || currentState == GameState.Win)
            return;

        // 1. Süre
        levelTime -= Time.deltaTime;
        timerText.text = "Süre: " + Mathf.Ceil(levelTime).ToString();
        if (levelTime <= 0) GameOver("Time is Over!");

        // 2. Iþýk Döngüsü
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            if (currentState == GameState.Green) SwitchToRed();
            else SwitchToGreen();
        }

        // 3. KIRMIZI IÞIK KONTROLÜ
        if (currentState == GameState.Red)
        {
            toleranceTimer -= Time.deltaTime;

            if (toleranceTimer <= 0)
            {
                if (playerRb.linearVelocity.magnitude > 0.1f)
                    GameOver("You Moved ):!");

                if (!playerController.IsHidden)
                    GameOver("You Did not Hide");
            }
        }
    }

    void SwitchToGreen()
    {
        currentState = GameState.Green;
        stateTimer = Random.Range(minGreenTime, maxGreenTime);

        stateText.text = "Run!!!";
        stateText.color = Color.green;

        PlaySFX(greenLightSFX);

        if (vignette != null)
        {
            vignette.color.value = Color.green;
            vignette.intensity.value = 0.3f;
        }
    }

    void SwitchToRed()
    {
        currentState = GameState.Red;
        stateTimer = Random.Range(minRedTime, maxRedTime);
        toleranceTimer = movementTolerance;

        stateText.text = "Hide!!!";
        stateText.color = Color.red;

        PlaySFX(redLightSFX);

        if (vignette != null)
        {
            vignette.color.value = Color.red;
            vignette.intensity.value = 0.55f;
        }
    }

    public void ReachFinishLine()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.Win;
        stateText.text = "Congrats!";

        PlaySFX(winSFX);

        // Kazanma Panelini Aç
        winPanel.SetActive(true);

        // Oyuncuyu Durdur
        LockPlayer(true);

        // Efekti kapat
        if (vignette != null) vignette.intensity.value = 0f;

        Time.timeScale = 0f;
    }

    void GameOver(string reason)
    {
        currentState = GameState.GameOver;
        stateText.text = reason;

        PlaySFX(loseSFX);

        // Kaybetme Panelini Aç
        gameOverPanel.SetActive(true);

        // Oyuncuyu Durdur
        LockPlayer(true);


        // Kýrmýzý/Siyah ekran efekti
        if (vignette != null)
        {
            vignette.color.value = Color.black;
            vignette.intensity.value = 0.7f;
        }

        Time.timeScale = 0f;
    }

    // --- YARDIMCI FONKSÝYON: Oyuncuyu Kilitleme ---
    void LockPlayer(bool isLocked)
    {
        // PlayerController scriptini kapatýrsak input alamaz
        playerController.enabled = !isLocked;

        if (isLocked)
        {
            // Hýzýný sýfýrla ki kaymaya devam etmesin
            playerRb.linearVelocity = Vector2.zero;
            // Animasyonlarý durdurmak istersen buraya ekleyebilirsin
            // Örn: playerController.GetComponent<Animator>().speed = 0;
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }

    public void TurnTown()
    {
        Time.timeScale = 1f;
        GameManager.instance.UnlockAbility("Hide");
        GameManager.instance.isReturningToHub = true;
        SceneManager.LoadScene("NewHub");
    }
}
