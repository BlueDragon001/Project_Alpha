using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AIMemorySystem
{
    private float memoryDuration;
    private int maxMemoryCount;
    private List<AIMemory> memories;

    public struct AIMemory
    {
        public Vector3 position;
        public float timestamp;
        public AIMemoryType type;
        public float importance;
    }

    public enum AIMemoryType
    {
        PlayerSpotted,
        DamageTaken,
        PlayerAttack,
        PlayerDodge,
        CombatStart,
        CombatEnd,
        PlayerBlock,
        PlayerRoll,
        PlayerHeal,
        PlayerItemUse,
        PlayerComboStart,
        PlayerComboEnd,
        PlayerStaggered
    }

    public AIMemorySystem(float memoryDuration = 30f, int maxMemoryCount = 10)
    {
        this.memoryDuration = memoryDuration;
        this.maxMemoryCount = maxMemoryCount;
        memories = new List<AIMemory>();
    }

    public void AddMemory(Vector3 position, AIMemoryType type, float importance = 1f)
    {
        CleanupOldMemories(); // Automatically cleanup before adding new memory

        if (memories.Count >= maxMemoryCount)
        {
            // Remove least important memory instead of oldest
            int leastImportantIndex = 0;
            float lowestImportance = float.MaxValue;
            
            for (int i = 0; i < memories.Count; i++)
            {
                float recency = 1f - ((Time.time - memories[i].timestamp) / memoryDuration);
                float weight = memories[i].importance * recency;
                
                if (weight < lowestImportance)
                {
                    lowestImportance = weight;
                    leastImportantIndex = i;
                }
            }
            
            memories.RemoveAt(leastImportantIndex);
        }

        memories.Add(new AIMemory
        {
            position = position,
            timestamp = Time.time,
            type = type,
            importance = importance
        });
    }

    public void CleanupOldMemories()
    {
        float currentTime = Time.time;
        memories.RemoveAll(m => currentTime - m.timestamp > memoryDuration);
    }

    public float CalculateAdaptationScore()
    {
        if (memories.Count == 0) return 0f;

        float currentTime = Time.time;
        return memories.Sum(m => 
        {
            float recency = 1f - ((currentTime - m.timestamp) / memoryDuration);
            return m.importance * recency;
        }) / memories.Count; // Normalize by memory count
    }

    public List<AIMemory> GetMemories(AIMemoryType type, float timeframe)
    {
        float currentTime = Time.time;
        return memories.FindAll(m => 
            (type == AIMemoryType.CombatEnd || m.type == type) && // Allow filtering all combat memories
            currentTime - m.timestamp <= timeframe
        );
    }

    public bool HasRecentMemories(AIMemoryType type, float timeWindow, int minCount = 1)
    {
        float currentTime = Time.time;
        return memories.Count(m => 
            m.type == type && 
            currentTime - m.timestamp <= timeWindow
        ) >= minCount;
    }

    public AIMemoryType GetDominantAction(float timeWindow)
    {
        var recentMemories = memories.Where(m => Time.time - m.timestamp <= timeWindow);
        
        if (!recentMemories.Any()) return AIMemoryType.CombatEnd;

        return recentMemories
            .GroupBy(m => m.type)
            .OrderByDescending(g => g.Sum(m => m.importance))
            .First().Key;
    }

    public Vector3 GetAverageActionPosition(AIMemoryType type, float timeWindow)
    {
        var relevantMemories = GetMemories(type, timeWindow);
        if (relevantMemories.Count == 0) return Vector3.zero;

        return relevantMemories.Aggregate(Vector3.zero, (sum, memory) => sum + memory.position) 
            / relevantMemories.Count;
    }
}
