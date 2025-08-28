using UnityEngine;

public class CustomPooledObject : MonoBehaviour
{
    private CustomObjectPool pool;

    public CustomObjectPool Pool
    {
        get { return pool; }
        set { pool = value; }
    }

    public void Release()
    {
        pool.ReturnToPool(this);
    }
}
