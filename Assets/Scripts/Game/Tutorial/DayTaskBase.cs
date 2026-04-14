using UnityEngine;
using UnityEngine.UI;
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
}
