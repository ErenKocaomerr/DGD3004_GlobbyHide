using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Ayarlar")]
    public bool isActivated = false; // Görsel deðiþim için (Opsiyonel)

    // Checkpoint alýnca renk deðiþsin veya animasyon girsin istersen:
    // public Sprite activeSprite; 
    // private SpriteRenderer sr;

    private void Start()
    {
        // sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sadece Player çarparsa çalýþsýn
        if (other.CompareTag("Player"))
        {
            // GameManager'a "Burayý kaydet" diyoruz
            GameManager.instance.SetCheckpoint(transform.position);

            if (!isActivated)
            {
                ActivateCheckpoint();
            }
        }
    }

    void ActivateCheckpoint()
    {
        isActivated = true;
        Debug.Log("Checkpoint Aktif!");

        // Görsel deðiþiklik (Örnek: Rengi yeþil yap)
        // if(sr != null) sr.color = Color.green;

        // Ses efekti ekleyebilirsin
        // AudioSource.PlayClipAtPoint(checkpointSound, transform.position);
    }
}
