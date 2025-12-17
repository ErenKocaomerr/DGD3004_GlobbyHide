using UnityEngine;

public class LightCollide : MonoBehaviour
{
    bool isPlayed = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Target") && !isPlayed)
        {
            SFXManager.Instance.PlayCry();
            isPlayed = true;
        }
    }
}
