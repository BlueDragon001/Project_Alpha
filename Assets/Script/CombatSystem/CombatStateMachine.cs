using UnityEngine;

/// <summary>
/// Represents the possible states in the combat system.
/// </summary>
public enum CombatState { Idle, Attacking, Jumping, Moving, Blocking, Dodging, Hiting, Dieing, None }

/// <summary>
/// Static class that manages the combat state transitions.
/// Implements a simple state machine pattern for combat mechanics.
/// </summary>
public static class CombatStateMachine
{
    public static CombatState currentState { get; private set; }
    
    public static void ChangeState(CombatState newState)
    {
        if (currentState == newState) return;
        ExitState(currentState);
        EnterState(newState);

    }

    static void EnterState(CombatState newState)
    {
        currentState = newState;
    }

    static void ExitState(CombatState oldState)
    {

    }
}
