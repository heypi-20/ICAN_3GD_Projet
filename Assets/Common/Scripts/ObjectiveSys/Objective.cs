﻿using System;
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
    public float CompletionTime { get; private set; }
    public void SetCompletionTime(float time) => CompletionTime = time;

    private readonly string _statusText;

    // Status text can have 2 parameters {0} and {1} for current and max value
    // Example: "Kill {0} of {1} enemies"
    public Objective(string eventTrigger, string statusText, int currentValue, int maxValue)
    {
        EventTrigger = eventTrigger;
        _statusText = statusText;
        CurrentValue = currentValue;
        MaxValue = maxValue;
    }

    public Objective(string statusText, int currentValue, int maxValue) : this("", statusText, currentValue, maxValue) {}

    private void CheckCompletion()
    {
        if (IsComplete)
            return;
        
        if (CurrentValue >= MaxValue) {
            IsComplete = true;
            OnComplete?.Invoke();
        }
    }

    private void CheckCompletionNegative()
    {
        if (IsComplete)
            return;
        
        if (CurrentValue <= 0 && !IsComplete) {
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

        CheckCompletion();
        if (!IsComplete)
            OnValueChange?.Invoke();
    }

    public void AddProgressNegative(int value)
    {
        if (IsComplete)
            return;
        CurrentValue -= value;

        if (CurrentValue < 0) {
            CurrentValue = 0;
        }

        CheckCompletionNegative();
        if (!IsComplete)
            OnValueChange?.Invoke();
    }

    public string GetStatusText()
    {
        return string.Format(_statusText, CurrentValue, MaxValue);
    }
}
