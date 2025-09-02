using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class GasMaskController : EnemyController
{
    [SerializeField] Transform firePoint; // �߻� ��ġ ���� (�ѱ�)
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
            // NavMesh�� �÷��̾� ����
            agent.SetDestination(playerTarget.position);

            // �ִϸ��̼ǿ� ���� ��� (NavMesh �̵� ���� ����)
            Vector2 navMeshVelocity = agent.velocity;
            animator.SetFloat("moveX", navMeshVelocity.x);
            animator.SetFloat("moveY", navMeshVelocity.y);

            //��� ���� ���� �ִ� �� Ȯ��
            if (Vector2.Distance(transform.position, playerTarget.transform.position) < stats.attackRange)
            {
                // ��� �� NavMesh ����
                agent.isStopped = true;
                // �÷��̾� �������� �ٶ󺸱� (��ݿ�)
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
                // ��� ���� ���̸� NavMesh �����
                agent.isStopped = false;

                StopAttack();

                // ������ �̵� ���� ��� (NavMesh ���� ����)
                if (navMeshVelocity != Vector2.zero)
                {
                    animator.SetFloat("lastMoveX", navMeshVelocity.normalized.x);
                    animator.SetFloat("lastMoveY", navMeshVelocity.normalized.y);
                }
            }
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

    protected override void FixedUpdate()
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
            // Rigidbody2D ���� �ӵ� ���� ����
        }
    }

    //�ִϸ��̼� end �̺�Ʈ�� ȣ��
    public void OnShootEnd()
    {
        actionState = ActionState.Idle;
        animator.SetInteger("actionState", (int)actionState);
    }

    protected override IEnumerator TryAttack()
    {

        SetFirePoint();//������ �߻� ���� ��ġ ���ϱ�

        // �߻�ü ����
        CustomPooledObject bullet = bulletPool.GetPooledObject();
        bullet.transform.position = firePoint.position;

        // ȸ�� ��� (2D���� Z�� ȸ��)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Projectile ��ũ��Ʈ�� ���� ����
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

        // ���� ������ ����� ��ȯ (0~360��)
        if (angle < 0) angle += 360f;

        // 8�������� �з� (�� ����� 45����)
        // ������ ������(0��)���� �ݽð�������� ����
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