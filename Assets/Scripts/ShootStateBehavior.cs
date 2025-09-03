using UnityEngine;

public class ShootStateBehavior : StateMachineBehaviour
{
    private PlayerController player;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null)
            player = animator.GetComponent<PlayerController>();

        if (player != null)
            player.isShootAnim = true; // 이동 금지
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null)
            player = animator.GetComponent<PlayerController>();

        if (player != null)
            player.isShootAnim = false; // 이동 허용
    }
}
