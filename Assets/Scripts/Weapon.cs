using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    float lastAttackTime = -999f;//���ӽ��� �� ��� ���� �����ϵ��� ���� ���ڸ� ����
    Coroutine attackRoutine;
    [SerializeField] protected float attackCooldown = 0.5f;

    protected PlayerController player;

    protected virtual void Awake()
    {
        player = transform.root.GetComponent<PlayerController>();
    }
    private void OnEnable()
    {
        if (player == null)
            player = transform.root.GetComponent<PlayerController>();

        if (player != null)
            player.OnAnimatorParamChanged += HandleAnimatorParamChanged;
    }

    private void OnDisable()
    {
        if (player == null)
            player = transform.root.GetComponent<PlayerController>();

        if (player != null)
            player.OnAnimatorParamChanged -= HandleAnimatorParamChanged;
    }

    private void HandleAnimatorParamChanged(string paramName, object value)
    {
        // ���� �� �ִϸ��̼ǿ��� �ݿ�
        if (value is float f)
            GetComponent<Animator>().SetFloat(paramName, f);
        else if (value is int i)
            GetComponent<Animator>().SetInteger(paramName, i);
        else if (value is bool b)
            GetComponent<Animator>().SetBool(paramName, b);

        // �ʿ�� �����
        //Debug.Log($"Weapon Animator �ݿ�: {paramName} = {value}");
    }

    public virtual bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    protected virtual IEnumerator TryAttack()
    {
        yield break;
    }

    public void StartAttack()
    {
        if (CanAttack() && attackRoutine == null)
        { // �̹� ���� ������ ���� �� ����
            lastAttackTime = Time.time;//���� ���� ���� ���
            attackRoutine = StartCoroutine(AttackCoroutine());
        }
    }
    private IEnumerator AttackCoroutine()
    {
        yield return StartCoroutine(TryAttack()); // ���� ���� ���� ����
        attackRoutine = null; // ���� �Ϸ� �� null�� ����
    }
    public void StopAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }
}