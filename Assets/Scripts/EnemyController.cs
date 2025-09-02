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

    float lastAttackTime = -999f;//게임시작 시 즉시 공격 가능하도록 낮은 숫자를 넣음
    Coroutine attackRoutine;
    [SerializeField] protected float attackCooldown = 1f;

    protected NavMeshAgent agent;

    protected virtual void Awake()
    {
        //rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<EntityStats>();

        //목표 지점 선택
        if (waypoints != null && waypoints.transform.childCount > 0)
        {
            int index = URandom.Range(0, waypoints.transform.childCount);
            currentWaypoint = waypoints.transform.GetChild(index).gameObject;
            Debug.Log("선택한 자식: " + currentWaypoint.name);
        }
        else
        {
            Debug.LogWarning("waypoints 없거나 자식이 없습니다.");
        }

        agent = GetComponent<NavMeshAgent>();
        //NavMesh 사용시 Ageng 본체만 붙게하고 렌더러 요소는 그대로 있도록 하는 코드

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.speed = stats.walkSpeed; // 이동속도
        agent.stoppingDistance = 0.1f; // 목적지와 이 거리 이하로 가까워지면 멈춤 (기본값 0f)
        agent.acceleration = 20f; //속도가 변할 때 가속도 (기본값 8f). 0에 가까우면 부드럽게 움직이고 커지면 급격하게 가속/감속

        agent.SetDestination(currentWaypoint.transform.position);
    }


    protected virtual void Update()
    {
        if (isChasing && playerTarget != null)
        {
            // 플레이어 방향 구하기
            agent.SetDestination(playerTarget.position);

            // 애니메이션용 방향 계산 (NavMesh 이동 방향 기준)
            Vector2 navMeshVelocity = agent.velocity;
            animator.SetFloat("moveX", navMeshVelocity.x);
            animator.SetFloat("moveY", navMeshVelocity.y);
        }
        else
        {
            //목표 지점에 도착했다면 새로운 목표지점으로 이동
            if (Vector2.Distance(transform.position, currentWaypoint.transform.position) < 0.1f)
            {
                SelectNewWaypoint();
                agent.SetDestination(currentWaypoint.transform.position);
            }

            //목표 지점 방향 구하기
            direction = ((Vector2)currentWaypoint.transform.position - (Vector2)transform.position).normalized;

            animator.SetFloat("moveX", agent.velocity.x);
            animator.SetFloat("moveY", agent.velocity.y);

            // 마지막 이동 방향 기록
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
            // 이동 정지
            agent.isStopped = true;
            return;
        }
        else
        {
            // NavMeshAgent를 이용한 이동
            agent.isStopped = false;

        }
    }

    public void OnPlayerEntered()
    {
        isChasing = true;
        agent.speed = stats.runSpeed;
        animator.SetBool("isChasing", isChasing);
        Debug.Log("플레이어 감지! 추적 시작");
    }

    protected void SelectNewWaypoint()
    {
        if (waypoints.transform.childCount == 0) return;
        int newIndex;
        do
        {
            newIndex = URandom.Range(0, waypoints.transform.childCount);
        } while (waypoints.transform.GetChild(newIndex).gameObject == currentWaypoint); // 이전 목표와 같은 거 선택 방지

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
