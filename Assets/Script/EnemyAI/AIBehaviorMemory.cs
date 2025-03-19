using UnityEngine;
public class AIBehaviorMemory
{
    private int dodgeCount, parryCount, blockCount, attackCount;
    private float lastDodgeTime, lastParryTime;

    public void RegisterDodge()
    {
        dodgeCount++;
        lastDodgeTime = Time.time;
    }

    public void RegisterParry()
    {
        parryCount++;
        lastParryTime = Time.time;
    }

    public void RegisterBlock()
    {
        blockCount++;
    }

    public void RegisterAttack()
    {
        attackCount++;
    }

    public void UpdateMemory()
    {
        // Reduce influence of old data over time
        dodgeCount = Mathf.Max(0, dodgeCount - 1);
        parryCount = Mathf.Max(0, parryCount - 1);
        blockCount = Mathf.Max(0, blockCount - 1);
        attackCount = Mathf.Max(0, attackCount - 1);
    }

    public bool ShouldPunishDodge() => dodgeCount > 3;  // If player dodges too much
    public bool ShouldPunishParry() => parryCount > 2;  // If player spams parry
    public bool ShouldPunishBlock() => blockCount > 3;  // If player blocks too often
}
