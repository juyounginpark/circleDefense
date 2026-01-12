using UnityEngine;
using TMPro;

public class EnemyHP : MonoBehaviour
{
    [Header("체력 설정")]
    [Tooltip("최대 체력")]
    public float maxHP = 100f;
    [Tooltip("현재 체력")]
    [SerializeField] private float currentHP;

    [Header("HP 표시 설정")]
    [Tooltip("HP 텍스트 Y 오프셋")]
    public float hpTextOffsetY = -0.5f;
    [Tooltip("HP 텍스트 크기")]
    public float hpTextSize = 3f;
    [Tooltip("HP 텍스트 색상")]
    public Color hpTextColor = Color.white;

    [Header("디버그")]
    public bool showDebugLog = true;

    private TextMeshPro hpText;

    private void Start()
    {
        currentHP = maxHP;
        CreateHPText();
        UpdateHPText();
    }

    private void CreateHPText()
    {
        // HP 텍스트 오브젝트 생성
        GameObject hpTextObj = new GameObject("HP_Text");
        hpTextObj.transform.SetParent(transform);
        hpTextObj.transform.localPosition = new Vector3(0, hpTextOffsetY, 0);
        hpTextObj.transform.localRotation = Quaternion.identity;

        // TextMeshPro 컴포넌트 추가
        hpText = hpTextObj.AddComponent<TextMeshPro>();
        hpText.fontSize = hpTextSize;
        hpText.color = hpTextColor;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.sortingOrder = 100;
    }

    private void UpdateHPText()
    {
        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(currentHP)}";
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        // 데미지만큼 골드 추가
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(Mathf.CeilToInt(damage));
        }

        if (showDebugLog) Debug.Log($"[EnemyHP] {gameObject.name} 데미지 {damage} 받음! 남은 HP: {currentHP}/{maxHP}");

        UpdateHPText();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (showDebugLog) Debug.Log($"[EnemyHP] {gameObject.name} 사망!");
        Destroy(gameObject);
    }

    // 현재 체력 반환
    public float GetCurrentHP()
    {
        return currentHP;
    }

    // 최대 체력 반환
    public float GetMaxHP()
    {
        return maxHP;
    }

    // 체력 비율 반환 (0~1)
    public float GetHPRatio()
    {
        return currentHP / maxHP;
    }
}
