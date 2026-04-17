using UnityEngine;
using UnityEngine.Events;

public class SimpleToggle : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onToggleOn;
    public UnityEvent onToggleOff;

    private bool isOn = false; // Tracks state internally

    public void Toggle()
    {
        if (isOn) ToggleOff();
        else ToggleOn();
    }

    public void ToggleOn()
    {
        if (isOn) return;
        
        isOn = true;
        onToggleOn?.Invoke();
    }

    public void ToggleOff()
    {
        if (!isOn) return;
        
        isOn = false;
        onToggleOff?.Invoke();
    }
}