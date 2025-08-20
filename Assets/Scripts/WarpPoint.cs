using UnityEngine;
using UnityEngine.Tilemaps;

public class WarpPoint : MonoBehaviour
{
    [Header("이동할 씬 이름")]
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

    // 플레이어가 영역 안으로 들어왔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // 플레이어가 영역 밖으로 나갔을 때
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        // 영역 안에 있고 E버튼 누르면 씬 전환
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            GameManager.Instance.ChangeScene(targetScene);
        }
    }
}
