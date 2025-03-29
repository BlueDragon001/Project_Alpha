using UnityEngine;
using System.Collections;

public class MeleeEnemyAI : BaseAdaptiveEnemyAI
{
    [Header("Melee Settings")]
    [SerializeField] private float meleeRange = 2.5f;
    [SerializeField] private float preferredCombatDistance = 2f;
   // [SerializeField] private float repositionThreshold = 3f;
    
    [Header("Attack Configuration")]
    [SerializeField] private float lightAttackCooldown = 1f;
    [SerializeField] private float heavyAttackCooldown = 2.5f;
    [SerializeField] private float comboWindowDuration = 2f;
    [SerializeField] private int maxComboCount = 3;

    private enum AttackType { Light, Heavy, Charged }
    private enum CombatPosition { TooClose, Optimal, TooFar }
    
    private float lastAttackTime;
    private int currentComboCount;
    private bool isCharging;
    private Vector3 combatMoveTarget;

    protected override void Start()
    {
        base.Start();
        stats.attackPower = 10f;
        stats.staggerResistance = 15f;
    }

    private void Update()
    {
        if (currentState != CombatState.Combat || currentTarget == null) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        UpdateMeleeCombat(distanceToTarget);
    }

    private void UpdateMeleeCombat(float distanceToTarget)
    {
        var position = EvaluateCombatPosition(distanceToTarget);
        
        switch (position)
        {
            case CombatPosition.TooClose:
                CreateDistance();
                break;
            case CombatPosition.Optimal:
                ExecuteCombatBehavior();
                break;
            case CombatPosition.TooFar:
                ApproachTarget();
                break;
        }
    }

    private void ExecuteCombatBehavior()
    {
        if (Time.time < lastAttackTime + lightAttackCooldown) return;

        if (currentComboCount >= maxComboCount)
        {
            ResetCombo();
            return;
        }

        // Use dominant action detection
        var dominantAction = memorySystem.GetDominantAction(2f);
        var playerPosition = memorySystem.GetAverageActionPosition(AIMemorySystem.AIMemoryType.PlayerAttack, 2f);
        
        switch (dominantAction)
        {
            case AIMemorySystem.AIMemoryType.PlayerBlock:
                StartCoroutine(ChargedAttack());
                break;
            case AIMemorySystem.AIMemoryType.PlayerDodge:
                if (memorySystem.HasRecentMemories(AIMemorySystem.AIMemoryType.PlayerDodge, 2f, 2))
                {
                    // Player is dodging frequently, use delayed attacks
                    StartCoroutine(DelayedCombo());
                }
                break;
            case AIMemorySystem.AIMemoryType.PlayerAttack:
                if (Vector3.Distance(transform.position, playerPosition) > meleeRange)
                {
                    // Player is attacking from distance, close in
                    combatMoveTarget = playerPosition;
                    ExecuteCombo();
                }
                else
                {
                    // Counter-attack
                    PerformAttack(AttackType.Heavy);
                }
                break;
            default:
                if (adaptationScore > 3f)
                {
                    ExecuteCombo();
                }
                else
                {
                    PerformAttack(AttackType.Light);
                }
                break;
        }
    }

    private IEnumerator ChargedAttack()
    {
        isCharging = true;
        yield return new WaitForSeconds(1f);
        if (isCharging) // Check if we weren't interrupted
        {
            PerformAttack(AttackType.Charged);
            isCharging = false;
        }
    }

    private IEnumerator DelayedCombo()
    {
        yield return new WaitForSeconds(0.5f);
        if (currentState == CombatState.Combat)
        {
            ExecuteCombo();
        }
    }

    private void ExecuteCombo()
    {
        if (Time.time > lastAttackTime + comboWindowDuration)
        {
            currentComboCount = 0;
        }

        AttackType attackType = (currentComboCount == maxComboCount - 1) ? 
            AttackType.Heavy : AttackType.Light;

        PerformAttack(attackType);
        currentComboCount++;
    }

    private void PerformAttack(AttackType type)
    {
        float damage = stats.attackPower;
        switch (type)
        {
            case AttackType.Heavy:
                damage *= 1.5f;
                lastAttackTime = Time.time + heavyAttackCooldown;
                break;
            case AttackType.Charged:
                damage *= 2f;
                lastAttackTime = Time.time + heavyAttackCooldown * 1.5f;
                break;
            default:
                lastAttackTime = Time.time + lightAttackCooldown;
                break;
        }
        
        // Implement actual attack logic here
    }

    private CombatPosition EvaluateCombatPosition(float distance)
    {
        if (distance < preferredCombatDistance - 0.5f)
            return CombatPosition.TooClose;
        if (distance > preferredCombatDistance + 0.5f)
            return CombatPosition.TooFar;
        return CombatPosition.Optimal;
    }

    protected override void AdaptBehavior()
    {
        // Use memory system to adapt combat distance and style
        var recentMemories = memorySystem.GetMemories(AIMemorySystem.AIMemoryType.PlayerAttack, 5f);
        var averageAttackPos = memorySystem.GetAverageActionPosition(AIMemorySystem.AIMemoryType.PlayerAttack, 5f);
        
        float targetDistance = recentMemories.Count > 2 ? 3.5f : 2f;
        preferredCombatDistance = Mathf.Lerp(preferredCombatDistance, targetDistance, Time.deltaTime);

        // Adjust position based on player's attack patterns
        if (averageAttackPos != Vector3.zero)
        {
            combatMoveTarget = averageAttackPos + (transform.position - averageAttackPos).normalized * preferredCombatDistance;
        }
    }

    private void ResetCombo()
    {
        currentComboCount = 0;
        lastAttackTime = Time.time + heavyAttackCooldown;
    }

    protected override void OnTakeDamage(float damage, Vector3 damageSource)
    {
        
        
        if (isCharging)
        {
            isCharging = false;
            CreateDistance();
        }

        // Add defensive behavior when taking significant damage
        if (damage > stats.staggerResistance * 0.5f)
        {
            var retreatDirection = (transform.position - damageSource).normalized;
            combatMoveTarget = transform.position + retreatDirection * preferredCombatDistance;
            CreateDistance();
        }
    }

    // Required abstract method implementations with basic behavior
    protected override void OnStaggered() => ResetCombo();
    protected override void OnDeath() => enabled = false;
    protected override void OnTargetDetected(Vector3 position) => TransitionToState(CombatState.Combat);
    protected override void OnStateChanged(CombatState oldState, CombatState newState) { }

    private void CreateDistance()
    {
        // Implement backwards movement/dodge
    }

    private void ApproachTarget()
    {
        // Implement approach movement
    }
}
