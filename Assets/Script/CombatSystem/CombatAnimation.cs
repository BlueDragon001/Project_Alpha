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
        animator.SetBool("Attack", true);
        StartCoroutine(ResetAnimationBool("Attack"));
    }

    public void PlayBlockAnimation()
    {
        animator.SetBool("Block", true);
        StartCoroutine(ResetAnimationBool("Block"));
    }

    public void PlayDodgeAnimation()
    {
        animator.SetBool("Dodge", true);
        StartCoroutine(ResetAnimationBool("Dodge"));
    }

    private IEnumerator ResetAnimationBool(string parameter)
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        animator.SetBool(parameter, false);
    }
}
