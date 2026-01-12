using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    [Header("UI 연결")]
    [Tooltip("골드 표시 텍스트 (TextMeshProUGUI)")]
    public TextMeshProUGUI goldText;
    [Tooltip("소환 비용 표시 텍스트 (TextMeshProUGUI)")]
    public TextMeshProUGUI costText;
    [Tooltip("Unit 스크립트 참조 (소환 비용 표시용)")]
    public Unit unitScript;

    [Header("표시 형식")]
    [Tooltip("골드 표시 형식 ({0} = 골드 값)")]
    public string goldFormat = "Gold: {0}";
    [Tooltip("비용 표시 형식 ({0} = 비용 값)")]
    public string costFormat = "Cost: {0}";

    private void Start()
    {
        // GoldManager 이벤트 구독
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged += UpdateGoldUI;
            UpdateGoldUI(GoldManager.Instance.GetCurrentGold());
        }

        // Unit 이벤트 구독
        if (unitScript != null)
        {
            unitScript.OnCostChanged += UpdateCostUI;
            UpdateCostUI(unitScript.GetCurrentCost());
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged -= UpdateGoldUI;
        }

        if (unitScript != null)
        {
            unitScript.OnCostChanged -= UpdateCostUI;
        }
    }

    private void UpdateGoldUI(int gold)
    {
        if (goldText != null)
        {
            goldText.text = string.Format(goldFormat, gold);
        }
    }

    private void UpdateCostUI(int cost)
    {
        if (costText != null)
        {
            costText.text = string.Format(costFormat, cost);
        }
    }
}
