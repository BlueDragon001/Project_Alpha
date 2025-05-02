using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BehaviorModule
{
    float PlayerHP;
    float EnemyHP;

    float aggressionLevel;
    float cautionLevel;
    float feintProbability; // 0 to 1, where 1 is fully aggressive

    public EnemyAIStateMachine.CounterState DecideStrategy(CombatState predictedPlayerAction)
    {
        float advantageRatio = GetPlayerAdvantageRatio(PlayerHP, EnemyHP);
        float roll = Random.Range(0f, 1f);

        // Aggression increases if player is weak or enemy is a brute
        bool beAggressive = (aggressionLevel > 0.5f && advantageRatio < 0.5f) || roll < 0.3f;

        // Defensive if enemy is low HP or cautious
        bool beDefensive = (cautionLevel > 0.5f && EnemyHP < PlayerHP) || roll > 0.8f;

        // Interpret based on predicted player move
        if (predictedPlayerAction == CombatState.Heal && beAggressive)
            return EnemyAIStateMachine.CounterState.AggressiveCombo;

        if (predictedPlayerAction == CombatState.Dodging && !beDefensive)
            return EnemyAIStateMachine.CounterState.WideSwing;

        if (predictedPlayerAction == CombatState.Blocking && roll < feintProbability)
            return EnemyAIStateMachine.CounterState.DelayFeint;

        if (beDefensive)
            return EnemyAIStateMachine.CounterState.DefensiveHold;

        // Fallback
        return EnemyAIStateMachine.CounterState.BaitAndPunish;
    }
    public CombatState GetMostFrequentResponse(List<CombatState> history)
    {
        Dictionary<CombatState, int> frequencyMap = new();

        foreach (var action in history)
        {
            if (frequencyMap.ContainsKey(action))
                frequencyMap[action]++;
            else
                frequencyMap[action] = 1;
        }

        return frequencyMap.OrderByDescending(pair => pair.Value).First().Key;
    }
    public EnemyAIStateMachine.CounterState GetCounterStrategy(List<CombatState> actionHistory)
    {
        var playerAction = GetMostFrequentResponse(actionHistory);

        return playerAction switch
        {
            CombatState.Attacking => EnemyAIStateMachine.CounterState.DefensiveHold,
            CombatState.Blocking => EnemyAIStateMachine.CounterState.WideSwing,
            CombatState.Dodging => EnemyAIStateMachine.CounterState.AggressiveCombo,
            _ => EnemyAIStateMachine.CounterState.DelayFeint
        };
    }

    private float GetPlayerAdvantageRatio(float playerHP, float enemyHP)
    {
        return Mathf.Clamp01((playerHP - enemyHP + 100f) / 200f);
    }

}
