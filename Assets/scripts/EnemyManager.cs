using UnityEngine;
using TMPro;
using System;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("게임 오버 설정")]
    [Tooltip("게임 오버가 되는 적 개수")]
    public int maxEnemyCount = 100;

    [Header("UI 연결")]
    [Tooltip("적 개수 표시 텍스트 (TextMeshProUGUI)")]
    public TextMeshProUGUI enemyCountText;
    [Tooltip("게임 오버 패널 (게임 오버 시 활성화)")]
    public GameObject gameOverPanel;

    [Header("표시 형식")]
    [Tooltip("적 개수 표시 형식 ({0} = 현재, {1} = 최대)")]
    public string countFormat = "Enemy: {0} / {1}";

    [Header("현재 상태")]
    [SerializeField] private int currentEnemyCount = 0;
    [SerializeField] private bool isGameOver = false;

    // 이벤트
    public event Action<int> OnEnemyCountChanged;
    public event Action OnGameOver;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        UpdateUI();
    }

    private void Update()
    {
        // 매 프레임 enemy 태그 개수 체크
        UpdateEnemyCount();
    }

    private void UpdateEnemyCount()
    {
        if (isGameOver) return;

        int count = 0;
        try
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");
            count = enemies.Length;
        }
        catch
        {
            count = 0;
        }

        if (count != currentEnemyCount)
        {
            currentEnemyCount = count;
            OnEnemyCountChanged?.Invoke(currentEnemyCount);
            UpdateUI();

            // 게임 오버 체크
            if (currentEnemyCount >= maxEnemyCount)
            {
                TriggerGameOver();
            }
        }
    }

    private void UpdateUI()
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = string.Format(countFormat, currentEnemyCount, maxEnemyCount);
        }
    }

    private void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("[EnemyManager] 게임 오버! 적이 " + maxEnemyCount + "마리에 도달했습니다.");

        // 게임 오버 패널 활성화
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 게임 일시정지
        Time.timeScale = 0f;

        // 이벤트 발생
        OnGameOver?.Invoke();
    }

    // 게임 재시작
    public void RestartGame()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    // 현재 적 개수 반환
    public int GetCurrentEnemyCount()
    {
        return currentEnemyCount;
    }

    // 게임 오버 상태 반환
    public bool IsGameOver()
    {
        return isGameOver;
    }
}
