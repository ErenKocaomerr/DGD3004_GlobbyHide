using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager instance; // Diðer scriptlerden eriþim için Singleton

    [Header("--- Oyun Ayarlarý ---")]
    public BasketReceiver playerBasket;
    public TMP_Text taskText;   // "Þunu Topla" yazýsý
    public TMP_Text scoreText;  // "Puan: 0" yazýsý
    public GameObject winPanel; // Kazandýn Paneli (Baþlangýçta kapalý olacak)
    public GameObject startPanel;

    public int scoreToWin = 10; // Kaç puanda kazansýn?
    public float targetChangeInterval = 10f; // Hedef deðiþim süresi

    private int currentScore = 0;
    private float timer;
    private bool isGameOver = false;

    void Awake()
    {
        // Singleton Kurulumu
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentScore = 0;
        UpdateScoreUI();
        winPanel.SetActive(false); // Kazanma ekranýný gizle
        ChangeTarget(); // Ýlk hedefi belirle
        startPanel.SetActive(true);
        Time.timeScale = 0f; // Oyunu Durdur
    }

    public void StartGameButton()
    {
        startPanel.SetActive(false); // Paneli kapat
        Time.timeScale = 1f; // Oyunu baþlat
    }

    void Update()
    {
        if (isGameOver) return; // Oyun bittiyse update çalýþmasýn

        timer += Time.deltaTime;
        if (timer >= targetChangeInterval)
        {
            ChangeTarget();
            timer = 0;
        }
    }

    // --- PUAN EKLEME FONKSÝYONU ---
    // Bu fonksiyonu Sepet (BasketReceiver) çaðýracak
    public void AddScore(int amount)
    {
        if (isGameOver) return;

        currentScore += amount;

        // Puan eksiye düþmesin (Ýsteðe baðlý)
        if (currentScore < 0) currentScore = 0;

        UpdateScoreUI();
        CheckWinCondition();
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score: " + currentScore + " / " + scoreToWin;
    }

    void CheckWinCondition()
    {
        if (currentScore >= scoreToWin)
        {
            WinGame();
        }
    }

    // --- KAZANMA FONKSÝYONU ---
    void WinGame()
    {
        isGameOver = true;
        Debug.Log("OYUN KAZANILDI!");

        // 1. Kazanma Panelini Aç
        winPanel.SetActive(true);

        // 2. Oyunu Durdur (Oyuncaklar havada donsun)
        Time.timeScale = 0f;
    }

    public void TurnTown()
    {
        Time.timeScale = 1f;
        GameManager.instance.UnlockAbility("Dash");
        GameManager.instance.isReturningToHub = true;
        SceneManager.LoadScene("NewHub");
    }

    // --- YARDIMCI FONKSÝYONLAR ---
    void ChangeTarget()
    {
        ToyType[] allTypes = (ToyType[])System.Enum.GetValues(typeof(ToyType));
        ToyType newTarget = allTypes[Random.Range(0, allTypes.Length)];

        playerBasket.currentTargetType = newTarget;
        taskText.text = "Mission: " + newTarget.ToString().ToUpper() + " Catch!";
    }
}
