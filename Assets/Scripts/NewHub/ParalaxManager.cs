using System.Collections.Generic;
using UnityEngine;

public class ParalaxManager : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        [Header("Katman Ayarlarý")]
        public Transform layerObject; // Hareket edecek obje

        [Range(-1f, 1f)]
        [Tooltip("1 = Gökyüzü (Kamerayla gider), 0 = Normal Zemin, Eksi deðer = Ön Plan")]
        public float parallaxFactorX; // Yatay hýz çarpaný

        [Range(0f, 1f)]
        public float parallaxFactorY; // Dikey hýz çarpaný (Opsiyonel)

        public bool infiniteHorizontal; // Sonsuz döngü olsun mu?

        // Gizli deðiþkenler (Script kendi hesaplayacak)
        [HideInInspector] public Vector3 startPos;
        [HideInInspector] public float lengthX;
    }

    [Header("Genel Ayarlar")]
    public Transform mainCamera; // Kamera referansý
    public List<ParallaxLayer> parallaxLayers; // Inspector'da dolduracaðýmýz liste

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main.transform;

        // Tüm katmanlarýn baþlangýç pozisyonlarýný ve boyutlarýný kaydet
        foreach (var layer in parallaxLayers)
        {
            if (layer.layerObject != null)
            {
                layer.startPos = layer.layerObject.position;

                // Eðer sonsuz döngü açýksa resmin geniþliðini bul
                if (layer.infiniteHorizontal)
                {
                    SpriteRenderer sr = layer.layerObject.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        layer.lengthX = sr.bounds.size.x;
                    }
                    else
                    {
                        Debug.LogWarning($"'{layer.layerObject.name}' objesinde SpriteRenderer yok, sonsuz döngü çalýþmayabilir!");
                    }
                }
            }
        }
    }

    void LateUpdate()
    {
        foreach (var layer in parallaxLayers)
        {
            if (layer.layerObject == null) continue;

            // 1. Parallax Hareketi Hesaplama
            float distX = (mainCamera.position.x * layer.parallaxFactorX);
            float distY = (mainCamera.position.y * layer.parallaxFactorY);

            // Yeni pozisyonu uygula
            layer.layerObject.position = new Vector3(layer.startPos.x + distX, layer.startPos.y + distY, layer.layerObject.position.z);

            // 2. Sonsuz Döngü (Infinite Scrolling) Kontrolü
            if (layer.infiniteHorizontal && layer.lengthX > 0)
            {
                // Kameranýn katmana göre "gerçek" ilerlemesi
                float tempX = (mainCamera.position.x * (1 - layer.parallaxFactorX));

                // Saða doðru sýnýr geçildi mi?
                if (tempX > layer.startPos.x + layer.lengthX)
                {
                    layer.startPos.x += layer.lengthX;
                }
                // Sola doðru sýnýr geçildi mi?
                else if (tempX < layer.startPos.x - layer.lengthX)
                {
                    layer.startPos.x -= layer.lengthX;
                }
            }
        }
    }
}
