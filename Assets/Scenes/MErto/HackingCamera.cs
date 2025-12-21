using UnityEngine;

public class HackingCamera : MonoBehaviour
{
    [Header("--- Ayarlar ---")]
    public Transform target;       // Takip edilecek obje (HackingPlayer)
    public float smoothSpeed = 5f; // Takip hýzý (Yüksek = Sert, Düþük = Yumuþak)
    public Vector3 offset = new Vector3(0, 0, -10); // Kamera mesafesi (Z = -10 standarttýr)

    [Header("--- Sýnýrlar (Opsiyonel) ---")]
    public bool useLimits = false;
    public Vector2 minLimit; // Sol Alt sýnýr
    public Vector2 maxLimit; // Sað Üst sýnýr

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Hedef pozisyonu belirle
        Vector3 desiredPosition = target.position + offset;

        // 2. Yumuþak geçiþ yap (Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 3. (Opsiyonel) Sýnýrlarý uygula
        if (useLimits)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minLimit.x, maxLimit.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minLimit.y, maxLimit.y);
        }

        // 4. Kamerayý hareket ettir
        transform.position = smoothedPosition;
    }
}
