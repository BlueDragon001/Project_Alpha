using System;
using UnityEngine;

public class StateMachine<T> where T : Enum
{
    private T currentState;

    public Action<T> OnEneterState;
    public Action<T> OnExitState;

    public StateMachine()
    {
        // Set initial state to the first value of the enum
        currentState = (T)Enum.GetValues(typeof(T)).GetValue(0);
    }

    public void ChangeState(T newState)
    {
        if (currentState.Equals(newState)) return;
        ExitState(currentState);
        EnterState(newState);
    }

    private void EnterState(T newState)
    {
        currentState = newState;
        Debug.Log($"Entering state: {newState}");
        OnEneterState?.Invoke(newState);
    }
    private void ExitState(T oldState)
    {
        // Optionally, you can add logic to handle exiting a state
        OnExitState?.Invoke(oldState);
        Debug.Log($"Exiting state: {oldState}");
    }

    public T GetCurrentState()
    {
        return currentState;
    }
}