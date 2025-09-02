using System;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;
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
    //private Rigidbody2D rb;
    protected Animator animator;
    protected ActionState actionState = ActionState.Idle;

    float lastAttackTime = -999f;//���ӽ��� �� ��� ���� �����ϵ��� ���� ���ڸ� ����
    Coroutine attackRoutine;
    [SerializeField] protected float attackCooldown = 1f;

    protected NavMeshAgent agent;

    protected virtual void Awake()
    {
        //rb = GetComponent<Rigidbody2D>();
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

        agent = GetComponent<NavMeshAgent>();
        //NavMesh ���� Ageng ��ü�� �ٰ��ϰ� ������ ��Ҵ� �״�� �ֵ��� �ϴ� �ڵ�

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.speed = stats.walkSpeed; // �̵��ӵ�
        agent.stoppingDistance = 0.1f; // �������� �� �Ÿ� ���Ϸ� ��������� ���� (�⺻�� 0f)
        agent.acceleration = 20f; //�ӵ��� ���� �� ���ӵ� (�⺻�� 8f). 0�� ������ �ε巴�� �����̰� Ŀ���� �ް��ϰ� ����/����

        agent.SetDestination(currentWaypoint.transform.position);
    }


    protected virtual void Update()
    {
        if (isChasing && playerTarget != null)
        {
            // �÷��̾� ���� ���ϱ�
            agent.SetDestination(playerTarget.position);

            // �ִϸ��̼ǿ� ���� ��� (NavMesh �̵� ���� ����)
            Vector2 navMeshVelocity = agent.velocity;
            animator.SetFloat("moveX", navMeshVelocity.x);
            animator.SetFloat("moveY", navMeshVelocity.y);
        }
        else
        {
            //��ǥ ������ �����ߴٸ� ���ο� ��ǥ�������� �̵�
            if (Vector2.Distance(transform.position, currentWaypoint.transform.position) < 0.1f)
            {
                SelectNewWaypoint();
                agent.SetDestination(currentWaypoint.transform.position);
            }

            //��ǥ ���� ���� ���ϱ�
            direction = ((Vector2)currentWaypoint.transform.position - (Vector2)transform.position).normalized;

            animator.SetFloat("moveX", agent.velocity.x);
            animator.SetFloat("moveY", agent.velocity.y);

            // ������ �̵� ���� ���
            if (direction != Vector2.zero)
            {
                animator.SetFloat("lastMoveX", direction.x);
                animator.SetFloat("lastMoveY", direction.y);
            }
        }
    }

    protected virtual void FixedUpdate()
    {

        if (actionState == ActionState.Shoot)
        {
            // �̵� ����
            agent.isStopped = true;
            return;
        }
        else
        {
            // NavMeshAgent�� �̿��� �̵�
            agent.isStopped = false;

        }
    }

    public void OnPlayerEntered()
    {
        isChasing = true;
        agent.speed = stats.runSpeed;
        animator.SetBool("isChasing", isChasing);
        Debug.Log("�÷��̾� ����! ���� ����");
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
