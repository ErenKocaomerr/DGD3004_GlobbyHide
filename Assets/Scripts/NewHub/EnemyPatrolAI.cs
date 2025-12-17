using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyPatrolAI : MonoBehaviour
{
    [Header("--- Patrol Ayarlarý ---")]
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("--- Algýlama ---")]
    public float visionRange = 5f; // Oyuncuyu görme mesafesi
    [Range(0, 360)] public float viewAngle = 45f; // Görüþ açýsý (Geniþlik)
    public LayerMask playerLayer; // Player layer'ý seçilecek

    private Rigidbody2D rb;
    private Transform currentPatrolTarget;
    private Transform playerTransform;
    private AdvancedPlayerController playerScript;
    private bool isChasing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentPatrolTarget = pointB; // Ýlk hedef B noktasý olsun
    }

    void Update()
    {
        if (playerTransform == null) FindPlayer();

        if (playerTransform != null)
        {
            // Player gizli deðilse kontrole baþla
            if (playerScript != null && !playerScript.IsHidden)
            {
                if (CanSeePlayer())
                {
                    isChasing = true;
                }
                else
                {
                    // Oyuncu menzilden çýktýysa veya arkasýna geçtiyse takibi býrak
                    // (Ýsteðe baðlý: Takibi hemen býrakmasýn, son görülen yere gitsin eklenebilir)
                    isChasing = false;
                }
            }
            else
            {
                // Player gizliyse (Stealth) görmezden gel
                isChasing = false;
            }
        }

        if (isChasing) ChasePlayer();
        else Patrol();
    }

    private bool CanSeePlayer()
    {
        // 1. Mesafe Kontrolü
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distToPlayer > visionRange) return false;

        // 2. Açý Kontrolü
        Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;

        // Enemy ne tarafa bakýyor? (Scale.x pozitifse saða, negatifse sola)
        Vector2 facingDir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        // Baktýðýmýz yön ile player arasýndaki açý farký
        float angleToPlayer = Vector2.Angle(facingDir, dirToPlayer);

        // Eðer açý görüþ açýmýzýn yarýsýndan küçükse (örn 45 derecenin yarýsý 22.5 sað, 22.5 sol)
        if (angleToPlayer < viewAngle / 2f)
        {
            // 3. Duvar Kontrolü (Raycast)
            // Player ile aramýzda duvar var mý?
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distToPlayer, playerLayer);

            // Eðer ýþýn Player'a çarptýysa (arada engel yoksa)
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            playerTransform = p.transform;
            playerScript = p.GetComponent<AdvancedPlayerController>();
        }
    }

    private void Patrol()
    {
        // Hedefe doðru git
        Vector2 targetPos = new Vector2(currentPatrolTarget.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, patrolSpeed * Time.deltaTime);

        // Hedefe çok yaklaþtýysak diðer noktaya geç
        if (Vector2.Distance(transform.position, currentPatrolTarget.position) < 0.5f)
        {
            currentPatrolTarget = (currentPatrolTarget == pointA) ? pointB : pointA;
            Flip();
        }
    }

    private void ChasePlayer()
    {
        // Player'a doðru koþ
        Vector2 targetPos = new Vector2(playerTransform.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);

        // Player ne taraftaysa o tarafa dön
        if (transform.position.x < playerTransform.position.x) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Çarpýþma Kontrolü (Game Over)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Player'a deðdi!
            Debug.Log("GAME OVER!");

            // Sahneyi yeniden baþlat
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // Editörde menzili görmek için çizgi çizer
    private void OnDrawGizmos()
    {
        // Devriye yolu
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }

        // Huni Çizimi
        Gizmos.color = isChasing ? Color.red : Color.yellow;

        // Bakýlan Yön
        Vector3 facingDir = transform.localScale.x > 0 ? Vector3.right : Vector3.left;

        // Sol ve Sað Sýnýr Çizgileri
        Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle / 2f, Vector3.forward);
        Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle / 2f, Vector3.forward);

        Vector3 leftDir = leftRayRotation * facingDir;
        Vector3 rightDir = rightRayRotation * facingDir;

        Gizmos.DrawRay(transform.position, leftDir * visionRange);
        Gizmos.DrawRay(transform.position, rightDir * visionRange);
    }
}
