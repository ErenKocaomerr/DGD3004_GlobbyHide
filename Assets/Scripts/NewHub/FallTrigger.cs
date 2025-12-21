using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Çarpan þey Oyuncu mu?
        if (other.CompareTag("Player"))
        {
            // GameManager var mý kontrol et
            if (GameManager.instance != null)
            {
                // 1. Oyuncuyu Iþýnla
                other.transform.position = GameManager.instance.currentCheckpointPos;

                // 2. Oyuncunun düþme hýzýný sýfýrla
                // (Bunu yapmazsak ýþýnlandýðýnda hala aþaðý doðru hýzla düþüyor olur)
                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }

                // Ýstersen burada can da azaltabilirsin:
                // GameManager.instance.TakeDamage(1); 
            }
            else
            {
                Debug.LogError("GameManager Sahnede Bulunamadý!");
            }
        }
        else
        {
            // Eðer düþen þey oyuncu deðilse (örn: düþman, kutu vb.) yok et
            Destroy(other.gameObject);
        }
    }

    // Editörde objeyi görebilmek için kýrmýzý bir çizgi çizelim
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.DrawCube(transform.position, box.size);
        }
    }
}
