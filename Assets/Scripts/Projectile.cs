using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] int damage = 10;
    [SerializeField] bool masterIsPlayer = true;
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

    void OnEnable()//Ǯ���� ���� �� �ʱ�ȭ
    {
        moveDir = Vector2.zero;
    }

    //�ڱ��ڽ� ���� �Լ�
    private void DestroySelf()
    {

        // Invoke ��� (�ߺ� ���� ����)
        CancelInvoke(nameof(DestroySelf));
        Debug.Log("�Ѿ� ����ó��");
        // ������Ʈ ����
        GetComponent<CustomPooledObject>()?.Release(); //�ٽ� Ǯ�� ����

    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (masterIsPlayer)
        {
            if (collision.CompareTag("EnemyHitBox"))
            {
                EntityStats es = collision.GetComponentInParent<EntityStats>();
                es.TakeDamage(damage);
                Debug.Log("Enemy �浹ó��");
                DestroySelf(); // �ٷ� ����
            }
        }
        else
        {
            if (collision.CompareTag("PlayerHitBox"))
            {
                EntityStats ps = collision.GetComponentInParent<EntityStats>();
                ps.TakeDamage(damage);
                Debug.Log("Player �浹ó��");
                DestroySelf(); // �ٷ� ����
            }
        }
    }
}
