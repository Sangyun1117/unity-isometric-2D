using UnityEngine;

public class EnemyShootBehavior : StateMachineBehaviour
{
    private GasMaskController enemy;

    // Update is called once per frame
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemy == null)
            enemy = animator.GetComponent<GasMaskController>();

        if (enemy != null)
            enemy.isShootAnim = true; // 이동 금지
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemy == null)
            enemy = animator.GetComponent<GasMaskController>();

        if (enemy != null)
            enemy.isShootAnim = false; // 이동 허용
    }
}
