using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void AttackAnimation()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack")) { animator.SetBool("ComboAttack", true); }
        else { animator.SetTrigger("Attack"); animator.SetBool("ComboAttack", false); }
    }
    public void JumpAnimation()
    {
        animator.SetTrigger("JumpAttack");
    }
    public void MoveAnimation(Vector2 moveInput)
    {
        if (moveInput.y > 0) animator.SetBool("RunForward", true);
        if (moveInput.y < 0) animator.SetBool("RunBackward", true);
    }
    public void Idle()
    {
        animator.SetBool("RunForward", false);
        animator.SetBool("RunBackward", false);
    }



}
