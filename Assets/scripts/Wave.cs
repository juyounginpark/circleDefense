using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class EnemyPrefabData
{
    [Tooltip("적 프리팹")]
    public GameObject prefab;
    [Range(0f, 100f)]
    [Tooltip("스폰 확률 (%)")]
    public float spawnChance = 100f;
    [Tooltip("이동 속도")]
    public float moveSpeed = 3f;
}

[Serializable]
public class WaveData
{
    [Tooltip("웨이브 이름 (표시용)")]
    public string waveName = "Wave";
    [Tooltip("웨이브 지속 시간 (초)")]
    public float waveDuration = 30f;
    [Tooltip("적 스폰 간격 (n초마다 스폰)")]
    public float spawnInterval = 2f;
    [Tooltip("적 프리팹 데이터베이스")]
    public EnemyPrefabData[] enemyDatabase;
}

public class Wave : MonoBehaviour
{
    [Header("이동 경로 (모든 웨이브 공통)")]
    [Tooltip("0번: 스폰 위치, 이후 순서대로 이동")]
    public Transform[] spotDatabase;

    [Header("웨이브 데이터베이스")]
    [Tooltip("0번: 기본 웨이브 (반복용), 1번부터 실제 웨이브 시작")]
    public WaveData[] waveDatabase;

    [Header("현재 상태")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private bool isWaveActive = false;

    [Header("디버그")]
    public bool showDebugLog = true;

    private float waveTimer = 0f;
    private float spawnTimer = 0f;
    private WaveData currentWave;

    private void Start()
    {
        if (waveDatabase != null && waveDatabase.Length > 1)
        {
            // 1번 웨이브부터 시작 (0번은 기본 반복용)
            StartWave(1);
        }
        else if (waveDatabase != null && waveDatabase.Length == 1)
        {
            // 0번만 있으면 0번 시작
            StartWave(0);
        }
    }

    private void Update()
    {
        if (!isWaveActive || currentWave == null) return;

        // 웨이브 타이머
        waveTimer += Time.deltaTime;
        if (waveTimer >= currentWave.waveDuration)
        {
            EndWave();
            return;
        }

        // 스폰 타이머
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentWave.spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    public void StartWave(int waveIndex)
    {
        if (waveDatabase == null || waveIndex >= waveDatabase.Length)
        {
            if (showDebugLog) Debug.LogWarning("[Wave] 유효하지 않은 웨이브 인덱스!");
            return;
        }

        currentWaveIndex = waveIndex;
        currentWave = waveDatabase[waveIndex];
        waveTimer = 0f;
        spawnTimer = 0f;
        isWaveActive = true;

        if (showDebugLog) Debug.Log($"[Wave] 웨이브 {waveIndex + 1} 시작! 지속시간: {currentWave.waveDuration}초");
    }

    private void EndWave()
    {
        isWaveActive = false;
        if (showDebugLog) Debug.Log($"[Wave] 웨이브 {currentWaveIndex} 종료!");

        // 다음 웨이브 시작
        int nextWave = currentWaveIndex + 1;
        if (nextWave < waveDatabase.Length)
        {
            // 다음 웨이브 존재 시 진행
            StartWave(nextWave);
        }
        else
        {
            // 다음 웨이브 없으면 0번 기본 웨이브 반복
            if (showDebugLog) Debug.Log("[Wave] 모든 웨이브 완료! 기본 웨이브(0번) 반복 시작");
            StartWave(0);
        }
    }

    private void SpawnEnemy()
    {
        if (currentWave.enemyDatabase == null || currentWave.enemyDatabase.Length == 0)
        {
            if (showDebugLog) Debug.LogWarning("[Wave] 적 데이터베이스가 비어있습니다!");
            return;
        }

        if (spotDatabase == null || spotDatabase.Length == 0)
        {
            if (showDebugLog) Debug.LogWarning("[Wave] 스폿 데이터베이스가 비어있습니다!");
            return;
        }

        // 확률에 따라 적 선택
        EnemyPrefabData selectedEnemy = SelectEnemyByChance();
        if (selectedEnemy == null || selectedEnemy.prefab == null)
        {
            if (showDebugLog) Debug.Log("[Wave] 이번 스폰에서 적이 선택되지 않음");
            return;
        }

        // 스폰 위치 (0번 스폿)
        Vector3 spawnPos = spotDatabase[0].position;

        // 적 생성
        GameObject enemy = Instantiate(selectedEnemy.prefab, spawnPos, Quaternion.identity);

        // EnemyMover 컴포넌트 추가
        EnemyMover mover = enemy.GetComponent<EnemyMover>();
        if (mover == null)
        {
            mover = enemy.AddComponent<EnemyMover>();
        }
        mover.Initialize(spotDatabase, selectedEnemy.moveSpeed);

        if (showDebugLog) Debug.Log($"[Wave] 적 '{selectedEnemy.prefab.name}' 스폰! 속도: {selectedEnemy.moveSpeed}");
    }

    private EnemyPrefabData SelectEnemyByChance()
    {
        // 확률에 따라 적 선택
        foreach (EnemyPrefabData enemyData in currentWave.enemyDatabase)
        {
            if (enemyData.prefab == null) continue;

            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll < enemyData.spawnChance)
            {
                return enemyData;
            }
        }

        // 모든 확률 실패 시 0번 적 반환
        if (currentWave.enemyDatabase.Length > 0 && currentWave.enemyDatabase[0].prefab != null)
        {
            return currentWave.enemyDatabase[0];
        }

        return null;
    }

    // 외부에서 웨이브 시작
    public void StartNextWave()
    {
        if (!isWaveActive)
        {
            int nextWave = currentWaveIndex + 1;
            if (nextWave < waveDatabase.Length)
            {
                StartWave(nextWave);
            }
        }
    }

    // 현재 웨이브 강제 종료
    public void ForceEndWave()
    {
        EndWave();
    }

    // 에디터 시각화
    private void OnDrawGizmosSelected()
    {
        if (spotDatabase == null) return;

        for (int i = 0; i < spotDatabase.Length; i++)
        {
            if (spotDatabase[i] == null) continue;

            // 스폰 위치는 녹색, 나머지는 노란색
            Gizmos.color = i == 0 ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(spotDatabase[i].position, 0.3f);

            // 경로 선 그리기
            if (i > 0 && spotDatabase[i - 1] != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(spotDatabase[i - 1].position, spotDatabase[i].position);
            }
        }
    }
}

// 적 이동 컴포넌트
public class EnemyMover : MonoBehaviour
{
    private Transform[] spots;
    private float speed;
    private int currentSpotIndex = 1; // 0번은 스폰 위치이므로 1번부터 시작
    private bool isInitialized = false;

    public void Initialize(Transform[] spotDatabase, float moveSpeed)
    {
        spots = spotDatabase;
        speed = moveSpeed;
        currentSpotIndex = 1;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || spots == null || spots.Length <= 1) return;

        // 현재 목표 스폿
        if (currentSpotIndex >= spots.Length)
        {
            // 마지막 스폿 도달 시 0번으로 루프 (또는 삭제)
            currentSpotIndex = 0;
        }

        Transform targetSpot = spots[currentSpotIndex];
        if (targetSpot == null) return;

        // 목표 방향으로 이동 (회전 없이)
        Vector3 direction = (targetSpot.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // 목표 도달 체크
        float distance = Vector3.Distance(transform.position, targetSpot.position);
        if (distance < 0.1f)
        {
            currentSpotIndex++;
        }
    }
}
