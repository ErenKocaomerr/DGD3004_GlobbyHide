using UnityEngine;

public class BossWeakPoint : MonoBehaviour
{
    public BossBrain bossBrain; // Beyine haber vereceðiz
    public float bounceForce = 12f; // Oyuncuyu zýplatma gücü

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

            // Mario Kuralý: Oyuncu aþaðý düþüyorsa (Kafaya basýyorsa) hasar ver
            if (playerRb != null)
            {
                // 1. Boss'a hasar ver
                bossBrain.TakeDamage();

                // 2. Oyuncuyu havaya zýplat
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);

                // Varsa Double Jump hakkýný yenile
                AdvancedPlayerController pc = other.GetComponent<AdvancedPlayerController>();
                if (pc != null) pc.unlockDoubleJump = true; // Basit bir hack, istersen public method yap
            }
        }
    }
}
