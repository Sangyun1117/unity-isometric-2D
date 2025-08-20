using UnityEngine;
enum ActionState
{
    Idle = 0,
    Shoot = 1,
    Reload = 2,
    Dash = 3
}
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private float x; //방향
    private float y;
    private ActionState actionState = ActionState.Idle;
    private Animator bodyAnimator;
    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyAnimator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (Input.GetMouseButton(0)) //마우스 좌클릭 사격
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 aimDir = (mouseWorld - transform.position);
            aimDir.Normalize();

            bodyAnimator.SetFloat("lastMoveX", aimDir.x);
            bodyAnimator.SetFloat("lastMoveY", aimDir.y);
            actionState = ActionState.Shoot;
            bodyAnimator.SetInteger("actionState", (int)actionState);

            ////사격중이면 이동금지
            x = 0f;
            y = 0f;
            bodyAnimator.SetFloat("moveX", 0f);
            bodyAnimator.SetFloat("moveY", 0f);

            return;
        }
        // 사격 중이면 이동 입력 읽지 않음
        if (actionState == ActionState.Shoot)
        {
            return;
        }
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");

        // 현재 이동 벡터
        Vector2 moveDir = new Vector2(x, y);

        // Animator에 현재 속도 넣어주기
        bodyAnimator.SetFloat("moveX", rb.linearVelocity.x);
        bodyAnimator.SetFloat("moveY", rb.linearVelocity.y);

        // 마지막 이동 방향 기록
        if (moveDir != Vector2.zero)
        {
            bodyAnimator.SetFloat("lastMoveX", x);
            bodyAnimator.SetFloat("lastMoveY", y);

        }
    }

    public void FixedUpdate()
    {
        // 사격 중일 때는 이동하지 않음
        if (actionState == ActionState.Shoot)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 moveDir = new Vector2(x, y).normalized;
        rb.linearVelocity = moveDir * moveSpeed;
    }

    //애니메이션 end 이벤트로 호출
    public void OnShootEnd()
    {
        actionState = ActionState.Idle;
        bodyAnimator.SetInteger("actionState", (int)actionState);


    }
    //private int GetDirectionIndex(Vector2 direction)
    //{
    //    direction.Normalize();
    //    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    //    if (angle >= -22.5f && angle < 22.5f) return 0; // East
    //    if (angle >= 22.5f && angle < 67.5f) return 1;  // NE
    //    if (angle >= 67.5f && angle < 112.5f) return 2; // N
    //    if (angle >= 112.5f && angle < 157.5f) return 3;// NW
    //    if (angle >= 157.5f || angle < -157.5f) return 4;// W
    //    if (angle >= -157.5f && angle < -112.5f) return 5;// SW
    //    if (angle >= -112.5f && angle < -67.5f) return 6;// S
    //    return 7; // SE
    //}
}
