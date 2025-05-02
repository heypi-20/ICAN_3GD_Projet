using System;
using UnityEngine;

public class Objective
{
    // Invoke when objective completed
    public Action OnComplete;
    // Invoke when objective's progress changes
    public Action OnValueChange;
    
    public string EventTrigger  { get; }
    public bool IsComplete { get; private set; }
    public int MaxValue { get; }
    public int CurrentValue { get; private set; }

    private readonly string _statusText;

    // Status text can have 2 parameters {0} and {1} for current and max value
    // Example: "Kill {0} of {1} enemies"
    public Objective(string eventTrigger, string statusText, int maxValue)
    {
        EventTrigger = eventTrigger;
        _statusText = statusText;
        MaxValue = maxValue;
    }

    public Objective(string statusText, int maxValue) : this("", statusText, maxValue) {}

    private void CheckCompletion()
    {
        if (CurrentValue >= MaxValue) {
            IsComplete = true;
            OnComplete?.Invoke();
        }
    }

    public void AddProgress(int value)
    {
        if (IsComplete)
            return;
        CurrentValue += value;

        if (CurrentValue > MaxValue) {
            CurrentValue = MaxValue;
        }
        
        OnValueChange?.Invoke();
        CheckCompletion();
    }

    public string GetStatusText()
    {
        return string.Format(_statusText, CurrentValue, MaxValue);
    }
}

