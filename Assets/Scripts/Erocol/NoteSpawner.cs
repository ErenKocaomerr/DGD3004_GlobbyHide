using System.Collections;
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

    void Start()
    {
        startTime = Time.time;
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
