using UnityEngine;

public abstract class BaseAdaptiveEnemyAI : MonoBehaviour
{
    [System.Serializable]
    protected struct CombatStats
    {
        public float health;
        public float stamina;
        public float attackPower;
        public float defense;
        public float staggerResistance;
        public float movementSpeed;
    }

    protected enum CombatState
    {
        Idle,
        Aware,
        Combat,
        Staggered,
        Recovering,
        Dead
    }

    [Header("Base Stats")]
    [SerializeField] protected CombatStats stats;
    [SerializeField] protected float memoryDuration = 30f;
    [SerializeField] protected int maxMemoryCount = 10;
    [SerializeField] protected float awarenessRange = 15f;
    [SerializeField] protected float combatRange = 10f;

    protected AIMemorySystem memorySystem;
    protected float adaptationScore;
    protected CombatState currentState;
    protected Transform currentTarget;
    protected Vector3 lastKnownTargetPosition;
    protected float stateTimer;

    protected virtual void Start()
    {
        memorySystem = new AIMemorySystem(memoryDuration, maxMemoryCount);
        InvokeRepeating(nameof(UpdateAI), 0.1f, 0.1f);
        currentState = CombatState.Idle;
    }

    private void UpdateAI()
    {
        memorySystem.CleanupOldMemories();
        adaptationScore = memorySystem.CalculateAdaptationScore();
        UpdateAwareness();
        UpdateCombatState();
        AdaptBehavior();
    }

    protected virtual void UpdateAwareness()
    {
        if (currentState == CombatState.Dead) return;

        var potentialTargets = Physics.OverlapSphere(transform.position, awarenessRange, LayerMask.GetMask("Player"));
        if (potentialTargets.Length > 0 && currentTarget == null)
        {
            currentTarget = potentialTargets[0].transform;
            OnTargetDetected(currentTarget.position);
        }
        else if (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget > awarenessRange * 1.5f)
            {
                LoseTarget();
            }
        }
    }

    protected virtual void UpdateCombatState()
    {
        stateTimer -= Time.deltaTime;
        
        if (stateTimer <= 0)
        {
            switch (currentState)
            {
                case CombatState.Staggered:
                    TransitionToState(CombatState.Recovering);
                    break;
                case CombatState.Recovering:
                    TransitionToState(currentTarget ? CombatState.Combat : CombatState.Aware);
                    break;
            }
        }
    }

    protected virtual void TransitionToState(CombatState newState)
    {
        CombatState oldState = currentState;
        currentState = newState;

        switch (newState)
        {
            case CombatState.Staggered:
                stateTimer = 2f;
                OnStaggered();
                break;
            case CombatState.Recovering:
                stateTimer = 1.5f;
                break;
        }

        OnStateChanged(oldState, newState);
    }

    protected virtual void LoseTarget()
    {
        lastKnownTargetPosition = currentTarget.position;
        currentTarget = null;
        TransitionToState(CombatState.Aware);
    }

    public virtual void TakeDamage(float damage, Vector3 source)
    {
        stats.health -= Mathf.Max(0, damage - stats.defense);
        if (stats.health <= 0)
        {
            TransitionToState(CombatState.Dead);
            OnDeath();
        }
        else
        {
            OnTakeDamage(damage, source);
            if (damage > stats.staggerResistance)
            {
                TransitionToState(CombatState.Staggered);
            }
        }
    }

    // Abstract methods
    protected abstract void AdaptBehavior();
    protected abstract void OnTargetDetected(Vector3 position);
    protected abstract void OnTakeDamage(float damage, Vector3 damageSource);
    protected abstract void OnStaggered();
    protected abstract void OnDeath();
    protected abstract void OnStateChanged(CombatState oldState, CombatState newState);
}
