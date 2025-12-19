using System.Collections.Generic;
using UnityEngine;

public class ToySpawner : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject[] toyPrefabs; // Tüm oyuncak türlerini buraya at (Örn: 5 tane)

    public float spawnInterval = 2f;
    public float yHeight = 6f;

    // Ekranýn ne kadar geniþliðine yayýlacaklar?
    // Örn: 14 yaparsan -7 ile +7 arasýna dizer.
    public float totalWidth = 14f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnRow();
            timer = 0;
        }
    }

    void SpawnRow()
    {
        // 1. KARIÞTIRMA (SHUFFLE) ÝÞLEMÝ
        // Mevcut diziden geçici bir liste oluþturuyoruz
        List<GameObject> shuffledToys = new List<GameObject>(toyPrefabs);

        // Fisher-Yates Algoritmasý ile listeyi karýþtýrýyoruz
        for (int i = 0; i < shuffledToys.Count; i++)
        {
            GameObject temp = shuffledToys[i];
            int randomIndex = Random.Range(i, shuffledToys.Count);
            shuffledToys[i] = shuffledToys[randomIndex];
            shuffledToys[randomIndex] = temp;
        }

        // 2. POZÝSYON HESAPLAMA VE YARATMA
        int count = shuffledToys.Count;

        // Her bir oyuncaða düþen alan geniþliði
        float step = totalWidth / count;

        // Ýlk oyuncaðýn baþlayacaðý en sol nokta (Ortalamasý)
        float startX = -(totalWidth / 2) + (step / 2);

        for (int i = 0; i < count; i++)
        {
            // X pozisyonunu hesapla: Baþlangýç + (Sýra * Adým)
            float posX = startX + (i * step);

            Vector3 spawnPos = new Vector3(posX, yHeight, 0);

            Instantiate(shuffledToys[i], spawnPos, Quaternion.identity);
        }
    }

    // Editörde objelerin nereye düþeceðini görmek için çizgiler
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(-totalWidth / 2, yHeight, 0), new Vector3(totalWidth / 2, yHeight, 0));

        if (toyPrefabs.Length > 0)
        {
            float step = totalWidth / toyPrefabs.Length;
            float startX = -(totalWidth / 2) + (step / 2);

            for (int i = 0; i < toyPrefabs.Length; i++)
            {
                float posX = startX + (i * step);
                Gizmos.DrawWireSphere(new Vector3(posX, yHeight, 0), 0.3f);
            }
        }
    }
}
