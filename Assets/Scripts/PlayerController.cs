using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

// 8방향 열거형 정의
public enum Direction8
{
    None,       // 정지
    Up,         // 위
    UpRight,    // 오른쪽 위
    Right,      // 오른쪽
    DownRight,  // 오른쪽 아래
    Down,       // 아래
    DownLeft,   // 왼쪽 아래
    Left,       // 왼쪽
    UpLeft      // 왼쪽 위
}
enum ActionState
{
    Idle = 0,
    Shoot = 1,
    Reload = 2,
    Dash = 3
}
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    PlayerStats playerStatus;
    WeaponManager weaponManager;
    int nowInventoryItem = 0;
    private Rigidbody2D rb;
    public Vector2 direction;
    public Vector2 lastMoveDir;
    private ActionState actionState = ActionState.Idle;
    private bool isRun = false;
    private bool runCheck = false;
    private Animator bodyAnimator;

    //애니메이션 상태를 옵저버 형태로 알림
    public event Action<string, object> OnAnimatorParamChanged;
    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyAnimator = GetComponent<Animator>();
        playerStatus = GetComponent<PlayerStats>();
        weaponManager = transform.Find("Weapons")?.GetComponent<WeaponManager>();
    }
    public void Update()
    {
        //달리기 : shift 키 누르면 뛰기, 아이템 교체시 눌려있는 상태면 다시 뜀
        if (Input.GetKeyDown(KeyCode.LeftShift)||(runCheck&&Input.GetKey(KeyCode.LeftShift)))
        {
            isRun = true;
            SetAnimParam("isRun", isRun);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRun = false;
            SetAnimParam("isRun", isRun);
        }

        //공격 : 마우스 좌클릭 누르면 사격
        if (Input.GetMouseButton(0)) 
        {
            //플레이어 위치에서 부터 마우스 클릭된 방향 구하기
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lastMoveDir = (mouseWorld - transform.position);
            lastMoveDir.Normalize();

            SetAnimParam("lastMoveX", lastMoveDir.x);
            SetAnimParam("lastMoveY", lastMoveDir.y);
            actionState = ActionState.Shoot;

            SetAnimParam("actionState", (int)actionState);
            ////사격중이면 이동금지
            direction.x = 0f;
            direction.y = 0f;
            SetAnimParam("moveX", 0f);
            SetAnimParam("moveY", 0f);
            weaponManager.GetWeapon().StartAttack();
            return;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            weaponManager.GetWeapon().StopAttack();
        }
        // 사격 중이면 이동 입력 읽지 않음
        if (actionState == ActionState.Shoot)
        {
            return;
        }

        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                ItemSwap(i);
                Debug.Log("숫자 " + i + " 키 입력됨");
            }
        }

        //이동 : WASD, 방향키 눌러서 이동
        direction.x = Input.GetAxisRaw("Horizontal");
        direction.y = Input.GetAxisRaw("Vertical");

        // 현재 이동 벡터
        Vector2 moveDir = new Vector2(direction.x, direction.y);

        // Animator에 현재 속도 넣어주기
        SetAnimParam("moveX", rb.linearVelocity.x);
        SetAnimParam("moveY", rb.linearVelocity.y);

        // 마지막 이동 방향 기록
        if (moveDir != Vector2.zero)
        {
            lastMoveDir = direction;
            SetAnimParam("lastMoveX", lastMoveDir.x);
            SetAnimParam("lastMoveY", lastMoveDir.y);
        }
    }

    public void FixedUpdate()
    {
        // 사격 중일 때는 이동하지 않음
        if (actionState == ActionState.Shoot)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 moveDir = new Vector2(direction.x, direction.y).normalized;
        rb.linearVelocity = isRun ? moveDir * playerStatus.runSpeed : moveDir * playerStatus.walkSpeed;
    }

    //애니메이션 end 이벤트로 호출
    public void OnShootEnd()
    {
        actionState = ActionState.Idle;
        SetAnimParam("actionState", (int)actionState);
    }

    public Direction8 GetAnimationDirection8()
    {
        //방향이 없는 경우 오류 방지
        if (lastMoveDir == Vector2.zero)
            return Direction8.None;

        float angle = Mathf.Atan2(lastMoveDir.y, lastMoveDir.x) * Mathf.Rad2Deg;

        // 음수 각도를 양수로 변환 (0~360도)
        if (angle < 0)
            angle += 360f;

        // 8방향으로 분류 (각 방향당 45도씩)
        // 각도는 오른쪽(0도)부터 반시계방향으로 증가
        if (angle >= 337.5f || angle < 22.5f)
            return Direction8.Right;
        else if (angle >= 22.5f && angle < 67.5f)
            return Direction8.UpRight;
        else if (angle >= 67.5f && angle < 112.5f)
            return Direction8.Up;
        else if (angle >= 112.5f && angle < 157.5f)
            return Direction8.UpLeft;
        else if (angle >= 157.5f && angle < 202.5f)
            return Direction8.Left;
        else if (angle >= 202.5f && angle < 247.5f)
            return Direction8.DownLeft;
        else if (angle >= 247.5f && angle < 292.5f)
            return Direction8.Down;
        else // 292.5f <= angle < 337.5f
            return Direction8.DownRight;
    }

    public void SetAnimParam<T>(string paramName, T value)
    {
        if (typeof(T) == typeof(float))
        {
            bodyAnimator.SetFloat(paramName, (float)(object)value);
        }
        else if (typeof(T) == typeof(int))
        {
            bodyAnimator.SetInteger(paramName, (int)(object)value);
        }
        else if (typeof(T) == typeof(bool))
        {
            bodyAnimator.SetBool(paramName, (bool)(object)value);
        }
        else
        {
            Debug.LogWarning($"Animator 파라미터 타입 {typeof(T)} 은 지원하지 않음");
        }

        //옵저버 이벤트 발행 가능
        OnAnimatorParamChanged?.Invoke(paramName, value);
    }

    public void ItemSwap(int index)
    {
        //아이템 교체시 애니메이션 싱크를 맞추기 위해 Idle 상태로 돌아감
        nowInventoryItem = index;
        weaponManager.SetWeapon(index);
        isRun = false;
        runCheck = true; //쉬프트가 눌려있는지 다시 확인함
        SetAnimParam("isRun", isRun);
        actionState = ActionState.Idle;
        SetAnimParam("lastMoveX", lastMoveDir.x);
        SetAnimParam("lastMoveY", lastMoveDir.y);
        direction.x = 0f;
        direction.y = 0f;
        SetAnimParam("moveX", rb.linearVelocity.x);
        SetAnimParam("moveY", rb.linearVelocity.y);
    }
}
