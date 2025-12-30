using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyBase : MonoBehaviour
{
    [Header("--- Ortak Ayarlar ---")]
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("--- Algýlama ---")]
    public float visionRange = 5f;
    [Range(0, 360)] public float viewAngle = 45f;
    public LayerMask playerLayer;

    protected Rigidbody2D rb;
    protected Transform currentPatrolTarget;
    protected Transform playerTransform;
    protected AdvancedPlayerController playerScript;
    protected bool isChasing = false;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentPatrolTarget = pointB;
        FindPlayer();
    }

    protected virtual void Update()
    {
        if (playerTransform == null) FindPlayer();

        // Ortak Karar Mekanizmasý
        if (playerTransform != null && playerScript != null && !playerScript.IsHidden)
        {
            if (CanSeePlayer())
            {
                isChasing = true;
            }
            else
            {
                isChasing = false;
            }
        }
        else
        {
            isChasing = false;
        }

        PerformMovement(); 
    }

    protected virtual void PerformMovement()
    {
    }


    protected bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distToPlayer > visionRange) return false;

        Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
        Vector2 facingDir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        if (Vector2.Angle(facingDir, dirToPlayer) < viewAngle / 2f)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distToPlayer, playerLayer);
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    protected void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            playerTransform = p.transform;
            playerScript = p.GetComponent<AdvancedPlayerController>();
        }
    }

    // Yön çevirme (Sprite Flip) - Ortak hale getirdik
    protected void FaceTarget(Vector3 target)
    {
        if (target.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }

        Gizmos.color = isChasing ? Color.red : Color.yellow;
        Vector3 facingDir = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle / 2f, Vector3.forward);
        Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle / 2f, Vector3.forward);
        Gizmos.DrawRay(transform.position, (leftRayRotation * facingDir) * visionRange);
        Gizmos.DrawRay(transform.position, (rightRayRotation * facingDir) * visionRange);
    }
}
