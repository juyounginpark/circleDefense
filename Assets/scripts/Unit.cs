using UnityEngine;
using UnityEngine.InputSystem;
using System;

[Serializable]
public class UnitData
{
    public GameObject prefab;
    [Range(0f, 100f)]
    [Tooltip("소환 확률 (%) - 0번 유닛은 기본 유닛으로 확률 무시")]
    public float spawnChance = 100f;
    [Tooltip("이 유닛 소환 시 재생할 사운드")]
    public AudioClip spawnSound;
}

public class Unit : MonoBehaviour
{
    [Header("생성 버튼")]
    [Tooltip("클릭하면 유닛을 소환할 오브젝트 (roleMove의 자식 가능)")]
    public GameObject genObject;

    [Header("유닛 데이터베이스")]
    [Tooltip("0번: 기본 유닛 (확률 실패 시 소환), 1번~n번: 확률에 따라 소환")]
    public UnitData[] unitDatabase;

    [Header("소환 비용")]
    [Tooltip("기본 소환 비용")]
    public int baseCost = 10;
    [Tooltip("소환할 때마다 증가하는 비용")]
    public int costIncrease = 2;
    [Tooltip("현재 소환 비용")]
    [SerializeField] private int currentCost;

    [Header("사운드 설정")]
    [Tooltip("사운드 볼륨")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    // 비용 변경 이벤트 (UI 업데이트용)
    public event System.Action<int> OnCostChanged;

    private Camera mainCamera;
    private Mouse mouse;
    private AudioSource audioSource;

    private void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;

        // 초기 비용 설정
        currentCost = baseCost;
        OnCostChanged?.Invoke(currentCost);

        // AudioSource 컴포넌트 가져오거나 생성
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // 현재 비용 반환
    public int GetCurrentCost()
    {
        return currentCost;
    }

    private void Update()
    {
        if (mouse == null) return;

        // 마우스 클릭 감지
        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (IsClickedOnGenObject())
            {
                SpawnUnit();
            }
        }
    }

    private bool IsClickedOnGenObject()
    {
        if (mainCamera == null || genObject == null) return false;

        Vector2 mousePos = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        // 3D - RaycastAll로 모든 충돌 체크 (부모 오브젝트에 가려지는 문제 해결)
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject == genObject)
            {
                return true;
            }
        }

        // 2D - RaycastAll로 모든 충돌 체크
        RaycastHit2D[] hits2D = Physics2D.RaycastAll(ray.origin, ray.direction);
        foreach (RaycastHit2D hit2D in hits2D)
        {
            if (hit2D.transform.gameObject == genObject)
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnUnit()
    {
        if (unitDatabase == null || unitDatabase.Length == 0)
        {
            Debug.LogWarning("유닛 데이터베이스가 비어있습니다!");
            return;
        }

        // 골드 체크
        if (GoldManager.Instance != null && !GoldManager.Instance.HasEnoughGold(currentCost))
        {
            Debug.Log($"골드가 부족합니다! 필요: {currentCost}, 보유: {GoldManager.Instance.GetCurrentGold()}");
            return;
        }

        // unitBlock 태그가 붙은 오브젝트들 찾기
        GameObject[] unitBlocks = GameObject.FindGameObjectsWithTag("unitBlock");

        if (unitBlocks.Length == 0)
        {
            Debug.LogWarning("unitBlock 태그를 가진 오브젝트가 없습니다!");
            return;
        }

        // 자식이 없는 unitBlock만 필터링
        var availableBlocks = System.Array.FindAll(unitBlocks, block => block.transform.childCount == 0);

        if (availableBlocks.Length == 0)
        {
            Debug.Log("사용 가능한 unitBlock이 없습니다. 모든 블록이 사용 중입니다.");
            return;
        }

        // 골드 차감
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.SpendGold(currentCost);
        }

        // 랜덤으로 unitBlock 선택
        GameObject selectedBlock = availableBlocks[UnityEngine.Random.Range(0, availableBlocks.Length)];

        // 확률에 따라 유닛 선택
        int selectedIndex = SelectUnitIndexByChance();

        if (selectedIndex < 0 || unitDatabase[selectedIndex].prefab == null)
        {
            Debug.LogWarning("소환할 유닛이 없습니다!");
            return;
        }

        UnitData selectedUnitData = unitDatabase[selectedIndex];

        // 유닛 생성 및 배치
        GameObject spawnedUnit = Instantiate(selectedUnitData.prefab, selectedBlock.transform);
        spawnedUnit.transform.localPosition = Vector3.zero;

        // 사운드 재생
        if (selectedUnitData.spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(selectedUnitData.spawnSound, soundVolume);
        }

        // 비용 증가
        currentCost += costIncrease;
        OnCostChanged?.Invoke(currentCost);

        Debug.Log($"유닛 '{selectedUnitData.prefab.name}'이(가) '{selectedBlock.name}'에 소환되었습니다! 다음 비용: {currentCost}");
    }

    private int SelectUnitIndexByChance()
    {
        // 1번 유닛부터 n번 유닛까지 확률 체크
        for (int i = 1; i < unitDatabase.Length; i++)
        {
            if (unitDatabase[i].prefab == null) continue;

            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll < unitDatabase[i].spawnChance)
            {
                // 확률 성공! 해당 인덱스 반환
                return i;
            }
        }

        // 모든 확률 실패 시 0번 기본 유닛 반환
        if (unitDatabase.Length > 0 && unitDatabase[0].prefab != null)
        {
            return 0;
        }

        return -1;
    }

    // 에디터에서 시각화
    private void OnDrawGizmosSelected()
    {
        if (genObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(genObject.transform.position, Vector3.one * 0.5f);
        }

        GameObject[] unitBlocks = null;
        try
        {
            unitBlocks = GameObject.FindGameObjectsWithTag("unitBlock");
        }
        catch
        {
            return;
        }

        Gizmos.color = Color.cyan;
        foreach (GameObject block in unitBlocks)
        {
            Gizmos.DrawWireSphere(block.transform.position, 0.3f);
        }
    }
}
