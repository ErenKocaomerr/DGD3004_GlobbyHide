using DG.Tweening;
using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    public enum AbilityType
    {
        Dash,
        WallJump,
        DoubleJump // Ýleride eklersen diye
    }

    [Header("Ayarlar")]
    public AbilityType abilityToUnlock; // Inspector'dan seçilecek (Dash mi WallJump mý?)
    public GameObject pickupEffect; // Toplayýnca çýkacak partikül (Opsiyonel)

    [Header("Animasyon")]
    public float floatSpeed = 1f;
    public float floatHeight = 0.5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        // Obje yerinde dursun ama hafifçe yukarý aþaðý süzülsün (Idle Animasyon)
        transform.DOMoveY(startPos.y + floatHeight, 1f / floatSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UnlockAbility(collision.GetComponent<AdvancedPlayerController>());
        }
    }

    private void UnlockAbility(AdvancedPlayerController player)
    {
        if (player == null) return;

        // Seçilen yeteneðe göre Player'daki bool'u true yap
        switch (abilityToUnlock)
        {
            case AbilityType.Dash:
                player.unlockDash = true;
                Debug.Log("DASH YETENEÐÝ KAZANILDI!");
                // Buraya UI'da "Dash Kazandýn!" yazýsý kodu eklenebilir.
                break;

            case AbilityType.WallJump:
                player.unlockWallJump = true;
                Debug.Log("DUVARDAN ZIPLAMA KAZANILDI!");
                break;
        }

        // Görsel Efekt Patlat
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        // Obje yok olsun (Ses çalarak yok olmak istersen Destroy'u geciktirebilirsin)
        Destroy(gameObject);
    }
}
