using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private ObjectMovement objectMovement;
    private void Awake()
    {
        objectMovement = GetComponent<ObjectMovement>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");        
        float y = Input.GetAxisRaw("Vertical");
        
        objectMovement.MoveTo(new Vector3(x, y, 0));
    }
}
