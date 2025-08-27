using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Direction")]
    [SerializeField] private Vector2 direction = Vector2.zero;
    private bool isChasing = false;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private int hp = 100;
    Rigidbody2D rb;
    private Animator animator;
    public void OnPlayerEntered()
    {
        isChasing = true;
        Debug.Log("플레이어 감지! 추적 시작");
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        if (hp <= 0)
        {
            Debug.Log("적 사망");
            // 오브젝트 삭제
            Destroy(gameObject);
        }
        if (isChasing && playerTarget != null)
        {
            // 플레이어 방향 구하기
            direction = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
            animator.SetFloat("moveX", rb.linearVelocity.x);
            animator.SetFloat("moveY", rb.linearVelocity.y);
            //transform.Translate(direction * moveSpeed * Time.deltaTime);

            // 마지막 이동 방향 기록
            if (direction != Vector2.zero)
            {
                animator.SetFloat("lastMoveX", direction.x);
                animator.SetFloat("lastMoveY", direction.y);
            }
        }
        else
        {
            direction = Vector2.zero;
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction * moveSpeed;
    }
}
