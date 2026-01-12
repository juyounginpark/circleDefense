using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("총알 설정")]
    [Tooltip("총알 데미지 (Initialize에서 덮어쓸 수 있음)")]
    public float bulletDamage = 10f;

    [Header("회전 설정")]
    [Tooltip("회전 오프셋 (도)")]
    public float rotationOffset = 0f;

    private Transform target;
    private float speed;
    private float maxRange;
    private Vector3 startPosition;
    private float damage;
    private bool isInitialized = false;

    private void Awake()
    {
        // Rigidbody2D 자동 추가 (충돌 감지용)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        // Collider2D가 없으면 추가
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
            circle.radius = 0.1f;
            circle.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }
    }

    public void Initialize(Transform targetTransform, float spd, float range, Vector3 startPos, float dmg = -1f)
    {
        target = targetTransform;
        speed = spd;
        maxRange = range;
        startPosition = startPos;
        // dmg가 -1이면 Inspector의 bulletDamage 사용
        damage = dmg >= 0 ? dmg : bulletDamage;
        isInitialized = true;

        // 초기 방향 설정
        if (target != null)
        {
            LookAtTarget();
        }
    }

    private void Update()
    {
        if (!isInitialized) return;

        // 타겟이 있으면 유도탄처럼 추적
        if (target != null)
        {
            // 타겟 방향으로 즉시 회전
            LookAtTarget();

            // 타겟 방향으로 이동
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
        else
        {
            // 타겟이 없으면 현재 방향으로 직진
            transform.position += transform.right * speed * Time.deltaTime;
        }

        // 범위 체크 - 시작 위치에서 maxRange 이상 벗어나면 제거
        float distance = Vector3.Distance(startPosition, transform.position);
        if (distance > maxRange)
        {
            Destroy(gameObject);
        }
    }

    private void LookAtTarget()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("enemy"))
        {
            ApplyDamage(other.gameObject);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("enemy"))
        {
            ApplyDamage(other.gameObject);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            ApplyDamage(collision.gameObject);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            ApplyDamage(collision.gameObject);
            Destroy(gameObject);
        }
    }

    private void ApplyDamage(GameObject enemy)
    {
        EnemyHP enemyHP = enemy.GetComponent<EnemyHP>();
        if (enemyHP != null)
        {
            enemyHP.TakeDamage(damage);
        }
    }
}
