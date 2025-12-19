using UnityEngine;
using UnityEngine.SceneManagement;

public class FlyingEnemyAI : MonoBehaviour
{
    [Header("--- Patrol (Devriye) Ayarlarý ---")]
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("--- Algýlama ---")]
    public float visionRange = 7f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask playerLayer;

    [Header("--- Uçuþ & Takip Sýnýrý ---")]
    public float maxChaseDistance = 10f; // Baþlangýçtan ne kadar uzaða gidebilir?
    private Vector3 startPosition; // Düþmanýn doðduðu yer (Merkez)

    private Rigidbody2D rb;
    private Transform currentPatrolTarget;
    private Transform playerTransform;
    private AdvancedPlayerController playerScript;

    // Durumlar
    private bool isChasing = false;
    private bool isReturning = false; // Kovalamayý býrakýp geri dönme modu

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // UÇAN DÜÞMAN ÝÇÝN ÇOK ÖNEMLÝ: Yerçekimini kapat
        rb.gravityScale = 0;

        currentPatrolTarget = pointB;
        startPosition = transform.position; // Baþlangýç noktasýný kaydet
    }

    void Update()
    {
        if (playerTransform == null) FindPlayer();

        // 1. Oyuncu Algýlama Mantýðý
        if (playerTransform != null && !isReturning)
        {
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            float distToStart = Vector2.Distance(transform.position, startPosition);

            // Oyuncu görünür mü ve menzilde mi?
            if (playerScript != null && !playerScript.IsHidden && CanSeePlayer())
            {
                // Ama çok mu uzaklaþtýk? (Tasma Mantýðý)
                if (distToStart < maxChaseDistance)
                {
                    isChasing = true;
                }
                else
                {
                    // Çok uzaklaþtýk, geri dön!
                    isChasing = false;
                    isReturning = true;
                }
            }
            else
            {
                isChasing = false;
            }
        }

        // 2. Hareket Karar Mekanizmasý
        if (isReturning)
        {
            ReturnToStart();
        }
        else if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        // YERDEKÝNDEN FARKLI: Sadece X deðil, direkt hedefe uç (X ve Y)
        transform.position = Vector2.MoveTowards(transform.position, currentPatrolTarget.position, patrolSpeed * Time.deltaTime);

        // Yönü hedefe çevir
        LookAtTarget(currentPatrolTarget.position);

        if (Vector2.Distance(transform.position, currentPatrolTarget.position) < 0.2f)
        {
            currentPatrolTarget = (currentPatrolTarget == pointA) ? pointB : pointA;
        }
    }

    private void ChasePlayer()
    {
        // Oyuncuya direkt uç
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, chaseSpeed * Time.deltaTime);
        LookAtTarget(playerTransform.position);
    }

    private void ReturnToStart()
    {
        // En yakýn patrol noktasýna veya baþlangýç noktasýna dön
        // Burada basitçe PointA'ya dönmesini saðlýyoruz.
        transform.position = Vector2.MoveTowards(transform.position, startPosition, patrolSpeed * Time.deltaTime);
        LookAtTarget(startPosition);

        // Baþlangýca vardýysa normal devriyeye dön
        if (Vector2.Distance(transform.position, startPosition) < 0.2f)
        {
            isReturning = false;
            currentPatrolTarget = pointA; // Kaldýðý yerden devam etsin
        }
    }

    private bool CanSeePlayer()
    {
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distToPlayer > visionRange) return false;

        Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
        Vector2 facingDir = transform.localScale.x > 0 ? Vector2.right : Vector2.left; // Saða mý bakýyor sola mý?

        float angleToPlayer = Vector2.Angle(facingDir, dirToPlayer);

        if (angleToPlayer < viewAngle / 2f)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distToPlayer, playerLayer);
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    private void LookAtTarget(Vector3 target)
    {
        // Hedef saðda mý solda mý?
        if (target.x > transform.position.x)
        {
            // Saða bak (Scale pozitif)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
        else
        {
            // Sola bak (Scale negatif)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { playerTransform = p.transform; playerScript = p.GetComponent<AdvancedPlayerController>(); }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnDrawGizmos()
    {
        // Patrol Noktalarý
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }

        // Görüþ Alaný
        Gizmos.color = isChasing ? Color.red : (isReturning ? Color.blue : Color.yellow);

        Vector3 facingDir = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle / 2f, Vector3.forward);
        Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle / 2f, Vector3.forward);
        Gizmos.DrawRay(transform.position, (leftRayRotation * facingDir) * visionRange);
        Gizmos.DrawRay(transform.position, (rightRayRotation * facingDir) * visionRange);

        // Chase Sýnýrý (Max Distance)
        Gizmos.color = Color.magenta;
        // Start position oyun baþlamadan null olabilir, o yüzden transform.position kullanýyoruz editörde
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, maxChaseDistance);
    }
}
