using UnityEngine;
using UnityEngine.Tilemaps;

public class WarpPoint : MonoBehaviour
{
    [Header("�̵��� �� �̸�")]
    [SerializeField] public string targetScene;

    private bool playerInRange = false;
    private SpriteRenderer spriteRenderer;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        spriteRenderer.enabled = false;
    }

    // �÷��̾ ���� ������ ������ ��
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // �÷��̾ ���� ������ ������ ��
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        // ���� �ȿ� �ְ� E��ư ������ �� ��ȯ
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            GameManager.Instance.ChangeScene(targetScene);
        }
    }
}
