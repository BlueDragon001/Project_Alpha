using UnityEditorInternal;
using UnityEngine;

public static class StateMachineHandler
{
    public enum EnemyType
    {
        Melee,
        Ranged
    }

    public static  StateMachine<EnemyType> enemyType = new StateMachine<EnemyType>();

    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead
    }

    public static StateMachine<EnemyState> enemyState = new();



    public enum CombatState
    {
        Idle,
        Attacking,
        Cooldown,
        Blocking,
        Dodging,
        Stunned,
    }

    public static StateMachine<CombatState> combatState = new();
}
