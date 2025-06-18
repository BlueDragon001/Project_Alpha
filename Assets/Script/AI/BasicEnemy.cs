using System;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
   

    public float health;
    public float attackDamage;
    public float attackRange;
    public float attackCooldown;
    public float speed;

    //public Memory memory; ***TODO: Implement Memory class for AI behavior***

    void Start()
    {

    }

    void Update()
    {
        ExecuteState();
    }

    private void ExecuteState()
    {
        switch (StateMachineHandler.enemyState.GetCurrentState())
        {
            case StateMachineHandler.EnemyState.Idle:
                HandleIdle();
                break;
            case StateMachineHandler.EnemyState.Patrol:
                HandlePatrol();
                break;
            case StateMachineHandler.EnemyState.Chase:
                HandleChase();
                break;
            case StateMachineHandler.EnemyState.Attack:
                HandleAttack();
                break;
            case StateMachineHandler.EnemyState.Dead:
                HandleDead();
                break;
        }
    }

    public virtual void HandleIdle() { }
    public virtual void HandlePatrol() { }
    public virtual void HandleChase() { }
    public virtual void HandleAttack() { }
    public virtual void HandleDead() { }

}
