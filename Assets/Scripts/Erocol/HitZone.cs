using System.Collections.Generic;
using UnityEngine;

public class HitZone : MonoBehaviour
{
    // thresholds (y-farký cinsinden)
    [Header("Timing thresholds (y distance)")]
    public float perfectWindow = 0.15f;
    public float goodWindow = 0.35f;
    // (> goodWindow) -> miss

    private List<Note> notesInZone = new List<Note>();

    void OnTriggerEnter2D(Collider2D other)
    {
        Note n = other.GetComponent<Note>();
        if (n != null && !notesInZone.Contains(n))
        {
            notesInZone.Add(n);
            Debug.Log("Note entered zone: " + n.name + " dir: " + n.direction + " count: " + notesInZone.Count);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Note n = other.GetComponent<Note>();
        if (n != null)
        {
            // nota hit olmadan çýkýyorsa bu bir kaçýrma
            if (!n.isHit)
            {
                // Miss because note passed through
                Debug.Log("Note missed by passing: " + n.name);
                if (EnemyManager.Instance != null) EnemyManager.Instance.OnMissedHit();
            }

            notesInZone.Remove(n);
            Debug.Log("Note exited zone. remaining: " + notesInZone.Count);
        }
    }

    // Input handler bu metodu çaðýracak; hangi yön için basýldýðýný ver.
    public HitResult TryHit(Direction dir)
    {
        CleanupNulls();

        Note best = null;
        float bestDist = float.MaxValue;
        float zoneY = transform.position.y;

        for (int i = notesInZone.Count - 1; i >= 0; i--)
        {
            Note n = notesInZone[i];
            if (n == null)
            {
                notesInZone.RemoveAt(i);
                continue;
            }

            if (n.direction != dir) continue;
            float dist = Mathf.Abs(n.transform.position.y - zoneY);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = n;
            }
        }

        if (best == null)
        {
            // hiçbir not yok -> miss (yanlýþ tuþ / çok geç vs.)
            Debug.Log("TryHit: no note found for direction " + dir);
            if (EnemyManager.Instance != null) EnemyManager.Instance.OnMissedHit();
            return HitResult.Miss;
        }

        // ölçüm
        if (bestDist <= perfectWindow)
        {
            best.OnHit();
            notesInZone.Remove(best);
            if (EnemyManager.Instance != null) EnemyManager.Instance.OnSuccessfulHit(HitResult.Perfect);
            return HitResult.Perfect;
        }
        else if (bestDist <= goodWindow)
        {
            best.OnHit();
            notesInZone.Remove(best);
            if (EnemyManager.Instance != null) EnemyManager.Instance.OnSuccessfulHit(HitResult.Good);
            return HitResult.Good;
        }
        else
        {
            // not var ama çok uzak -> miss
            Debug.Log("TryHit: note too far (dist=" + bestDist + ")");
            if (EnemyManager.Instance != null) EnemyManager.Instance.OnMissedHit();
            return HitResult.Miss;
        }
    }

    void CleanupNulls()
    {
        for (int i = notesInZone.Count - 1; i >= 0; i--)
            if (notesInZone[i] == null) notesInZone.RemoveAt(i);
    }
}
