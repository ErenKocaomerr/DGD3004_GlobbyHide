using UnityEngine;

public class BasketReceiver : MonoBehaviour
{
    [HideInInspector] // Inspector'da görünmesine gerek yok, Manager ayarlýyor
    public ToyType currentTargetType;

    [Header("--- Ses Efektleri (YENÝ) ---")]
    public AudioClip correctSFX; // Doðru yakalama sesi
    public AudioClip wrongSFX;   // Yanlýþ yakalama sesi
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ToyData toy = other.GetComponent<ToyData>();

        if (toy != null)
        {
            // Doðru oyuncaðý mý yakaladýk?
            if (toy.toyType == currentTargetType)
            {
                // DOÐRU: +1 Puan
                MiniGameManager.instance.AddScore(1);
                PlaySFX(correctSFX);

                // Ýstersen burada bir "Ding" sesi çalabilirsin
            }
            else
            {
                // YANLIÞ: -1 Puan (Ceza vermek istiyorsan)
                // Ýstemiyorsan bu satýrý silebilirsin.
                MiniGameManager.instance.AddScore(-1);
                PlaySFX(wrongSFX);
            }

            // Yakalanan oyuncaðý yok et
            Destroy(other.gameObject);
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            // Pitch (Perde) ile hafif oynayarak sesi doðallaþtýr
            audioSource.PlayOneShot(clip);
        }
    }
}
