using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance; // Singleton referansý

    [Header("UI Referanslarý")]
    public GameObject pauseMenuUI; // Panel objesini buraya sürükle

    private InputSystem_Actions inputActions;
    private bool isPaused = false;

    void Awake()
    {
        // --- SINGLETON & DONTDESTROYONLOAD ---
        // Bu objeden sahnede baþka var mý?
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Büyü burada: Sahne deðiþince beni yok etme!
        }
        else
        {
            // Eðer zaten bir PauseManager varsa (örn: Main Menu'ye geri döndün),
            // yeni oluþan bu kopyayý yok et.
            Destroy(gameObject);
            return;
        }

        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        // UI Action Map'ini dinle
        inputActions.UI.Enable();
        inputActions.UI.Pause.performed += context => TogglePause();
    }

    void OnDisable()
    {
        // Script kapanýrsa aboneliði kaldýr (Hata vermemesi için)
        if (inputActions != null)
        {
            inputActions.UI.Pause.performed -= context => TogglePause();
            inputActions.UI.Disable();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false); // Menüyü kapat
        Time.timeScale = 1f; // Zamaný normal akýt
        isPaused = false;

        // Player inputlarýný tekrar açmamýz gerekebilir (Opsiyonel)
        // Cursor'ý kilitle (FPS/TPS oyunuysa)
        // Cursor.lockState = CursorLockMode.Locked; 
    }

    void PauseGame()
    {
        pauseMenuUI.SetActive(true); // Menüyü aç
        Time.timeScale = 0f; // Zamaný durdur (Fizik, hareket her þey durur)
        isPaused = true;

        // Cursor'ý serbest býrak ki butona basabilelim
        // Cursor.lockState = CursorLockMode.None;
    }

    // Butonlar Ýçin Fonksiyonlar
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Menüye dönerken zamaný düzeltmeyi unutma!
        isPaused = false;
        pauseMenuUI.SetActive(false);
        SceneManager.LoadScene("MainMenu"); // Sahne adýný buraya yaz
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan Çýkýldý.");
        Application.Quit();
    }
}
