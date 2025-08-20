using UnityEngine;

public class SightDetector : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController; // �ν����Ϳ��� �Ҵ�

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemyController.OnPlayerEntered();
        }
    }
}
