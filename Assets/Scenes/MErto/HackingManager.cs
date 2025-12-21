using TMPro;
using UnityEngine;

public class HackingManager : MonoBehaviour
{
    [Header("--- Ayarlar ---")]
    public float timeLimit = 15f;
    public GameObject hackingPanel; // Tüm minigame'in içinde olduðu ana panel
    public HackingPlayer hackingPlayer;
    public HackingNode startNode;

    [Header("--- UI Panelleri (YENÝ) ---")]
    public GameObject successPanel; // Kazanýnca açýlacak panel
    public GameObject failPanel;    // Kaybedince açýlacak panel

    [Header("--- UI Text ---")]
    public TMP_Text timerText;
    public TMP_Text statusText;

    [HideInInspector] public bool isGameOver = false;
    private float currentTime;

    void OnEnable()
    {
        // Minigame açýlýnca oyunu sýfýrla
        ResetGame();
    }

    // Oyunu baþtan baþlatan fonksiyon
    // Fail Panelindeki "Try Again" butonuna bunu baðlayacaksýn.
    public void RetryMinigame()
    {
        ResetGame();
    }

    void ResetGame()
    {
        isGameOver = false;
        currentTime = timeLimit;

        // Textleri ayarla
        if (statusText)
        {
            statusText.text = "SYSTEM BREACHING...";
            statusText.color = Color.yellow;
        }

        // Panelleri gizle
        if (successPanel) successPanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);

        // --- YENÝ KISIM: TÜM NODE'LARI SIFIRLA ---
        // Bu panelin altýndaki (çocuðu olan) tüm HackingNode scriptlerini bul
        HackingNode[] allNodes = GetComponentsInChildren<HackingNode>();
        foreach (var node in allNodes)
        {
            node.ResetNode();
        }
        // ------------------------------------------

        // Player'ý baþlangýç noktasýna koy ve resetle
        if (startNode != null) hackingPlayer.Setup(startNode);
    }

    void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;

        if (timerText) timerText.text = currentTime.ToString("F1");

        if (currentTime <= 0)
        {
            LoseGame();
        }
    }

    public void WinGame()
    {
        if (isGameOver) return; // Zaten bittiyse tekrar tetiklenmesin
        isGameOver = true;

        if (statusText)
        {
            statusText.text = "ACCESS GRANTED";
            statusText.color = Color.green;
        }

        Debug.Log("HACK BAÞARILI!");

        // Success Paneli Aç
        if (successPanel) successPanel.SetActive(true);

        // NOT: Artýk otomatik kapanmýyor, Success Panelindeki butona basýnca kapanacak.
    }

    public void LoseGame()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (statusText)
        {
            statusText.text = "ACCESS DENIED";
            statusText.color = Color.red;
        }

        Debug.Log("HACK BAÞARISIZ!");

        // Fail Paneli Aç
        if (failPanel) failPanel.SetActive(true);
    }

    // Minigame'i tamamen kapatýp ana oyuna dönen fonksiyon
    // Success Panelindeki "Continue" butonuna bunu baðla.
    // Fail Panelindeki "Exit" butonuna da baðlayabilirsin.
    public void CloseMinigame()
    {
        hackingPanel.SetActive(false);
        // Ana oyundaki kapýyý açma kodunu buraya veya WinGame içine ekleyebilirsin
        // Örn: GameManager.instance.UnlockDoor();

        Time.timeScale = 1f; // Oyunu durdurduysan devam ettir
    }

    // Ana oyundan etkileþime girince çaðrýlacak
    public void Interact()
    {
        hackingPanel.SetActive(true);
        Time.timeScale = 0; // Oyunu durdur
    }
}
