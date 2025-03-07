using UnityEngine;

public enum CombatState { Idle, Attacking, Jumping, Moving, Blocking, Dodging, Hiting, Dieing }
public class CombatStateMachine
{
    public CombatState currentState { get; private set; }
    public void ChangeState(CombatState newState)
    {
        if (currentState == newState) return;
        ExitState(currentState);
        EnterState(newState);

    }

    void EnterState(CombatState newState)
    {
        currentState = newState;
    }

    void ExitState(CombatState oldState)
    {
        
    }
}
