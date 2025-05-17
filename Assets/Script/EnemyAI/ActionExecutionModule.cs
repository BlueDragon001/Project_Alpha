using UnityEngine;

[RequireComponent(typeof(MemoryModule))]
[RequireComponent(typeof(AIMovementModule))]
[RequireComponent(typeof(PerceptionModule))]
public class ActionExecutionModule : MonoBehaviour
{
    private MemoryModule memoryModule;
    private AIMovementModule movementModule;
    private PerceptionModule perceptionModule;
    private BehaviorModule behaviorModule;
    private EnemyAIStateMachine stateMachine = new();
    private float lastStateChangeTime;
    private float stateUpdateInterval = 0.5f;
    GameObject player;

    void Awake()
    {
        memoryModule = GetComponent<MemoryModule>();
        movementModule = GetComponent<AIMovementModule>();
        perceptionModule = GetComponent<PerceptionModule>();
    }

    void Start()
    {
        behaviorModule = new BehaviorModule();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (Time.time - lastStateChangeTime < stateUpdateInterval) return;

        if (perceptionModule.IsPlayerInLineOfSight())
        {
            if (perceptionModule.IsPlayerInRange())
            {
                stateMachine.ChangeState(EnemyAIStateMachine.EnemyState.CombatMode);
            }
            else
            {
                stateMachine.ChangeState(EnemyAIStateMachine.EnemyState.Chase);
            }
        }
        else if (memoryModule.GetCurrentAIState() == EnemyAIStateMachine.EnemyState.CombatMode)
        {
            stateMachine.ChangeState(EnemyAIStateMachine.EnemyState.Patrol);
        }

        ExecuteAction(stateMachine.currentState);
        lastStateChangeTime = Time.time;
    }

    void ExecuteAction(EnemyAIStateMachine.EnemyState currentState)
    {

        Debug.Log($"Current State: {currentState}");


        switch (currentState)
        {
            case EnemyAIStateMachine.EnemyState.Idle:
                break;

            case EnemyAIStateMachine.EnemyState.Patrol:
                if (perceptionModule.ScanForPlayer())
                {
                    stateMachine.ChangeState(EnemyAIStateMachine.EnemyState.Chase);
                }
                break;

            case EnemyAIStateMachine.EnemyState.Chase:
                movementModule.MoveToTarget(player.transform);
                break;

            case EnemyAIStateMachine.EnemyState.CombatMode:
                HandleCombat();
                break;

            
        }
    }

    private void HandleCombat()
    {
        Debug.Log(memoryModule.memory.memoryStates);
        if (memoryModule.memory.memoryStates == null || memoryModule.memory.memoryStates.Count == 0)
        {
            stateMachine.ChangeState(EnemyAIStateMachine.EnemyState.Idle);
            return;
        }

        var counterStrategy = behaviorModule.GetCounterStrategy(memoryModule.memory.memoryStates);

        switch (counterStrategy)
        {
            case EnemyAIStateMachine.CounterState.AggressiveCombo:
                movementModule.MoveToTarget(player.transform);
                break;

            case EnemyAIStateMachine.CounterState.DefensiveHold:
                // Stay in place, ready to counter
                break;

            case EnemyAIStateMachine.CounterState.BaitAndPunish:
                Vector3 circlePosition = GetCirclingPosition();
                movementModule.MoveToTarget(CreateTempTarget(circlePosition));
                break;
        }
    }

    private Vector3 GetCirclingPosition()
    {

        float angle = Time.time * 90f;
        Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.right * 2f;
        return player.transform.position + offset;
    }

    private Transform CreateTempTarget(Vector3 position)
    {
        GameObject temp = new("TempTarget");
        temp.transform.position = position;
        Destroy(temp, 0.1f);
        return temp.transform;
    }
}
