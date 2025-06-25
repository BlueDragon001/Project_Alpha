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


    public CombatState PredictNextPlayerAction(int lookback = 5)
    {
        if (memory.memoryStates == null || memory.memoryStates.Count == 0)
        {
            Debug.LogWarning("No memory states to process.");
            return CombatState.Idle; // Or a default state
        }

        // Look at the last 'lookback' actions (or all if fewer)
        int count = Mathf.Min(lookback, memory.memoryStates.Count);
        var recentStates = memory.memoryStates.GetRange(memory.memoryStates.Count - count, count);

        // Count occurrences, ignoring Idle and Moving states
        Dictionary<CombatState, int> freq = new();
        foreach (var state in recentStates)
        {
            if (state == CombatState.Idle || state == CombatState.Moving)
                continue;
            freq.TryAdd(state, 0);
            freq[state]++;
        }

        // Find the most frequent
        CombatState mostLikely = CombatState.Idle;
        int maxCount = 0;
        foreach (var kvp in freq)
        {
            if (kvp.Value > maxCount)
            {
                mostLikely = kvp.Key;
                maxCount = kvp.Value;
            }
        }
        return mostLikely;
    }
}
