using UnityEngine;
using UnityEngine.InputSystem;

public class UnitMove : MonoBehaviour
{
    [Header("드래그 설정")]
    [Tooltip("드래그 중 유닛이 따라다닐 Z 깊이 (카메라로부터의 거리)")]
    public float dragDepth = 10f;

    [Header("점선 설정")]
    [Tooltip("점선 색상")]
    public Color lineColor = Color.white;
    [Tooltip("점선 두께")]
    public float lineWidth = 0.05f;
    [Tooltip("점선 소팅 레이어")]
    public string sortingLayerName = "Default";
    [Tooltip("점선 소팅 오더 (높을수록 앞에 표시)")]
    public int sortingOrder = 100;

    private Camera mainCamera;
    private Mouse mouse;
    private bool isDragging = false;
    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Vector3 originalWorldPosition;

    // 점선용 LineRenderer
    private LineRenderer lineRenderer;

    private void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        // LineRenderer 컴포넌트 생성
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // 기본 머티리얼 설정
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        // 소팅 레이어 설정
        lineRenderer.sortingLayerName = sortingLayerName;
        lineRenderer.sortingOrder = sortingOrder;

        // 점선 텍스처 패턴 설정
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.textureScale = new Vector2(10f, 1f);

        // 처음에는 숨김
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (mouse == null || mainCamera == null) return;

        // 마우스 클릭 시작
        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (IsMouseOverThis())
            {
                StartDrag();
            }
        }

        // 드래그 중
        if (isDragging)
        {
            FollowMouse();
            UpdateLine();
        }

        // 마우스 버튼 놓음
        if (mouse.leftButton.wasReleasedThisFrame && isDragging)
        {
            EndDrag();
        }
    }

    private bool IsMouseOverThis()
    {
        Vector2 mousePos = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        // 3D 체크
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform)
            {
                return true;
            }
        }

        // 2D 체크
        RaycastHit2D[] hits2D = Physics2D.RaycastAll(ray.origin, ray.direction);
        foreach (RaycastHit2D hit2D in hits2D)
        {
            if (hit2D.transform == transform)
            {
                return true;
            }
        }

        return false;
    }

    private void StartDrag()
    {
        isDragging = true;
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalWorldPosition = transform.position;

        // 드래그 중에는 부모에서 분리
        transform.SetParent(null);

        // 점선 보이기
        if (lineRenderer != null)
        {
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.enabled = true;
        }
    }

    private void FollowMouse()
    {
        Vector2 mousePos = mouse.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, dragDepth));
        transform.position = worldPos;
    }

    private void UpdateLine()
    {
        if (lineRenderer == null) return;

        // 원래 위치에서 현재 위치까지 선 그리기
        lineRenderer.SetPosition(0, originalWorldPosition);
        lineRenderer.SetPosition(1, transform.position);
    }

    private void EndDrag()
    {
        isDragging = false;

        // 점선 숨기기
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        // 가장 가까운 빈 unitBlock 찾기
        Transform nearestBlock = FindNearestEmptyUnitBlock();

        if (nearestBlock != null)
        {
            // 새로운 블록의 자식으로 이동
            transform.SetParent(nearestBlock);
            transform.localPosition = Vector3.zero;
            Debug.Log($"유닛이 '{nearestBlock.name}'으로 이동했습니다!");
        }
        else
        {
            // 빈 블록이 없으면 원래 위치로 복귀
            transform.SetParent(originalParent);
            transform.localPosition = originalLocalPosition;
            Debug.Log("이동 가능한 빈 unitBlock이 없어 원래 위치로 복귀합니다.");
        }
    }

    private Transform FindNearestEmptyUnitBlock()
    {
        GameObject[] unitBlocks;
        try
        {
            unitBlocks = GameObject.FindGameObjectsWithTag("unitBlock");
        }
        catch
        {
            Debug.LogWarning("unitBlock 태그가 존재하지 않습니다!");
            return null;
        }

        if (unitBlocks.Length == 0)
        {
            return null;
        }

        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject block in unitBlocks)
        {
            // 자식이 없거나, 자식이 자기 자신뿐인 블록만 (원래 부모 포함)
            bool isEmpty = block.transform.childCount == 0;
            bool isOriginalParent = block.transform == originalParent;

            if (isEmpty || isOriginalParent)
            {
                float distance = Vector3.Distance(transform.position, block.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = block.transform;
                }
            }
        }

        return nearest;
    }

    private void OnDestroy()
    {
        // 동적으로 생성한 머티리얼 정리
        if (lineRenderer != null && lineRenderer.material != null)
        {
            Destroy(lineRenderer.material);
        }
    }
}
