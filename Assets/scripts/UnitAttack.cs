using UnityEngine;
using System.Collections.Generic;

public class UnitAttack : MonoBehaviour
{
    [Header("범위 설정")]
    [Tooltip("범위 오브젝트 (CircleCollider2D 또는 SphereCollider 필요)")]
    public Transform rangeObject;

    [Header("공격 설정")]
    [Tooltip("총알 프리팹")]
    public GameObject bulletPrefab;
    [Tooltip("총알 발사 위치 (없으면 유닛 위치에서 발사)")]
    public Transform firePoint;
    [Tooltip("n초에 한 번 발사 (1 = 1초에 한 번)")]
    public float fireInterval = 1f;
    [Tooltip("총알 속도")]
    public float bulletSpeed = 10f;
    [Tooltip("총알 데미지")]
    public float bulletDamage = 10f;

    [Header("회전 설정")]
    [Tooltip("회전 오프셋 (도)")]
    public float rotationOffset = 0f;

    [Header("디버그")]
    public bool showDebugLog = true;

    private Transform currentTarget;
    private float fireTimer = 0f;
    private float attackRange = 5f;

    private void Start()
    {
        // range 오브젝트에서 범위 계산
        if (rangeObject != null)
        {
            // 2D Collider 체크
            CircleCollider2D circle2D = rangeObject.GetComponent<CircleCollider2D>();
            if (circle2D != null)
            {
                attackRange = circle2D.radius * Mathf.Max(rangeObject.lossyScale.x, rangeObject.lossyScale.y);
                if (showDebugLog) Debug.Log($"[UnitAttack] 2D 범위 설정: {attackRange}");
            }
            else
            {
                // 3D Collider 체크
                SphereCollider sphere = rangeObject.GetComponent<SphereCollider>();
                if (sphere != null)
                {
                    attackRange = sphere.radius * Mathf.Max(rangeObject.lossyScale.x, rangeObject.lossyScale.y, rangeObject.lossyScale.z);
                    if (showDebugLog) Debug.Log($"[UnitAttack] 3D 범위 설정: {attackRange}");
                }
                else
                {
                    // 콜라이더 없으면 스케일 사용
                    attackRange = Mathf.Max(rangeObject.lossyScale.x, rangeObject.lossyScale.y) / 2f;
                    if (showDebugLog) Debug.Log($"[UnitAttack] 스케일 기반 범위 설정: {attackRange}");
                }
            }
        }
        else
        {
            if (showDebugLog) Debug.LogWarning("[UnitAttack] Range 오브젝트가 설정되지 않았습니다!");
        }
    }

    private void Update()
    {
        // 범위 내 적 찾기
        FindTarget();

        if (currentTarget != null)
        {
            // 적 방향으로 회전
            RotateTowardsTarget();

            // 발사 타이머
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval)
            {
                Fire();
                fireTimer = 0f;
            }
        }
    }

    private void FindTarget()
    {
        // 범위 중심 위치
        Vector3 rangeCenter = rangeObject != null ? rangeObject.position : transform.position;

        // 범위 내 모든 콜라이더 찾기 (2D)
        Collider2D[] colliders2D = Physics2D.OverlapCircleAll(rangeCenter, attackRange);

        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D col in colliders2D)
        {
            if (col.CompareTag("enemy"))
            {
                float distance = Vector2.Distance(rangeCenter, col.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = col.transform;
                }
            }
        }

        // 3D도 체크
        if (nearest == null)
        {
            Collider[] colliders3D = Physics.OverlapSphere(rangeCenter, attackRange);
            foreach (Collider col in colliders3D)
            {
                if (col.CompareTag("enemy"))
                {
                    float distance = Vector3.Distance(rangeCenter, col.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = col.transform;
                    }
                }
            }
        }

        if (nearest != currentTarget && showDebugLog)
        {
            if (nearest != null)
                Debug.Log($"[UnitAttack] 타겟 발견: {nearest.name}");
            else if (currentTarget != null)
                Debug.Log("[UnitAttack] 타겟 없음");
        }

        currentTarget = nearest;
    }

    private void RotateTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = currentTarget.position - transform.position;

        // 2D 회전 (Z축 기준) - 즉시 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Fire()
    {
        if (bulletPrefab == null)
        {
            if (showDebugLog) Debug.LogWarning("[UnitAttack] 총알 프리팹이 없습니다!");
            return;
        }

        if (currentTarget == null)
        {
            if (showDebugLog) Debug.LogWarning("[UnitAttack] 타겟이 없습니다!");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // Bullet 컴포넌트 설정
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript == null)
        {
            bulletScript = bullet.AddComponent<Bullet>();
        }
        bulletScript.Initialize(currentTarget, bulletSpeed, attackRange, spawnPos, bulletDamage);

        if (showDebugLog) Debug.Log($"[UnitAttack] 총알 발사! 타겟: {currentTarget.name}");
    }

    // 에디터 시각화
    private void OnDrawGizmosSelected()
    {
        // 공격 범위 표시
        Gizmos.color = Color.red;
        Vector3 rangePos = rangeObject != null ? rangeObject.position : transform.position;
        Gizmos.DrawWireSphere(rangePos, attackRange);

        // 현재 타겟 표시
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}
