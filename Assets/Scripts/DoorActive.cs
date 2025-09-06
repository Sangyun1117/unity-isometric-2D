using UnityEngine;

public class DoorActive : MonoBehaviour
{
    [SerializeField] bool isOpen = false;
    [SerializeField] private Sprite closedSprite;   // 닫힌 문
    [SerializeField] private Sprite openSprite;     // 열린 문
    [SerializeField] private Vector3 moveOffset;     // 열린 문

    private SpriteRenderer spriteRenderer;
    private bool isPlayerNearby = false;            // 플레이어 감지 여부

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
        }
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            spriteRenderer.sprite = openSprite;
            transform.position += moveOffset;
        }
        else
        {
            spriteRenderer.sprite = closedSprite;
            transform.position -= moveOffset;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
