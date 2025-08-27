using UnityEngine;
using System.Collections;

public class Rifles : Weapon
{
    [SerializeField] GameObject projectile;
    [SerializeField] Transform firePoint; // 발사 위치 지정 (총구)
    //Vector2 firePoint = Vector2.zero;

    protected override IEnumerator TryAttack()
    {

        SetFirePoint(player.GetAnimationDirection8());//투사페 발사 시작 위치 구하기

        Vector2 aimDir = player.lastMoveDir;
        // 발사체 생성
        GameObject bullet = Instantiate(projectile, firePoint.transform.position, Quaternion.identity);

        // 회전 계산 (2D에서 Z축 회전)
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Projectile 스크립트에 방향 전달
        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetDirection(aimDir);
        }

        yield break;
    }

    void SetFirePoint(Direction8 direction)
    {
        Vector2 playerPos = player.transform.position;

        switch (direction)
        {
            case Direction8.Right:
                firePoint.localPosition = new Vector2(0.3f,0.087f);
                break;
            case Direction8.UpRight:
                firePoint.localPosition = new Vector2(0.24f, 0.24f);
                break;
            case Direction8.Up:
                firePoint.localPosition = new Vector2(0.058f, 0.34f);
                break;
            case Direction8.UpLeft:
                firePoint.localPosition = new Vector2(-0.16f, 0.29f);
                break;
            case Direction8.Left:
                firePoint.localPosition = new Vector2(-0.285f, 0.144f);
                break;
            case Direction8.DownLeft:
                firePoint.localPosition = new Vector2(-0.23f, -0.02f);
                break;
            case Direction8.Down:
                firePoint.localPosition = new Vector2(-0.025f, -0.1f);
                break;
            case Direction8.DownRight:
                firePoint.localPosition = new Vector2(0.2f, -0.05f);
                break;
        }

    }
}
