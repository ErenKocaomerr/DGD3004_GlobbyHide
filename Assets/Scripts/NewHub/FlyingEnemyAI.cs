using UnityEngine;
using UnityEngine.SceneManagement;

public class FlyingEnemyAI : EnemyBase
{
    [Header("--- Uçuþ Ayarlarý ---")]
    public float maxChaseDistance = 10f;
    private Vector3 startPosition;
    private bool isReturning = false;

    protected override void Start()
    {
        base.Start(); // Önce EnemyBase'in Start'ýný çalýþtýr (RB alma vs.)

        rb.gravityScale = 0; // Uçan düþman için yerçekimini kapat
        startPosition = transform.position;
    }

    // Base'deki Update mantýðý buna tam uymuyor (Return mantýðý var),
    // O yüzden Update'i tamamen eziyoruz (Override).
    protected override void Update()
    {
        if (playerTransform == null) FindPlayer();

        // 1. Algýlama ve Durum Belirleme
        if (playerTransform != null && !isReturning)
        {
            float distToStart = Vector2.Distance(transform.position, startPosition);

            // Oyuncuyu görüyor muyuz?
            if (playerScript != null && !playerScript.IsHidden && CanSeePlayer())
            {
                // Sýnýrý aþtýk mý?
                if (distToStart < maxChaseDistance)
                {
                    isChasing = true;
                }
                else
                {
                    isChasing = false;
                    isReturning = true; // Çok uzaklaþtý, geri dön
                }
            }
            else
            {
                isChasing = false;
            }
        }

        // 2. Hareket
        PerformMovement();
    }

    protected override void PerformMovement()
    {
        if (isReturning)
        {
            ReturnToStart();
        }
        else if (isChasing)
        {
            Chase();
        }
        else
        {
            Patrol();
        }
    }

    private void Chase()
    {
        // Uçtuðu için direkt pozisyona gider (Y ekseni dahil)
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, chaseSpeed * Time.deltaTime);
        FaceTarget(playerTransform.position);
    }

    private void Patrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, currentPatrolTarget.position, patrolSpeed * Time.deltaTime);
        FaceTarget(currentPatrolTarget.position);

        if (Vector2.Distance(transform.position, currentPatrolTarget.position) < 0.2f)
        {
            currentPatrolTarget = (currentPatrolTarget == pointA) ? pointB : pointA;
        }
    }

    private void ReturnToStart()
    {
        transform.position = Vector2.MoveTowards(transform.position, startPosition, patrolSpeed * Time.deltaTime);
        FaceTarget(startPosition);

        if (Vector2.Distance(transform.position, startPosition) < 0.2f)
        {
            isReturning = false;
            currentPatrolTarget = pointA;
        }
    }

    // Gizmos'a ekstra çizim eklemek için override ediyoruz
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos(); // Önce Base'in çizdiklerini çiz

        // Sonra buna özel olaný çiz
        Gizmos.color = Color.magenta;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, maxChaseDistance);
    }
}
