using UnityEngine;

public class UIMapScroller : MonoBehaviour
{
    [Header("--- Referanslar ---")]
    public RectTransform playerRect;   // Player_Cursor
    public RectTransform mapContent;   // Map_Content (Hareket edecek olan dev harita)

    [Header("--- Ayarlar ---")]
    public float smoothSpeed = 10f;

    // Deadzone: Ekranýn ortasýndaki güvenli alan (Piksel cinsinden)
    // X: 100 demek, saða sola 100 piksel gidene kadar harita kaymaz demek.
    public Vector2 deadzone = new Vector2(100f, 100f);

    private Vector2 currentVelocity; // SmoothDamp için gerekli

    void LateUpdate()
    {
        if (playerRect == null || mapContent == null) return;

        // 1. Player'ýn Harita içindeki pozisyonunu (AnchoredPosition) al
        Vector2 playerPos = playerRect.anchoredPosition;

        // 2. Haritanýn þu anki pozisyonu
        Vector2 mapPos = mapContent.anchoredPosition;

        // 3. Hedef: Harita öyle bir yerde olmalý ki, Player ekranýn ortasýna (0,0) denk gelmeli.
        // Bunun formülü: MapPos = -PlayerPos
        // Ancak Deadzone olduðu için direkt eþitlemiyoruz, farka bakýyoruz.

        // Player'ýn "Dünya" üzerindeki gerçek pozisyonu (Map kaymasý dahil)
        // Bu kýsým biraz matematiksel: Player'ýn local pozisyonu + Haritanýn pozisyonu = Ekrandaki yeri
        Vector2 playerScreenPos = playerPos + mapPos;

        Vector2 targetMapPos = mapPos;

        // --- X EKSENÝ DEADZONE ---
        if (playerScreenPos.x > deadzone.x)
        {
            // Saða çok gitti, haritayý sola çek
            targetMapPos.x = -(playerPos.x - deadzone.x);
        }
        else if (playerScreenPos.x < -deadzone.x)
        {
            // Sola çok gitti, haritayý saða çek
            targetMapPos.x = -(playerPos.x + deadzone.x);
        }

        // --- Y EKSENÝ DEADZONE ---
        if (playerScreenPos.y > deadzone.y)
        {
            // Yukarý çok gitti, haritayý aþaðý çek
            targetMapPos.y = -(playerPos.y - deadzone.y);
        }
        else if (playerScreenPos.y < -deadzone.y)
        {
            // Aþaðý çok gitti, haritayý yukarý çek
            targetMapPos.y = -(playerPos.y + deadzone.y);
        }

        // 4. HAREKET (SmoothDamp kullanarak yumuþak geçiþ)
        mapContent.anchoredPosition = Vector2.SmoothDamp(
            mapContent.anchoredPosition,
            targetMapPos,
            ref currentVelocity,
            1f / smoothSpeed
        );
    }

    // Editörde Deadzone kutusunu görelim (Canvas merkezinde)
    private void OnDrawGizmos()
    {
        // Not: Bu gizmo sahne merkezinde görünür, UI üzerinde tam oturmayabilir ama fikir verir.
        Gizmos.color = Color.yellow;
        if (mapContent != null)
        {
            // Basit bir görselleþtirme
            Gizmos.matrix = mapContent.parent.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(deadzone.x * 2, deadzone.y * 2, 0));
        }
    }
}
