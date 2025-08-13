using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;  // 따라갈 대상 (플레이어)
    
    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);  // 카메라와 타겟의 거리
    [SerializeField] private float smoothSpeed = 3f;  // 부드러운 움직임 속도 (더 느리게)
    
    [Header("Bounds")]
    [SerializeField] private bool useBounds = true;  // 경계 사용 여부 (기본 활성화)
    [SerializeField] private float minX = -20f;  // 최소 X 위치
    [SerializeField] private float maxX = 20f;   // 최대 X 위치
    [SerializeField] private float minY = -15f;  // 최소 Y 위치
    [SerializeField] private float maxY = 15f;   // 최대 Y 위치

    private void Start()
    {
        // 타겟이 설정되지 않았으면 플레이어를 찾기
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;
        
        // 경계 적용
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }
        
        // 부드러운 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
} 