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
        Debug.Log("�÷��̾� ����! ���� ����");
    }
    void Awake()
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


    void Update()
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

        rb.linearVelocity = isChasing ? direction * stats.runSpeed : direction * stats.walkSpeed;
    }

    private void SelectNewWaypoint()
    {
        if (waypoints.transform.childCount == 0) return;
        int newIndex;
        do
        {
            newIndex = URandom.Range(0, waypoints.transform.childCount);
        } while (waypoints.transform.GetChild(newIndex).gameObject == currentWaypoint); // ���� ��ǥ�� ���� �� ���� ����

        currentWaypoint = waypoints.transform.GetChild(newIndex).gameObject;
    }
}
