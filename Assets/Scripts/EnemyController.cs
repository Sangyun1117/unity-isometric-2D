using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using URandom = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    EntityStats stats;
    [Header("Direction")]
    [SerializeField] private Vector2 direction = Vector2.zero;
    private bool isChasing = false;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private GameObject waypoints;
    private GameObject currentWaypoint;
    Rigidbody2D rb;
    private Animator animator;
    public void OnPlayerEntered()
    {
        isChasing = true;
        animator.SetBool("isChasing", isChasing);
        Debug.Log("플레이어 감지! 추적 시작");
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
    }


    void Update()
    {
        if (isChasing && playerTarget != null)
        {
            // 플레이어 방향 구하기
            direction = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
            animator.SetFloat("moveX", rb.linearVelocity.x);
            animator.SetFloat("moveY", rb.linearVelocity.y);
            //transform.Translate(direction * moveSpeed * Time.deltaTime);

            // 마지막 이동 방향 기록
            if (direction != Vector2.zero)
            {
                animator.SetFloat("lastMoveX", direction.x);
                animator.SetFloat("lastMoveY", direction.y);
            }
        }
        else
        {
            //목표 지점에 도착했다면 새로운 목표지점으로 이동
            if(Vector2.Distance(transform.position, currentWaypoint.transform.position) < 0.1f)
            {
                SelectNewWaypoint();
            }

            //목표 지점 방향 구하기
            direction = ((Vector2)currentWaypoint.transform.position - (Vector2)transform.position).normalized;

            animator.SetFloat("moveX", rb.linearVelocity.x);
            animator.SetFloat("moveY", rb.linearVelocity.y);
            //transform.Translate(direction * moveSpeed * Time.deltaTime);

            // 마지막 이동 방향 기록
            if (direction != Vector2.zero)
            {
                animator.SetFloat("lastMoveX", direction.x);
                animator.SetFloat("lastMoveY", direction.y);
            }
        }
    }

    private void FixedUpdate()
    {

        rb.linearVelocity = isChasing ? direction * stats.runSpeed : direction * stats.walkSpeed;
    }

    private void SelectNewWaypoint()
    {
        if (waypoints.transform.childCount == 0) return;
        int newIndex;
        do
        {
            newIndex = URandom.Range(0, waypoints.transform.childCount);
        } while (waypoints.transform.GetChild(newIndex).gameObject == currentWaypoint); // 이전 목표와 같은 거 선택 방지

        currentWaypoint = waypoints.transform.GetChild(newIndex).gameObject;
    }
}
