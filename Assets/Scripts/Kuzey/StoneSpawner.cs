using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSpawner : MonoBehaviour
{
    [Header("Prefab & settings")]
    public GameObject stonePrefab;
    public Transform spawnParent;
    public float spawnIntervalMin = 0.35f;
    public float spawnIntervalMax = 0.9f;

    [Header("Spawn area (world space)")]
    public float spawnY = 10f;
    public float minX = -6f;
    public float maxX = 6f;

    [Header("Size & physics")]
    public float minSize = 0.4f;
    public float maxSize = 1.8f;
    public float minGravity = 0.8f;
    public float maxGravity = 1.8f;

    [Header("Shake tuning (per spawn)")]
    public float baseShakeAmplitude = 0.15f;
    public float sizeToShakeMultiplier = 0.6f;
    public float shakeFrequency = 2.5f;
    public float shakeDuration = 0.12f;

    List<GameObject> spawned = new List<GameObject>();
    Coroutine spawnRoutine = null;

    public RoundManager roundManagerRef; // optional: inspector assign
    public bool autoRegisterToRoundManager = true;

    void Start()
    {
        if (autoRegisterToRoundManager && RoundManager.Instance != null)
        {
            RoundManager.Instance.RegisterSpawner(this);
            roundManagerRef = RoundManager.Instance;
        }
    }

    public void StartSpawning()
    {
        if (stonePrefab == null) { Debug.LogWarning("StoneSpawner: prefab null"); return; }
        if (spawnRoutine == null) spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float wait = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(wait);

            float x = Random.Range(minX, maxX);
            Vector3 pos = new Vector3(x, spawnY, 0f);
            var go = Instantiate(stonePrefab, pos, Quaternion.identity, spawnParent);
            spawned.Add(go);

            float size = Random.Range(minSize, maxSize);
            float grav = Random.Range(minGravity, maxGravity);
            var stone = go.GetComponent<Stone>();
            if (stone != null)
            {
                stone.Init(size, grav);
                stone.roundManager = roundManagerRef != null ? roundManagerRef : RoundManager.Instance;
            }

            // Camera shake scaled by stone size
            float amplitude = baseShakeAmplitude + (size - minSize) * sizeToShakeMultiplier;
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake();
        }
    }

    public void ClearAll()
    {
        StopSpawning();
        foreach (var s in spawned) if (s != null) Destroy(s);
        spawned.Clear();
    }
}
