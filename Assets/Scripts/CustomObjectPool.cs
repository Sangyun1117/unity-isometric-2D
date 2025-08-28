using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CustomObjectPool : MonoBehaviour
{
    [SerializeField] private uint initPoolSize;
    [SerializeField] private CustomPooledObject objectToPool;
    private Stack<CustomPooledObject> stack;

    private void Start()
    {
        SetupPool();
    }

    //풀에 오브젝트 미리 만들어두기
    private void SetupPool()
    {
        stack = new Stack<CustomPooledObject>();
        CustomPooledObject instance = null;

        for (int i = 0; i < initPoolSize; ++i)
        {
            instance = Instantiate(objectToPool);
            instance.Pool = this;
            instance.gameObject.SetActive(false);

            stack.Push(instance);
        }
    }

    //풀에서 오브젝트 요청
    public CustomPooledObject GetPooledObject()
    {
        CustomPooledObject instance = null;
        if (stack.Count <= 0)
        {
            instance = Instantiate(objectToPool);
            instance.Pool = this;
            return instance;
        }

        instance = stack.Pop();
        instance.gameObject.SetActive(true);

        return instance;
    }

    public void ReturnToPool(CustomPooledObject pooledObject)
    {
        stack.Push(pooledObject);
        pooledObject.gameObject.SetActive(false);
    }
}
