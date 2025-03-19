using System.Collections.Generic;
using UnityEngine;

public class PlayerAction
{
    public string ActionType { get; set; }
    public float PlayerStamina { get; set; }
    public float DistanceToEnemy { get; set; }
    public float TimeStamp { get; set; }
}

public class CombatPattern
{
    public List<PlayerAction> Sequence { get; set; }
    public int OccurrenceCount { get; set; }
    public float SuccessRate { get; set; }
}

public class CombatMemory
{
    private Queue<PlayerAction> recentActions;
    private Dictionary<string, CombatPattern> recognizedPatterns;
    private const int MAX_MEMORY_SIZE = 10;
    private const int PATTERN_RECOGNITION_THRESHOLD = 3;

    public CombatMemory()
    {
        recentActions = new Queue<PlayerAction>();
        recognizedPatterns = new Dictionary<string, CombatPattern>();
    }

    public void RecordAction(PlayerAction action)
    {
        recentActions.Enqueue(action);
        if (recentActions.Count > MAX_MEMORY_SIZE)
            recentActions.Dequeue();
            
        AnalyzePattern();
    }

    public string PredictNextAction()
    {
        var currentSequence = GetCurrentActionSequence();
        foreach (var pattern in recognizedPatterns)
        {
            if (MatchesPatternStart(currentSequence, pattern.Value.Sequence))
                return pattern.Value.Sequence[currentSequence.Count].ActionType;
        }
        return null;
    }

    private void AnalyzePattern()
    {
        var sequence = new List<PlayerAction>(recentActions);
        string patternKey = GeneratePatternKey(sequence);
        
        if (!recognizedPatterns.ContainsKey(patternKey))
        {
            recognizedPatterns[patternKey] = new CombatPattern
            {
                Sequence = sequence,
                OccurrenceCount = 1,
                SuccessRate = 0
            };
        }
        else
        {
            recognizedPatterns[patternKey].OccurrenceCount++;
        }
    }

    private string GeneratePatternKey(List<PlayerAction> sequence)
    {
        return string.Join("-", sequence.ConvertAll(a => a.ActionType));
    }

    private List<PlayerAction> GetCurrentActionSequence()
    {
        return new List<PlayerAction>(recentActions);
    }

    private bool MatchesPatternStart(List<PlayerAction> current, List<PlayerAction> pattern)
    {
        if (current.Count >= pattern.Count) return false;
        for (int i = 0; i < current.Count; i++)
        {
            if (current[i].ActionType != pattern[i].ActionType)
                return false;
        }
        return true;
    }

    protected virtual void OnPatternRecognized(CombatPattern pattern)
    {
        // Override in derived classes
    }

    public virtual void UpdatePatternSuccess(string patternKey, bool wasSuccessful)
    {
        if (recognizedPatterns.ContainsKey(patternKey))
        {
            var pattern = recognizedPatterns[patternKey];
            float currentSuccess = pattern.SuccessRate * pattern.OccurrenceCount;
            pattern.OccurrenceCount++;
            pattern.SuccessRate = (currentSuccess + (wasSuccessful ? 1 : 0)) / pattern.OccurrenceCount;
        }
    }

    public virtual float GetPatternConfidence(string patternKey)
    {
        if (recognizedPatterns.ContainsKey(patternKey))
            return recognizedPatterns[patternKey].SuccessRate * recognizedPatterns[patternKey].OccurrenceCount / PATTERN_RECOGNITION_THRESHOLD;
        return 0f;
    }
}
