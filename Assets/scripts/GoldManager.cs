using UnityEngine;
using System;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    [Header("골드 설정")]
    [Tooltip("시작 골드")]
    public int startingGold = 50;

    [Header("현재 골드")]
    [SerializeField] private int currentGold;

    // 골드 변경 이벤트 (UI 업데이트용)
    public event Action<int> OnGoldChanged;

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
        currentGold = startingGold;
        OnGoldChanged?.Invoke(currentGold);
    }

    // 골드 추가
    public void AddGold(int amount)
    {
        currentGold += amount;
        OnGoldChanged?.Invoke(currentGold);
    }

    // 골드 사용 (성공 시 true 반환)
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            return true;
        }
        return false;
    }

    // 골드 충분한지 확인
    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    // 현재 골드 반환
    public int GetCurrentGold()
    {
        return currentGold;
    }
}
