using UnityEngine;
using UnityEngine.UI;

public class EntityStats : MonoBehaviour
{
    [Header("�⺻ ����")]
    [SerializeField] public int maxHP = 100;
    public int currentHP;
    [SerializeField] public float walkSpeed = 4f; //�ȱ� �ӵ�
    [SerializeField] public float runSpeed = 7f; //�޸��� �ӵ�
    [SerializeField] public float attackPower = 10f; //���ݷ�
    [SerializeField] public float attackSpeed = 1f; //���� �ӵ�
    [SerializeField] public float criticalChance = 0.1f;   // ġ��Ÿ Ȯ�� (10%)
    [SerializeField] public float criticalMultiplier = 2f; // ġ��Ÿ ����
    [SerializeField] public float defense = 5f; //����
    [SerializeField] public float evadeChance = 0.05f;//ȸ�� Ȯ��

    [SerializeField] public float attackRange = 9;

    [SerializeField] private Image hpFillImage;


    private void Awake()
    {
        currentHP = maxHP;
    }

    public void SetHP()
    {
        hpFillImage.fillAmount = (float)currentHP / maxHP;
    }

    //�ǰ�
    public void TakeDamage(float damage)
    {
        // ���� ����
        float finalDamage = Mathf.Max(damage - defense, 1f);

        // ȸ�� ����
        if (Random.value < evadeChance)
        {
            Debug.Log($"{gameObject.name} ȸ�� ����!");
            return;
        }

        // ġ��Ÿ ����
        if (Random.value < criticalChance)
        {
            finalDamage *= criticalMultiplier;
            Debug.Log($"{gameObject.name} ġ��Ÿ! {finalDamage} ����");
        }

        currentHP = Mathf.Max(currentHP - (int)finalDamage, 0);
        Debug.Log($"{gameObject.name} HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
            Die();

        SetHP();
    }

    //ȸ��
    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + (int)amount, maxHP);
        Debug.Log($"{gameObject.name} ȸ��! HP: {currentHP}/{maxHP}");

        SetHP();
    }

    //��� ó��
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} ���");
        Destroy(gameObject);
    }
}
