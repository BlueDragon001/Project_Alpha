using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PhysicsBasedPlayerController))]
[RequireComponent(typeof(AnimationHandler))]
public class ActionHandler : MonoBehaviour
{
    private PhysicsBasedPlayerController physicsBasedPlayerController;
    private AnimationHandler animationHandler;
    void Start()
    {
        physicsBasedPlayerController = GetComponent<PhysicsBasedPlayerController>();
        animationHandler = GetComponent<AnimationHandler>();
    }

    public void Idle()
    {
        animationHandler.Idle();
        // CombatStateMachine.ChangeState(CombatState.Idle);

    }
    public float Attack()
    {
        float holdTime = animationHandler.AttackAnimation();
        CombatStateMachine.ChangeState(CombatState.Attacking);
        return holdTime;
    }

    public float Jump()
    {
        float holdTime = animationHandler.JumpAnimation();
        CombatStateMachine.ChangeState(CombatState.Jumping);
        physicsBasedPlayerController.HandleJumpInput();
        return holdTime;
      
    }



    public void Move(Vector2 Input)
    {
        animationHandler.MoveAnimation(Input);
        physicsBasedPlayerController.HandleMovementInput(Input);
        CombatStateMachine.ChangeState(CombatState.Moving);
    }

    public void Block()
    {

    }

    IEnumerator ChangeStateAfterTime(CombatState newState, float time)
    {
        yield return new WaitForSeconds(Mathf.Abs(time));
        CombatStateMachine.ChangeState(newState);
    }
}
