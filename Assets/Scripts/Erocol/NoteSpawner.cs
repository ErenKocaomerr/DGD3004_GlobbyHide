using System.Collections;
using TMPro;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public Transform spawnUp, spawnDown, spawnLeft, spawnRight;

    [Header("Spawn timing (seconds)")]
    public float initialSpawnInterval = 1.2f;
    public float minSpawnInterval = 0.35f;
    public float timeToMinInterval = 120f; // kaç saniyede maksimum hýza ulaþsýn

    [Header("Note speed")]
    public float initialNoteSpeed = 2f;
    public float maxNoteSpeed = 8f;
    public float timeToMaxSpeed = 120f;

    float startTime;

    [Header("--- UI ---")]
    public TMP_Text countdownText;

    void Start()
    {
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        // Eðer text atanmamýþsa hata vermesin diye kontrol
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true); // Görünür yap

            countdownText.text = "3";
            // Ýstersen burada bir ses çal: AudioSource.PlayOneShot(beepSound);
            yield return new WaitForSeconds(1f);

            countdownText.text = "2";
            yield return new WaitForSeconds(1f);

            countdownText.text = "1";
            yield return new WaitForSeconds(1f);

            countdownText.text = "BAÞLA!"; // Veya "GO!"
            yield return new WaitForSeconds(0.5f);

            countdownText.gameObject.SetActive(false); // Gizle
        }
        else
        {
            // Eðer text yoksa boþuna 3 saniye beklemesin, hemen baþlasýn (Debug için)
            Debug.LogWarning("Countdown Text atanmadý! Sayaçsýz baþlýyor.");
        }

        // --- OYUN BAÞLIYOR ---

        // Zamaný þimdi baþlatýyoruz ki zorluk seviyesi 0'dan baþlasýn
        startTime = Time.time;

        // Notlarý üretmeye baþla
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float elapsed = Time.time - startTime;
            // spawn interval: lineer olarak initial -> min arasýnda
            float tInterval = Mathf.Clamp01(elapsed / timeToMinInterval);
            float currentInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, tInterval);

            // note speed: initial -> max
            float tSpeed = Mathf.Clamp01(elapsed / timeToMaxSpeed);
            float currentSpeed = Mathf.Lerp(initialNoteSpeed, maxNoteSpeed, tSpeed);

            SpawnRandom(currentSpeed);

            yield return new WaitForSeconds(currentInterval);
        }
    }

    void SpawnRandom(float speed)
    {
        Direction dir = (Direction)Random.Range(0, 4);
        Transform spawnPoint = spawnUp;
        switch (dir)
        {
            case Direction.Up: spawnPoint = spawnUp; break;
            case Direction.Down: spawnPoint = spawnDown; break;
            case Direction.Left: spawnPoint = spawnLeft; break;
            case Direction.Right: spawnPoint = spawnRight; break;
        }

        GameObject go = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity);
        Note note = go.GetComponent<Note>();
        note.direction = dir;
        note.speed = speed;
        note.UpdateSpriteByDirection();
    }
}
