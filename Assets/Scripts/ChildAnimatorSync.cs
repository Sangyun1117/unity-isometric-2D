using UnityEngine;

//플레이어 애니메이션을 구독해서 변경이 있으면 같이 변경하는 컴포넌트
public class ChildAnimatorSync : MonoBehaviour
{
    protected PlayerController player;

    private void Awake()
    {
        player = transform.root.GetComponent<PlayerController>();
    }
    private void OnEnable()
    {
        player.OnAnimatorParamChanged += HandleAnimatorParamChanged;
    }

    private void OnDisable()
    {
        player.OnAnimatorParamChanged -= HandleAnimatorParamChanged;
    }

    private void HandleAnimatorParamChanged(string paramName, object value)
    {
        if (value is float f)
            GetComponent<Animator>().SetFloat(paramName, f);
        else if (value is int i)
            GetComponent<Animator>().SetInteger(paramName, i);
        else if (value is bool b)
            GetComponent<Animator>().SetBool(paramName, b);
    }
}
