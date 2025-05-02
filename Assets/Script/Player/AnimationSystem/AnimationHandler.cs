using UnityEngine;

/// <summary>
/// Handles animation state changes and transitions for the character.
/// Works with Unity's Animator component to manage combat and movement animations.
/// </summary>
public class AnimationHandler : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Triggers attack animation and handles combo state.
    /// Returns the length of the current animation.
    /// </summary>
    public float AttackAnimation()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack")) { animator.SetBool("ComboAttack", true); }
        else { animator.SetTrigger("Attack"); animator.SetBool("ComboAttack", false); }
        return stateInfo.length;
    }

    /// <summary>
    /// Triggers jump attack animation and returns its duration.
    /// </summary>
    public float JumpAnimation()
    {
        animator.SetTrigger("JumpAttack");
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.length;
    }

    /// <summary>
    /// Updates movement animation states based on input direction.
    /// </summary>
    /// <param name="moveInput">2D movement input vector</param>
    public void MoveAnimation(Vector2 moveInput)
    {
        if (moveInput.y > 0 || moveInput.x > 0) animator.SetBool("RunForward", true);
        if (moveInput.y < 0 || moveInput.x < 0) animator.SetBool("RunBackward", true);
    }

    /// <summary>
    /// Resets movement animation states to idle.
    /// </summary>
    public void Idle()
    {
        animator.SetBool("RunForward", false);
        animator.SetBool("RunBackward", false);
    }
}
