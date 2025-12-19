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

    void Start()
    {
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

        stateText.text = "HAZIR MISIN?";
        timerText.text = "Süre: " + levelTime;

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
        if (levelTime <= 0) GameOver("SÜRE BÝTTÝ!");

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
                    GameOver("KIMILDADIN!");

                if (!playerController.IsHidden)
                    GameOver("SAKLANMADIN! (Göründün)");
            }
        }
    }

    void SwitchToGreen()
    {
        currentState = GameState.Green;
        stateTimer = Random.Range(minGreenTime, maxGreenTime);

        stateText.text = "YEÞÝL IÞIK (KOÞ!)";
        stateText.color = Color.green;

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

        stateText.text = "KIRMIZI IÞIK (SAKLAN!)";
        stateText.color = Color.red;

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
        stateText.text = "KAZANDIN!";

        // Kazanma Panelini Aç
        winPanel.SetActive(true);

        // Oyuncuyu Durdur
        LockPlayer(true);

        // Efekti kapat
        if (vignette != null) vignette.intensity.value = 0f;
    }

    void GameOver(string reason)
    {
        currentState = GameState.GameOver;
        stateText.text = reason;

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

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Kazanma panelinden sonraki level'a geçmek için opsiyonel buton
    public void LoadNextLevel()
    {
        SceneManager.LoadScene("NewHub");
    }
}
