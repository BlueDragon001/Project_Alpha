using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAI : MonoBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float stamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Combat Behavior")]
    [SerializeField] private float preferredCombatDistance = 2.5f;
    [SerializeField] private float dodgeStaminaCost = 25f;
    [SerializeField] private float attackStaminaCost = 20f;

    [Header("Combat Intelligence")]
    [SerializeField] private float baitDistance = 3.5f;
    [SerializeField] private float counterWindowTime = 0.5f;

    [Header("Timing Parameters")]
    [SerializeField] protected float thinkingDuration = 1f;
    [SerializeField] protected float strategyCooldown = 2f;
    [SerializeField] protected float decisionDelay = 0.2f;

    protected enum EnemyState { Idle, Think, Chase, Attack, Dodge }
    private EnemyState currentState;
    private Transform player;
    private Vector3 lastKnownPlayerPosition;
    private Dictionary<string, float> behaviorWeights;
    private List<Vector3> successfulAttackPositions;
    private CombatMemory combatMemory;
    private string predictedPlayerAction;
    private bool isBaiting;
    private float counterTimer;

    protected float thinkingTimer;
    protected float strategyTimer;
    protected bool isThinking;
    protected bool canChangeStrategy;

    protected virtual void Start()
    {
        InitializeBehavior();
        combatMemory = new CombatMemory();
        thinkingTimer = 0f;
        strategyTimer = 0f;
        canChangeStrategy = true;
    }

    private void InitializeBehavior()
    {
        behaviorWeights = new Dictionary<string, float>
        {
            {"aggressive", 0.5f},
            {"defensive", 0.5f},
            {"spacing", 2.5f}
        };
        successfulAttackPositions = new List<Vector3>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected virtual void Update()
    {
        UpdateTimers();
        UpdateStamina();
        
        if (!isThinking)
        {
            UpdateState();
            ExecuteCurrentState();
            if (canChangeStrategy)
                UpdateCombatStrategy();
        }
    }

    protected virtual void UpdateTimers()
    {
        if (isThinking)
        {
            thinkingTimer -= Time.deltaTime;
            if (thinkingTimer <= 0)
            {
                isThinking = false;
                StartStrategyTimer();
            }
        }

        if (!canChangeStrategy)
        {
            strategyTimer -= Time.deltaTime;
            if (strategyTimer <= 0)
                canChangeStrategy = true;
        }
    }

    protected virtual void StartThinking()
    {
        isThinking = true;
        thinkingTimer = thinkingDuration;
        currentState = EnemyState.Think;
    }

    protected virtual void StartStrategyTimer()
    {
        canChangeStrategy = false;
        strategyTimer = strategyCooldown;
    }

    protected virtual void UpdateStamina()
    {
        stamina = Mathf.Min(100f, stamina + staminaRegenRate * Time.deltaTime);
    }

    protected virtual void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer > attackRange * 1.5f)
        {
            currentState = EnemyState.Chase;
        }
        else if (ShouldDodge())
        {
            currentState = EnemyState.Dodge;
        }
        else if (CanAttack())
        {
            currentState = EnemyState.Attack;
        }
    }

    private bool ShouldDodge()
    {
        return stamina >= dodgeStaminaCost && 
               behaviorWeights["defensive"] > Random.value && 
               IsPlayerAttacking();
    }

    private bool CanAttack()
    {
        return stamina >= attackStaminaCost && 
               behaviorWeights["aggressive"] > Random.value;
    }

    public void OnSuccessfulHit()
    {
        successfulAttackPositions.Add(transform.position - player.position);
        behaviorWeights["aggressive"] += 0.1f;
        behaviorWeights["spacing"] = Vector3.Distance(transform.position, player.position);
    }

    public void OnTakeDamage()
    {
        behaviorWeights["defensive"] += 0.1f;
        behaviorWeights["aggressive"] -= 0.05f;
    }

    private bool IsPlayerAttacking()
    {
        // Implement player attack detection
        return false;
    }

    protected virtual void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Chase:
                ChasePlayer();
                break;
            case EnemyState.Attack:
                PerformAttack();
                break;
            case EnemyState.Dodge:
                PerformDodge();
                break;
        }
    }

    private void ChasePlayer()
    {
        float targetDistance = behaviorWeights["spacing"];
        Vector3 targetPosition = player.position - (player.position - transform.position).normalized * targetDistance;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    private void PerformAttack()
    {
        if (stamina >= attackStaminaCost)
        {
            stamina -= attackStaminaCost;
            // Implement attack logic
        }
    }

    private void PerformDodge()
    {
        if (stamina >= dodgeStaminaCost)
        {
            stamina -= dodgeStaminaCost;
            // Implement dodge logic
        }
    }

    protected virtual void UpdateCombatStrategy()
    {
        predictedPlayerAction = combatMemory.PredictNextAction();
        if (predictedPlayerAction != null && !isBaiting)
        {
            StartThinking();
            StartCoroutine(DelayedSetupBait());
        }
    }

    protected virtual IEnumerator DelayedSetupBait()
    {
        yield return new WaitForSeconds(decisionDelay);
        if (!isThinking)
            SetupBait();
    }

    protected virtual void SetupBait()
    {
        switch (predictedPlayerAction)
        {
            case "Attack":
                PrepareCounter();
                break;
            case "Heal":
               // PrepareAggressive();
                break;
            case "Roll":
            //    PrepareRollCatch();
                break;
        }
    }

    public void OnPlayerAction(string actionType)
    {
        var playerAction = new PlayerAction
        {
            ActionType = actionType,
            PlayerStamina = GetPlayerStamina(),
            DistanceToEnemy = Vector3.Distance(transform.position, player.position),
            TimeStamp = Time.time
        };
        
        combatMemory.RecordAction(playerAction);
        
        if (isBaiting && actionType == predictedPlayerAction)
        {
            ExecuteCounter();
        }
    }

    private void PrepareCounter()
    {
        isBaiting = true;
        counterTimer = counterWindowTime;
        // Position at optimal counter distance
        Vector3 targetPos = player.position + (transform.position - player.position).normalized * baitDistance;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    private void ExecuteCounter()
    {
        // Execute appropriate counter attack based on predicted action
        if (stamina >= attackStaminaCost * 1.5f)
        {
            stamina -= attackStaminaCost * 1.5f;
            // Implement counter attack with bonus damage
        }
    }

    private float GetPlayerStamina()
    {
        // Implement player stamina detection
        return 100f;
    }

    // Add new protected methods for specialized AI
    protected virtual void OnStrategyChange()
    {
        // Override in derived classes
    }

    protected virtual void OnThinkingComplete()
    {
        // Override in derived classes
    }

    protected virtual bool ShouldChangeStrategy()
    {
        // Override in derived classes
        return true;
    }
}
