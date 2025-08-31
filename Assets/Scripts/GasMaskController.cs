using UnityEngine;
using System.Collections;
public class GasMaskController : EnemyController
{
    [SerializeField] Transform firePoint; // 발사 위치 지정 (총구)
    CustomObjectPool bulletPool;
    private ActionState actionState = ActionState.Idle;
    protected override void Awake()
    {
        base.Awake();
        bulletPool = GetComponent<CustomObjectPool>();
    }

    protected override void Update()
    {
        if (isChasing)
        {
            if (Vector2.Distance(transform.position, playerTarget.transform.position) < stats.attackRange)
            {

                animator.SetFloat("lastMoveX", direction.x);
                animator.SetFloat("lastMoveY", direction.y);
                actionState = ActionState.Shoot;

                animator.SetInteger("actionState", (int)actionState);
                ////사격중이면 이동금지
                direction.x = 0f;
                direction.y = 0f;
                animator.SetFloat("moveX", 0f);
                animator.SetFloat("moveY", 0f);

                StartAttack();
                return;
            }
            else if (Vector2.Distance(transform.position, playerTarget.transform.position) >= stats.attackRange && actionState == ActionState.Shoot)
            {
                StopAttack();
            }
            // 사격 중이면 이동 입력 읽지 않음
            if (actionState == ActionState.Shoot)
            {
                return;
            }
        }
        base.Update();
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
