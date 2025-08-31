using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    float lastAttackTime = -999f;//게임시작 시 즉시 공격 가능하도록 낮은 숫자를 넣음
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
        // 무기 쪽 애니메이션에도 반영
        if (value is float f)
            GetComponent<Animator>().SetFloat(paramName, f);
        else if (value is int i)
            GetComponent<Animator>().SetInteger(paramName, i);
        else if (value is bool b)
            GetComponent<Animator>().SetBool(paramName, b);

        // 필요시 디버깅
        //Debug.Log($"Weapon Animator 반영: {paramName} = {value}");
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
        { // 이미 돌고 있으면 새로 안 시작
            lastAttackTime = Time.time;//공격 시작 시점 기록
            attackRoutine = StartCoroutine(AttackCoroutine());
        }
    }
    private IEnumerator AttackCoroutine()
    {
        yield return StartCoroutine(TryAttack()); // 실제 공격 로직 실행
        attackRoutine = null; // 공격 완료 후 null로 설정
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