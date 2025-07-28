using UnityEngine;

public class ObjectMovement: MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private Vector3 moveDirection = Vector3.zero;

    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    public void MoveTo(Vector3 direction)
    {
        moveDirection = direction;
    }

}
