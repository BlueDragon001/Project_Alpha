using UnityEngine;

public class EnemyAIStateMachine
{
    public enum EnemyState { Patrol, Idle, Chase, CombatMode, Attack, Flee, Die, None }
    public enum CounterState
    {
        AggressiveCombo,
        GuardBreak,
        DelayFeint,
        ThrustAttack,
        WideSwing,
        BaitAndPunish,
        DefensiveHold
    }

    public EnemyState currentState { get; private set; }

    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        ExitState(currentState);
        EnterState(newState);
    }

    void EnterState(EnemyState newState)
    {
        currentState = newState;
        // Add any additional logic for entering a state here
    }

    void ExitState(EnemyState oldState)
    {
        // Add any additional logic for exiting a state here
    }
}
