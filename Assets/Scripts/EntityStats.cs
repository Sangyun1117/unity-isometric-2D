using UnityEngine;
using UnityEngine.UI;

public class EntityStats : MonoBehaviour
{
    [Header("기본 스탯")]
    [SerializeField] public int maxHP = 100;
    public int currentHP;
    [SerializeField] public float walkSpeed = 4f; //걷기 속도
    [SerializeField] public float runSpeed = 7f; //달리기 속도
    [SerializeField] public float attackPower = 10f; //공격력
    [SerializeField] public float attackSpeed = 1f; //공격 속도
    [SerializeField] public float criticalChance = 0.1f;   // 치명타 확률 (10%)
    [SerializeField] public float criticalMultiplier = 2f; // 치명타 배율
    [SerializeField] public float defense = 5f; //방어력
    [SerializeField] public float evadeChance = 0.05f;//회피 확률

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

    //피격
    public void TakeDamage(float damage)
    {
        // 방어력 적용
        float finalDamage = Mathf.Max(damage - defense, 1f);

        // 회피 여부
        if (Random.value < evadeChance)
        {
            Debug.Log($"{gameObject.name} 회피 성공!");
            return;
        }

        // 치명타 여부
        if (Random.value < criticalChance)
        {
            finalDamage *= criticalMultiplier;
            Debug.Log($"{gameObject.name} 치명타! {finalDamage} 피해");
        }

        currentHP = Mathf.Max(currentHP - (int)finalDamage, 0);
        Debug.Log($"{gameObject.name} HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
            Die();

        SetHP();
    }

    //회복
    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + (int)amount, maxHP);
        Debug.Log($"{gameObject.name} 회복! HP: {currentHP}/{maxHP}");

        SetHP();
    }

    //사망 처리
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} 사망");
        Destroy(gameObject);
    }
}
