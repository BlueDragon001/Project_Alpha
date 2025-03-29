using UnityEngine;

public class EnemyAI : BaseAdaptiveEnemyAI
{
    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float circlingDistance = 3f;
    [SerializeField] private float attackCooldown = 2f;
    
    private enum CombatStyle
    {
        Passive,
        Aggressive,
        Defensive
    }

    private CombatStyle combatStyle = CombatStyle.Passive;
    private float lastAttackTime;
    private Vector3 circlingDirection;

    protected override void Start()
    {
        base.Start();
        circlingDirection = Random.value > 0.5f ? Vector3.right : Vector3.left;
    }

    private void Update()
    {
        if (currentState != CombatState.Combat || currentTarget == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        UpdateCombatBehavior(distanceToTarget);
    }

    private void UpdateCombatBehavior(float distanceToTarget)
    {
        if (currentState != CombatState.Combat) return;

        switch (combatStyle)
        {
            case CombatStyle.Passive:
                CircleTarget();
                break;
            case CombatStyle.Aggressive:
                if (distanceToTarget <= attackRange && Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
                else
                {
                    MoveTowardsTarget();
                }
                break;
            case CombatStyle.Defensive:
                if (distanceToTarget < circlingDistance)
                {
                    Dodge();
                }
                CircleTarget();
                break;
        }
    }

    protected override void AdaptBehavior()
    {
        if (currentState != CombatState.Combat) return;

        var recentDodges = memorySystem.GetMemories(AIMemorySystem.AIMemoryType.PlayerDodge, 5f);
        var recentAttacks = memorySystem.GetMemories(AIMemorySystem.AIMemoryType.PlayerAttack, 5f);

        if (recentDodges.Count > recentAttacks.Count)
        {
            combatStyle = CombatStyle.Aggressive;
        }
        else if (adaptationScore > 5f)
        {
            combatStyle = CombatStyle.Defensive;
        }
        else
        {
            combatStyle = CombatStyle.Passive;
        }
    }

    protected override void OnTargetDetected(Vector3 position)
    {
        memorySystem.AddMemory(position, AIMemorySystem.AIMemoryType.PlayerSpotted);
        TransitionToState(CombatState.Combat);
    }

    protected override void OnTakeDamage(float damage, Vector3 damageSource)
    {
        memorySystem.AddMemory(damageSource, AIMemorySystem.AIMemoryType.DamageTaken, damage);
        combatStyle = CombatStyle.Defensive;
    }

    protected override void OnStaggered()
    {
        // Animation and stagger logic
    }

    protected override void OnDeath()
    {
        // Death animation and cleanup
        enabled = false;
    }

    protected override void OnStateChanged(CombatState oldState, CombatState newState)
    {
        // Handle state transitions, animations, etc.
    }

    private void Attack()
    {
        // Attack implementation
        lastAttackTime = Time.time;
    }

    private void Dodge()
    {
        // Dodge implementation
    }

    private void CircleTarget()
    {
        if (currentTarget == null) return;
        // Circling movement implementation
    }

    private void MoveTowardsTarget()
    {
        if (currentTarget == null) return;
        // Movement implementation
    }
}
