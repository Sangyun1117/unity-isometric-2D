using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] int damage = 10;
    Vector2 moveDir = Vector2.zero;

    // ������ �ܺο��� ����
    public void SetDirection(Vector2 dir)
    {
        moveDir = dir.normalized;
    }

    void Update()
    {
        // ������ �������� �̵�
        transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
    }
    //�ڱ��ڽ� ���� �Լ�
    public void DestroySelf()
    {

        // Invoke ��� (�ߺ� ���� ����)
        CancelInvoke(nameof(DestroySelf));
        Debug.Log("����ó��");
        // ������Ʈ ����
        Destroy(gameObject);

    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyHitBox"))
        {
            EnemyController ec = collision.GetComponentInParent<EnemyController>();
            ec.TakeDamage(damage);
            Debug.Log("�浹ó��");
            DestroySelf(); // �ٷ� ����
        }
    }
}
