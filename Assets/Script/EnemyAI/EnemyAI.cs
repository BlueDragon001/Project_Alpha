using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum AIState { Idle, Engage, Attack, Defend, Retreat, Adaptive }
    private AIState currentState;

    private AIBehaviorMemory memory;
   // private EnemyStats stats;

    private Transform player;
    private float engageRange = 10f;
    private float attackRange = 2.5f;

    void Start()
    {
        memory = new AIBehaviorMemory();
     //   stats = GetComponent<EnemyStats>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentState = AIState.Idle;
    }

    void Update()
    {
        ProcessAIState();
        memory.UpdateMemory();
    }

    void ProcessAIState()
    {
        switch (currentState)
        {
            case AIState.Idle: HandleIdle(); break;
            case AIState.Engage: HandleEngage(); break;
            case AIState.Attack: HandleAttack(); break;
            case AIState.Defend: HandleDefend(); break;
            case AIState.Retreat: HandleRetreat(); break;
            case AIState.Adaptive: HandleAdaptive(); break;
        }
    }

    void HandleIdle()
    {
        if (Vector3.Distance(transform.position, player.position) < engageRange)
        {
            currentState = AIState.Engage;
        }
    }

    void HandleEngage()
    {
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            currentState = AIState.Attack;
        }
        else
        {
            // Move towards player
           // transform.position = Vector3.MoveTowards(transform.position, player.position, Time.deltaTime * stats.moveSpeed);
        }
    }

    void HandleAttack()
    {
        if (memory.ShouldPunishDodge()) PerformDelayedSwing();
        else if (memory.ShouldPunishParry()) PerformFeint();
        else PerformStandardAttack();
    }

    void HandleDefend()
    {
        if (memory.ShouldPunishBlock()) PerformGuardBreak();
        else if (memory.ShouldPunishDodge()) PerformChaseAttack();
        else PerformStandardBlock();
    }

    void HandleRetreat()
    {
        // if (stats.health < stats.maxHealth * 0.3f)
        // {
        //     // Retreat logic (backpedal, reposition, or heal)
        //     currentState = AIState.Engage;
        // }
    }

    void HandleAdaptive()
    {
        if (memory.ShouldPunishDodge()) PerformDelayedSwing();
        if (memory.ShouldPunishParry()) PerformFeint();
        if (memory.ShouldPunishBlock()) PerformGuardBreak();
    }

    void PerformDelayedSwing()
    {
        // Add a slight delay before attacking to punish dodge spammers
        StartCoroutine(DelayedAttackCoroutine());
    }

    IEnumerator DelayedAttackCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        PerformStandardAttack();
    }

    void PerformFeint()
    {
        // Fake an attack and then immediately follow up
        Debug.Log("Feinting attack to bait parry");
        StartCoroutine(FeintCoroutine());
    }

    IEnumerator FeintCoroutine()
    {
        yield return new WaitForSeconds(0.3f);
        PerformStandardAttack();
    }

    void PerformGuardBreak()
    {
        Debug.Log("Using Guard Break to counter excessive blocking");
        // Logic for breaking through guard
    }

    void PerformStandardAttack()
    {
        Debug.Log("Performing standard attack");
        // Logic for attacking
    }

    void PerformChaseAttack()
    {
        Debug.Log("Chasing down dodging player");
        // Logic for chasing down dodging player
    }
    void PerformStandardBlock()
    {
        Debug.Log("Performing standard block");
        // Logic for blocking
    }

}
