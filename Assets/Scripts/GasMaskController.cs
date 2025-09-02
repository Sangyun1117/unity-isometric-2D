using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class GasMaskController : EnemyController
{
    [SerializeField] Transform firePoint; // 발사 위치 지정 (총구)
    CustomObjectPool bulletPool;
    protected override void Awake()
    {
        base.Awake();
        bulletPool = GetComponent<CustomObjectPool>();

    }

    protected override void Update()
    {
        if (isChasing)
        {
            // NavMesh로 플레이어 추적
            agent.SetDestination(playerTarget.position);

            // 애니메이션용 방향 계산 (NavMesh 이동 방향 기준)
            Vector2 navMeshVelocity = agent.velocity;
            animator.SetFloat("moveX", navMeshVelocity.x);
            animator.SetFloat("moveY", navMeshVelocity.y);

            //사격 범위 내에 있는 지 확인
            if (Vector2.Distance(transform.position, playerTarget.transform.position) < stats.attackRange)
            {
                // 사격 시 NavMesh 정지
                agent.isStopped = true;
                // 플레이어 방향으로 바라보기 (사격용)
                direction = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;

                StartAttack();
                animator.SetFloat("lastMoveX", direction.x);
                animator.SetFloat("lastMoveY", direction.y);
                actionState = ActionState.Shoot;
                animator.SetInteger("actionState", (int)actionState);

                animator.SetFloat("moveX", 0f);
                animator.SetFloat("moveY", 0f);
                return;
            }
            else
            {
                // 사격 범위 밖이면 NavMesh 재시작
                agent.isStopped = false;

                StopAttack();

                // 마지막 이동 방향 기록 (NavMesh 방향 기준)
                if (navMeshVelocity != Vector2.zero)
                {
                    animator.SetFloat("lastMoveX", navMeshVelocity.normalized.x);
                    animator.SetFloat("lastMoveY", navMeshVelocity.normalized.y);
                }
            }
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

    protected override void FixedUpdate()
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
            // Rigidbody2D 직접 속도 세팅 제거
        }
    }

    //애니메이션 end 이벤트로 호출
    public void OnShootEnd()
    {
        actionState = ActionState.Idle;
        animator.SetInteger("actionState", (int)actionState);
    }

    protected override IEnumerator TryAttack()
    {

        SetFirePoint();//투사페 발사 시작 위치 구하기

        // 발사체 생성
        CustomPooledObject bullet = bulletPool.GetPooledObject();
        bullet.transform.position = firePoint.position;

        // 회전 계산 (2D에서 Z축 회전)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Projectile 스크립트에 방향 전달
        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetDirection(direction);
            //proj.SetDirection(Vector2.down);
        }

        yield break;
    }

    void SetFirePoint()
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 음수 각도를 양수로 변환 (0~360도)
        if (angle < 0) angle += 360f;

        // 8방향으로 분류 (각 방향당 45도씩)
        // 각도는 오른쪽(0도)부터 반시계방향으로 증가
        if (angle >= 337.5f || angle < 22.5f)
            firePoint.localPosition = new Vector2(0.3f, 0.087f);
        else if (angle >= 22.5f && angle < 67.5f)
            firePoint.localPosition = new Vector2(0.24f, 0.24f);
        else if (angle >= 67.5f && angle < 112.5f)
            firePoint.localPosition = new Vector2(0.058f, 0.34f);
        else if (angle >= 112.5f && angle < 157.5f)
            firePoint.localPosition = new Vector2(-0.16f, 0.29f);
        else if (angle >= 157.5f && angle < 202.5f)
            firePoint.localPosition = new Vector2(-0.285f, 0.144f);
        else if (angle >= 202.5f && angle < 247.5f)
            firePoint.localPosition = new Vector2(-0.23f, -0.02f);
        else if (angle >= 247.5f && angle < 292.5f)
            firePoint.localPosition = new Vector2(-0.025f, -0.1f);
        else // 292.5f <= angle < 337.5f
            firePoint.localPosition = new Vector2(0.2f, -0.05f);

    }
}