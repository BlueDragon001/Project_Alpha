# Enemy AI System Documentation

## Overview
The Enemy AI system consists of two main components:
- `EnemyAI`: Abstract base class for all enemy behaviors
- `CombatMemory`: Pattern recognition and learning system

## EnemyAI Base Class

### Core Features
- State-based behavior system (Idle, Think, Chase, Attack, Dodge)
- Stamina management
- Adaptive combat positioning
- Pattern-based decision making
- Natural thinking delays and strategy cooldowns

### Key Parameters
```csharp
[Header("Timing Parameters")]
protected float thinkingDuration = 1f;    // Time spent "thinking" before action
protected float strategyCooldown = 2f;     // Cooldown between strategy changes
protected float decisionDelay = 0.2f;      // Delay before executing decisions
```

### Inheritance Guide
To create a specialized enemy type:

1. Create a new class inheriting from EnemyAI
```csharp
public class BossEnemy : EnemyAI
{
    protected override void UpdateCombatStrategy()
    {
        // Custom boss logic
        base.UpdateCombatStrategy();
    }
}
```

2. Override key virtual methods:
- `UpdateCombatStrategy()`: Custom decision making
- `ExecuteCurrentState()`: Unique attack patterns
- `OnStrategyChange()`: Reaction to strategy changes
- `OnThinkingComplete()`: Post-thinking actions

## Combat Memory System

### Features
- Tracks player action sequences
- Recognizes recurring patterns
- Calculates success rates for strategies
- Predicts likely player actions

### Pattern Recognition
```csharp
public class PlayerAction
{
    public string ActionType;      // Type of action performed
    public float PlayerStamina;    // Player's stamina at time of action
    public float DistanceToEnemy;  // Distance to enemy when performed
    public float TimeStamp;        // When the action occurred
}
```

### Usage Example
```csharp
// Record player actions
void OnPlayerAttack()
{
    OnPlayerAction("Attack");
}

// React to patterns
if (combatMemory.PredictNextAction() == "Heal")
{
    PrepareAggressive();
}
```

## Best Practices

1. **Thinking Time**: Always use thinking time between major strategy changes to make AI feel more natural and give players reaction time.

2. **Pattern Recognition**: Keep MAX_MEMORY_SIZE balanced:
   - Too high: Complex but potentially slow pattern matching
   - Too low: Quick but might miss important patterns

3. **Combat Distance**: Use preferredCombatDistance for enemy-specific positioning:
```csharp
[SerializeField] private float preferredCombatDistance = 2.5f;
```

4. **Stamina Management**: Implement stamina costs for all major actions to prevent spam:
```csharp
[SerializeField] private float attackStaminaCost = 20f;
[SerializeField] private float dodgeStaminaCost = 25f;
```

## Extension Points

1. **New States**
Add new states to EnemyState enum:
```csharp
protected enum EnemyState 
{ 
    Idle, Think, Chase, Attack, Dodge, 
    // Add new states:
    Flank, Retreat, SpecialAttack 
}
```

2. **Custom Patterns**
Extend CombatPattern for complex behaviors:
```csharp
public class AdvancedCombatPattern : CombatPattern
{
    public float OptimalDistance { get; set; }
    public float SuccessRate { get; set; }
}
```

## Troubleshooting

1. **AI Too Reactive**: Increase thinkingDuration and strategyCooldown
2. **Pattern Recognition Issues**: Adjust PATTERN_RECOGNITION_THRESHOLD
3. **Stamina Problems**: Balance staminaRegenRate with action costs

## Performance Considerations

- Keep MAX_MEMORY_SIZE reasonable (default: 10)
- Clear successfulAttackPositions periodically
- Use object pooling for frequent actions
