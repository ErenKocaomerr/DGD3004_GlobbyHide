using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyPatrolAI : EnemyBase
{
    protected override void PerformMovement()
    {
        if (isChasing)
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
        // Yerde sadece X ekseninde hareket ederiz (Y sabit kalýr gibi veya zemin eðimine uyar)
        // Senin orijinal kodundaki MoveTowards mantýðý:
        Vector2 targetPos = new Vector2(playerTransform.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
        FaceTarget(playerTransform.position);
    }

    private void Patrol()
    {
        Vector2 targetPos = new Vector2(currentPatrolTarget.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, currentPatrolTarget.position) < 0.5f)
        {
            currentPatrolTarget = (currentPatrolTarget == pointA) ? pointB : pointA;
            // FaceTarget zaten MoveTowards ile otomatik yön bulmaz, hedefe göre dönelim:
            FaceTarget(currentPatrolTarget.position);
        }
    }
}
