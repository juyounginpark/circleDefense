using UnityEngine;
using UnityEngine.InputSystem;
using System;

[Serializable]
public class RotationTarget
{
    public Transform target;
    [Tooltip("이 오브젝트만 반대 방향으로 회전")]
    public bool reverse = false;
}

public class RoleMove : MonoBehaviour
{
    [Header("회전 대상 오브젝트들")]
    [Tooltip("Z축 회전을 적용할 오브젝트 배열 (개별 reverse 설정 가능)")]
    public RotationTarget[] targetObjects;

    [Header("회전 설정")]
    [Tooltip("마우스 회전 감도")]
    public float rotationSpeed = 1f;

    [Header("마우스 드래그 설정")]
    [Tooltip("마우스 드래그로 회전 활성화")]
    public bool enableMouseRotation = true;

    private bool isDragging = false;
    private Vector2 lastMousePosition;
    private Camera mainCamera;
    private Mouse mouse;

    private void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;
    }

    private void Update()
    {
        if (enableMouseRotation)
        {
            HandleMouseRotation();
        }

        ApplyRotationToTargets();
    }

    private void HandleMouseRotation()
    {
        if (mouse == null) return;

        // 마우스 버튼 눌렀을 때 드래그 시작
        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (IsMouseOverObject())
            {
                isDragging = true;
                lastMousePosition = mouse.position.ReadValue();
            }
        }

        // 마우스 버튼 뗐을 때 드래그 종료
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        // 드래그 중일 때 회전 적용
        if (isDragging)
        {
            Vector2 currentMousePosition = mouse.position.ReadValue();
            float deltaX = currentMousePosition.x - lastMousePosition.x;

            // Z축 회전 적용 (마우스 X 이동에 따라)
            float rotationAmount = -deltaX * rotationSpeed;
            transform.Rotate(0, 0, rotationAmount);

            lastMousePosition = currentMousePosition;
        }
    }

    private bool IsMouseOverObject()
    {
        if (mainCamera == null || mouse == null) return false;

        Vector2 mousePos = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.transform == transform;
        }

        // 2D 콜라이더 체크
        RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit2D.collider != null)
        {
            return hit2D.transform == transform;
        }

        return false;
    }

    private void ApplyRotationToTargets()
    {
        if (targetObjects == null || targetObjects.Length == 0) return;

        float baseZRotation = transform.eulerAngles.z;

        foreach (RotationTarget rotationTarget in targetObjects)
        {
            if (rotationTarget.target != null)
            {
                // 각 오브젝트의 개별 reverse 옵션에 따라 회전 방향 결정
                float zRotation = rotationTarget.reverse ? -baseZRotation : baseZRotation;

                // 타겟 오브젝트의 Z 회전만 변경
                Vector3 currentRotation = rotationTarget.target.eulerAngles;
                rotationTarget.target.eulerAngles = new Vector3(currentRotation.x, currentRotation.y, zRotation);
            }
        }
    }

    // 에디터에서 오브젝트 선택 시 시각화
    private void OnDrawGizmosSelected()
    {
        if (targetObjects == null) return;

        foreach (RotationTarget rotationTarget in targetObjects)
        {
            if (rotationTarget.target != null)
            {
                // reverse 오브젝트는 빨간색, 일반은 노란색으로 표시
                Gizmos.color = rotationTarget.reverse ? Color.red : Color.yellow;
                Gizmos.DrawLine(transform.position, rotationTarget.target.position);
                Gizmos.DrawWireSphere(rotationTarget.target.position, 0.2f);
            }
        }
    }
}
