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

    void OnEnable()//Ǯ���� ���� �� �ʱ�ȭ
    {
        moveDir = Vector2.zero;
    }

    //�ڱ��ڽ� ���� �Լ�
    public void DestroySelf()
    {

        // Invoke ��� (�ߺ� ���� ����)
        CancelInvoke(nameof(DestroySelf));
        Debug.Log("����ó��");
        // ������Ʈ ����
        GetComponent<CustomPooledObject>()?.Release(); //�ٽ� Ǯ�� ����

    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyHitBox"))
        {
            EntityStats es = collision.GetComponentInParent<EntityStats>();
            es.TakeDamage(damage);
            Debug.Log("�浹ó��");
            DestroySelf(); // �ٷ� ����
        }
    }
}
