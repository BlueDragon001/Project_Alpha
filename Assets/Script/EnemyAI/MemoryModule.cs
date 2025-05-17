using System.Collections.Generic;
using UnityEngine;

public class MemoryModule : MonoBehaviour
{
    public struct Memory
    {
        public List<CombatState> memoryStates;
    }
    private GameObject player;

    public Memory memory = new();
    private EnemyAIStateMachine enemyAI = new();

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found in the scene. Make sure the player has the 'Player' tag.");
        }
    }

    void Update()
    {
        if (enemyAI.currentState == EnemyAIStateMachine.EnemyState.CombatMode)
        {
            AddMemory(CombatStateMachine.currentState);
        }
        if (enemyAI.currentState == EnemyAIStateMachine.EnemyState.Die)
        {
            ClearMemory();
        }
    }


    private void AddMemory(CombatState state)
    {
        if (state == CombatState.Attacking || state == CombatState.Blocking || state == CombatState.Dodging)
        {
            memory.memoryStates.Add(state);
        }


    }
    private void ClearMemory()
    {
        memory.memoryStates.Clear();
    }

    public EnemyAIStateMachine.EnemyState GetCurrentAIState()
    {
        return enemyAI.currentState;
    }
}
