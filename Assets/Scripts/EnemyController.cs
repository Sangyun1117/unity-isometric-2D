using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using System.Collections;
using URandom = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    protected EntityStats stats;
    [Header("Direction")]
    [SerializeField] public Vector2 direction = Vector2.zero;
    protected bool isChasing = false;
    [SerializeField] protected Transform playerTarget;
    [SerializeField] protected GameObject waypoints;
    protected GameObject currentWaypoint;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected ActionState actionState = ActionState.Idle;

    float lastAttackTime = -999f;//���ӽ��� �� ��� ���� �����ϵ��� ���� ���ڸ� ����
    Coroutine attackRoutine;
    [SerializeField] protected float attackCooldown = 1f;
    public void OnPlayerEntered()
    {
        isChasing = true;
        animator.SetBool("isChasing", isChasing);
        Debug.Log("�÷��̾� ����! ���� ����");
    }
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<EntityStats>();

        //��ǥ ���� ����
        if (waypoints != null && waypoints.transform.childCount > 0)
        {
            int index = URandom.Range(0, waypoints.transform.childCount);
            currentWaypoint = waypoints.transform.GetChild(index).gameObject;
            Debug.Log("������ �ڽ�: " + currentWaypoint.name);
        }
        else
        {
            Debug.LogWarning("waypoints ���ų� �ڽ��� �����ϴ�.");
        }
    }


    protected virtual void Update()
    {
        if (isChasing && playerTarget != null)
        {
            // �÷��̾� ���� ���ϱ�
            direction = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
            animator.SetFloat("moveX", rb.linearVelocity.x);
            animator.SetFloat("moveY", rb.linearVelocity.y);
            //transform.Translate(direction * moveSpeed * Time.deltaTime);

            // ������ �̵� ���� ���
            if (direction != Vector2.zero)
            {
                animator.SetFloat("lastMoveX", direction.x);
                animator.SetFloat("lastMoveY", direction.y);
            }
        }
        else
        {
            //��ǥ ������ �����ߴٸ� ���ο� ��ǥ�������� �̵�
            if(Vector2.Distance(transform.position, currentWaypoint.transform.position) < 0.1f)
            {
                SelectNewWaypoint();
            }

            //��ǥ ���� ���� ���ϱ�
            direction = ((Vector2)currentWaypoint.transform.position - (Vector2)transform.position).normalized;

            animator.SetFloat("moveX", rb.linearVelocity.x);
            animator.SetFloat("moveY", rb.linearVelocity.y);
            //transform.Translate(direction * moveSpeed * Time.deltaTime);

            // ������ �̵� ���� ���
            if (direction != Vector2.zero)
            {
                animator.SetFloat("lastMoveX", direction.x);
                animator.SetFloat("lastMoveY", direction.y);
            }
        }
    }

    private void FixedUpdate()
    {
        if(actionState!=ActionState.Shoot)
            rb.linearVelocity = isChasing ? direction * stats.runSpeed : direction * stats.walkSpeed;
    }

     protected void SelectNewWaypoint()
    {
        if (waypoints.transform.childCount == 0) return;
        int newIndex;
        do
        {
            newIndex = URandom.Range(0, waypoints.transform.childCount);
        } while (waypoints.transform.GetChild(newIndex).gameObject == currentWaypoint); // ���� ��ǥ�� ���� �� ���� ����

        currentWaypoint = waypoints.transform.GetChild(newIndex).gameObject;
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
