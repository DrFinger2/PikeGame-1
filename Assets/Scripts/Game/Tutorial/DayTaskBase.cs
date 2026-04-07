using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public abstract class DayTaskBase : MonoBehaviour
{

    [System.Serializable]
    public class DayEvents
    {
        public UnityEvent OnDayCompleted = new();
        public UnityEvent OnDayStarted = new();
        public UnityEvent OnDayEnded = new();
    }


    public DayEvents Events = new DayEvents();

    public int extraPointsPerDay;


    public abstract void StartDay();
    public abstract void EndDay();

    protected void CompleteDay()
    {
        Events.OnDayCompleted?.Invoke();
    }

    protected void SetInteractable(bool state, params Button[] buttons)
    {
        foreach (var btn in buttons) 
            if (btn != null) btn.interactable = state;
    }

    protected void SetActive(bool state, params GameObject[] objects)
    {
        foreach (var obj in objects) 
            if (obj != null) obj.SetActive(state);
    }
}