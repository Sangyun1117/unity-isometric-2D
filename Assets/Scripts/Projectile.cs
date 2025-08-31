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

    void OnEnable()//풀에서 나올 때 초기화
    {
        moveDir = Vector2.zero;
    }

    //자기자신 삭제 함수
    public void DestroySelf()
    {

        // Invoke 취소 (중복 삭제 방지)
        CancelInvoke(nameof(DestroySelf));
        Debug.Log("삭제처리");
        // 오브젝트 삭제
        GetComponent<CustomPooledObject>()?.Release(); //다시 풀로 돌라감

    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyHitBox"))
        {
            EntityStats es = collision.GetComponentInParent<EntityStats>();
            es.TakeDamage(damage);
            Debug.Log("충돌처리");
            DestroySelf(); // 바로 삭제
        }
    }
}
