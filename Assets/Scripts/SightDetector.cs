using UnityEngine;

public class SightDetector : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController; // 인스펙터에서 할당

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemyController.OnPlayerEntered();
        }
    }
}
