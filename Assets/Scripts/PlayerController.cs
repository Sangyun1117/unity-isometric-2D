using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private float x;
    private float y;
    private Vector2 lastDirection = Vector2.zero; // 마지막 이동 방향
    [SerializeField] private int spriteIndex = 0;

    public void Awake()
    {
        Debug.Log($"startssss");
        rb = GetComponent<Rigidbody2D>();
    }

    public void Update()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        
        // 방향이 변경되었을 때만 스프라이트 업데이트
        Vector2 currentDirection = new Vector2(x, y);
        if (currentDirection != Vector2.zero && currentDirection != lastDirection)
        {
            UpdateSprite(currentDirection);
            lastDirection = currentDirection;
        }
    }

    public void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(x * moveSpeed, y * moveSpeed);
    }
    
    private void UpdateSprite(Vector2 direction)
    {
        // 입력이 없으면 이전 스프라이트 유지
        if (direction == Vector2.zero) return;
        
        // 방향을 정규화하고 각도 계산
        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // 8방향으로 분할 (각 45도씩)
        
        if (angle >= -22.5f && angle < 22.5f)
        {
            spriteIndex = 0; // East
            Debug.Log($"East");
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            spriteIndex = 1; // NorthEast
            Debug.Log($"NorthEast");
        }
        else if (angle >= 67.5f && angle < 112.5f)
        {
            spriteIndex = 2; // North
            Debug.Log($"North");
        }
        else if (angle >= 112.5f && angle < 157.5f)
        {
            spriteIndex = 3; // NorthWest
            Debug.Log($"NorthWest");
        }
        else if (angle >= 157.5f || angle < -157.5f)
        {
            spriteIndex = 4; // West
            Debug.Log($"West");
        }
        else if (angle >= -157.5f && angle < -112.5f)
        {   
            spriteIndex = 5; // SouthWest
            Debug.Log($"SouthWest");
        }
        else if (angle >= -112.5f && angle < -67.5f)
        {
            spriteIndex = 6; // South
            Debug.Log($"South");
        }
        else if (angle >= -67.5f && angle < -22.5f)
        {
            spriteIndex = 7; // SouthEast
            Debug.Log($"SouthEast");
        }
        
    }
}
