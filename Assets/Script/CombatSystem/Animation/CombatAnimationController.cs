using UnityEngine;

public class CombatAnimationController : MonoBehaviour
{
    public Animator animator;
    public static CombatStateMachine combatStateMachine = new();
    float animationSmoothness = 0.5f;
    public AnimationClips animationClips;


    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void AttackAnimation()
    {
        Play(InputType.Attack);
        combatStateMachine.ChangeState(CombatState.Attacking);
    }

    public void JumpAnimation()
    {
        Play(InputType.Jump);
        combatStateMachine.ChangeState(CombatState.Jumping);
    }

    public void MoveAnimation()
    {
        Play(InputType.MoveBackward);
        combatStateMachine.ChangeState(CombatState.Moving);
    }
    public void BlockAnimation()
    {
        Play(InputType.Block);
        combatStateMachine.ChangeState(CombatState.Blocking);
    }

    public void IdleAnimation()
    {
        Play(InputType.Idle);
        combatStateMachine.ChangeState(CombatState.Idle);
    }
    void Play(InputType inputClip)
    {
        if(animator == null) return;
        string AnimationName = GetAnimationClip(inputClip).name;
        animator.CrossFade(AnimationName, animationSmoothness);

    }

    private AnimationClip GetAnimationClip(InputType inputClip)
    {
        return inputClip switch
        {
            InputType.Attack => animationClips.attack1,
            InputType.Jump => animationClips.jump,
            InputType.MoveForward => animationClips.MoveForward,
            InputType.MoveBackward => animationClips.MoveBackward,
            InputType.MoveLeft => animationClips.MoveLeft,
            InputType.MoveRight => animationClips.MoveRight,
            InputType.Block => animationClips.block,
            _ => null,
        };
    }
}