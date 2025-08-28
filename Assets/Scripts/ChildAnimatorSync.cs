using UnityEngine;

//�÷��̾� �ִϸ��̼��� �����ؼ� ������ ������ ���� �����ϴ� ������Ʈ
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
