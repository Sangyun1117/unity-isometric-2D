using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] int damage = 10;
    Vector2 moveDir = Vector2.zero;

    // 방향을 외부에서 세팅
    public void SetDirection(Vector2 dir)
    {
        moveDir = dir.normalized;
    }

    void Update()
    {
        // 지정된 방향으로 이동
        transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
    }
    //자기자신 삭제 함수
    public void DestroySelf()
    {

        // Invoke 취소 (중복 삭제 방지)
        CancelInvoke(nameof(DestroySelf));
        Debug.Log("삭제처리");
        // 오브젝트 삭제
        Destroy(gameObject);

    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyHitBox"))
        {
            EnemyController ec = collision.GetComponentInParent<EnemyController>();
            ec.TakeDamage(damage);
            Debug.Log("충돌처리");
            DestroySelf(); // 바로 삭제
        }
    }
}
