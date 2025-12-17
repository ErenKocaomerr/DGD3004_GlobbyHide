using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeTarget : MonoBehaviour
{
    [Header("Escape points (in order)")]
    public List<Transform> escapePoints = new List<Transform>();

    [Header("Flee settings")]
    public float detectRadius = 3f;     // player yaklaþýnca tetikleme
    public float fleeSpeed = 3.5f;
    public float arriveThreshold = 0.15f;
    public float postArriveDelay = 0.25f; // bir noktaya varýnca kýsa bekleme (atlamalarý önler)

    [Header("References (auto-find if null)")]
    public Transform playerTransform;

    // durum
    int currentEscapeIndex = 0; // hangi escape point'e gidilecek (0-based)
    bool isFleeing = false;
    public bool isCatchable = false;

    Rigidbody2D rb;
    Coroutine arriveCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        // güvenlik: index'i sýnýrla
        ClampCurrentIndex();
    }

    void Update()
    {
        if (isCatchable) return; // yakalanabilirse hedefine koþma

        if (playerTransform != null)
        {
            float distToPlayer = Vector2.Distance(playerTransform.position, transform.position);

            // Sadece eðer hâlihazýrda kaçmýyorsa ve bir sonraki point varsa tetikle
            if (!isFleeing && distToPlayer <= detectRadius && HasNextEscape())
            {
                // Yalnýzca sýradaki escape point aktif olur — baþka bir escape point
                // ile ilgili kontrol burada yapýlmaz; bu hedef sadece currentEscapeIndex ile hareket eder.
                StartFleeToNext();
            }
        }

        if (isFleeing)
            MoveTowardsEscapePoint();
    }

    bool HasNextEscape()
    {
        return currentEscapeIndex >= 0 && currentEscapeIndex < escapePoints.Count;
    }

    void ClampCurrentIndex()
    {
        if (escapePoints == null) escapePoints = new List<Transform>();
        if (currentEscapeIndex < 0) currentEscapeIndex = 0;
        if (currentEscapeIndex > escapePoints.Count) currentEscapeIndex = escapePoints.Count;
    }

    void StartFleeToNext()
    {
        if (!HasNextEscape()) return;
        if (isFleeing) return;

        // Güvenlik: hedef transform null mý kontrol et
        if (escapePoints[currentEscapeIndex] == null)
        {
            Debug.LogWarning($"FleeTarget {name}: escapePoints[{currentEscapeIndex}] is null. Skipping to next.");
            currentEscapeIndex++;
            ClampCurrentIndex();
            return;
        }

        isFleeing = true;
        // Opsiyonel ses/efekt
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayCry();

        // Görsel dönüþ vb. eklenebilir
    }

    void MoveTowardsEscapePoint()
    {
        if (!HasNextEscape())
        {
            // kaçacak yer kalmadý -> artýk catchable
            isFleeing = false;
            isCatchable = true;
            LevelManager.Instance?.OnTargetFinalReached(this);
            return;
        }

        Transform target = escapePoints[currentEscapeIndex];
        if (target == null)
        {
            // güvenlik: eðer nullsa atla ve index'i arttýr
            currentEscapeIndex++;
            ClampCurrentIndex();
            isFleeing = false;
            return;
        }

        Vector2 pos = rb.position;
        Vector2 targetPos = target.position;
        Vector2 dir = (targetPos - pos).normalized;
        Vector2 newPos = pos + dir * fleeSpeed * Time.deltaTime;
        rb.MovePosition(newPos);

        // rotation to face movement (opsiyonel)
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rb.SetRotation(angle - 90f); // sprite yönüne göre ayarla; sprite'ýnýn forward eksenine bak
        }

        // hedefine ulaþtý mý?
        if (Vector2.Distance(newPos, targetPos) <= arriveThreshold)
        {
            // Noktaya ulaþtý — önce fleeing'i durdur, sonra index'i ilerlet.
            isFleeing = false;

            // index'i arttýr: böylece sadece gerçekten noktaya geldikten sonra bir sonraki aktifleþtirilir
            currentEscapeIndex++;
            ClampCurrentIndex();

            // Eðer daha fazla escape point yoksa finalReached olarak iþaretle
            if (!HasNextEscape())
            {
                isCatchable = true;
                LevelManager.Instance?.OnTargetFinalReached(this);
            }
            else
            {
                // Bir sonraki sýradaki kaçýþ noktasý hazýr ama hemen tekrar tetiklenmemesi için küçük gecikme koy
                if (arriveCoroutine != null) StopCoroutine(arriveCoroutine);
                arriveCoroutine = StartCoroutine(PostArriveCooldown());
            }
        }
    }

    IEnumerator PostArriveCooldown()
    {
        // Bu bekleme süresi sýrasýnda player tekrar detection içinde olsa bile
        // StartFleeToNext() isFleeing kontrolüyle birlikte çalýþarak atlamalarý engeller.
        yield return new WaitForSeconds(postArriveDelay);
        arriveCoroutine = null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCatchable) return;
        if (other.CompareTag("Player"))
        {
            // Yakalandý
            LevelManager.Instance?.OnTargetCaught(this);
            gameObject.SetActive(false);
        }
    }

    // Debug: draw detect circle and points
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
        Gizmos.color = Color.green;
        for (int i = 0; i < escapePoints.Count; i++)
        {
            if (escapePoints[i] != null)
            {
                Gizmos.DrawSphere(escapePoints[i].position, 0.08f);
                // sýrayý da yazdýrmak istersen:
#if UNITY_EDITOR
                UnityEditor.Handles.Label(escapePoints[i].position + Vector3.up * 0.12f, i.ToString());
#endif
            }
        }
    }

    // Ýhtiyaç olursa dýþarýdan force ile sýradaki noktaya gitmesini saðlayan metod
    public void ForceGoToNext()
    {
        if (!HasNextEscape()) return;
        if (isFleeing) return;
        StartFleeToNext();
    }

    // Reset atýlmasý gerekirse
    public void ResetFlee()
    {
        currentEscapeIndex = 0;
        isFleeing = false;
        isCatchable = false;
        if (arriveCoroutine != null) { StopCoroutine(arriveCoroutine); arriveCoroutine = null; }
    }
}
