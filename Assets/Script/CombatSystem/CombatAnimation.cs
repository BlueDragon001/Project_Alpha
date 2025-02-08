using UnityEngine;
using System.Collections;

public class CombatAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAttackAnimation()
    {
        animator.SetBool(StaticStrings.aniCombatMode, true);
        StartCoroutine(ResetAnimationBool(StaticStrings.aniCombatMode));

    }

    public void PlayConsecutiveAttackAniamtion()
    {
        animator.SetBool(StaticStrings.aniCombatMode, true);
        animator.SetBool(StaticStrings.aniConsecutiveAttack, true);
        StartCoroutine(ResetAnimationBool(StaticStrings.aniCombatMode));
    }

    public void PlayJumpAttackAniamtion()
    {
        animator.SetBool(StaticStrings.aniCombatMode, true);
        animator.SetBool(StaticStrings.aniJump, true);
        StartCoroutine(ResetAnimationBool(StaticStrings.aniCombatMode));
    }



    public void PlayBlockAnimation()
    {
        animator.SetBool(StaticStrings.aniBlock, true);
        StartCoroutine(ResetAnimationBool(StaticStrings.aniBlock));
    }

    public void PlayWalkAnimation(bool isForward)
    {
        animator.SetBool(StaticStrings.aniMove, true);
        animator.SetBool(StaticStrings.aniMoveForward, isForward);
    }

    public void PlayRunAnimation(bool isForward)
    {
        animator.SetBool(StaticStrings.aniMove, true);
        animator.SetBool(StaticStrings.aniRunForward, isForward);
    }


    public void StopMoveAnimation()
    {
        StartCoroutine(ResetAnimationBool(StaticStrings.aniMove));
    }




    private IEnumerator ResetAnimationBool(string parameter)
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        animator.SetBool(parameter, false);
    }
}
