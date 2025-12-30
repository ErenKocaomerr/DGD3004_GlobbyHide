using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{

    [Header("--- Animasyon Ayarlarý ---")]
    public Animator storyAnimator; // Hikaye animasyonunun olduðu Animator
    private bool isPaused = false; // Þu an bekleme modunda mýyýz?

    [Header("--- Ses ve Diðerleri ---")]
    public AudioSource audioSource;
    public AudioClip speakClip;

    void Start()
    {
        // Eðer Inspector'dan atamazsan, ayný objedeki Animator'ý al
        if (storyAnimator == null)
            storyAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // Eðer animasyon duraklatýldýysa VE oyuncu sol týkladýysa (veya ekrana dokunduysa)
        if (isPaused && Input.GetMouseButtonDown(0))
        {
            ResumeStory();
        }
    }

    // --- BU FONKSÝYONU ANIMATION EVENT ÝLE ÇAÐIRACAKSIN ---
    public void PauseStory()
    {
        isPaused = true;
        storyAnimator.speed = 0f; // Animasyon hýzýný 0 yap (Dondur)
    }

    // Týklayýnca çalýþacak olan devam ettirme fonksiyonu
    private void ResumeStory()
    {
        isPaused = false;
        storyAnimator.speed = 1f; // Animasyon hýzýný normale döndür (Devam et)
    }

    // --- ESKÝ FONKSÝYONLARIN ---
    public void GoHub()
    {
        SceneManager.LoadScene("NewHub");
    }

    public void Speak()
    {
        if (audioSource && speakClip)
            audioSource.PlayOneShot(speakClip);
    }
}
