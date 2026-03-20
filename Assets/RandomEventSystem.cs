using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class RandomEventSystem : MonoBehaviour
{
    public static RandomEventSystem instance;

    [Header("Event Weights")]
    [SerializeField, Range(0f, 100f)] private float catastrophicWeight = 5f;
    [SerializeField, Range(0f, 100f)] private float badWeight = 20f;
    [SerializeField, Range(0f, 100f)] private float neutralWeight = 55f;
    [SerializeField, Range(0f, 100f)] private float goodWeight = 20f;

    [Header("Event Cooldowns")]
    [SerializeField] private int neutralCooldown = 0;
    [SerializeField] private int badCooldown = 1;
    [SerializeField] private int goodCooldown = 2;
    [SerializeField] private int catastrophicCooldown = 3;

    [Header("Event Storage")]
    [SerializeField] private List<WetlandEvent> forcedEvents = new List<WetlandEvent>();
    [SerializeField] private List<WetlandEvent> catastrophicEvents = new List<WetlandEvent>();
    [SerializeField] private List<WetlandEvent> badEvents = new List<WetlandEvent>();
    [SerializeField] private List<WetlandEvent> neutralEvents = new List<WetlandEvent>();
    [SerializeField] private List<WetlandEvent> goodEvents = new List<WetlandEvent>();

    [Header("Other")]
    [SerializeField] private int eventQueLength;

    public Queue<WetlandEvent> eventQue = new Queue<WetlandEvent>();

    private Dictionary<EventCategory, int> categoryCooldowns = new Dictionary<EventCategory, int>();
    private Dictionary<EventCategory, float> currentWeights = new Dictionary<EventCategory, float>();
    private Dictionary<EventCategory, float> baseWeights = new Dictionary<EventCategory, float>();

    // ✅ NEW: tracks unused events per category
    private Dictionary<EventCategory, List<WetlandEvent>> unusedEvents = new Dictionary<EventCategory, List<WetlandEvent>>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        InitializeWeights();
        InitializeCooldowns();
        InitializeUnusedEvents(); // ✅ NEW

        for (int i = 0; i < eventQueLength; i++)
        {
            GenerateNewEvent();
        }
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            for (int i = 0; i < 100f; i++)
                GenerateNewEvent();
        }
    }

    private void InitializeWeights()
    {
        baseWeights[EventCategory.Catastrophic] = catastrophicWeight;
        baseWeights[EventCategory.Bad] = badWeight;
        baseWeights[EventCategory.Neutral] = neutralWeight;
        baseWeights[EventCategory.Good] = goodWeight;
    }

    private void InitializeCooldowns()
    {
        categoryCooldowns[EventCategory.Catastrophic] = 0;
        categoryCooldowns[EventCategory.Bad] = 0;
        categoryCooldowns[EventCategory.Neutral] = 0;
        categoryCooldowns[EventCategory.Good] = 0;
    }

    // ✅ NEW
    private void InitializeUnusedEvents()
    {
        unusedEvents[EventCategory.Catastrophic] = new List<WetlandEvent>(catastrophicEvents);
        unusedEvents[EventCategory.Bad] = new List<WetlandEvent>(badEvents);
        unusedEvents[EventCategory.Neutral] = new List<WetlandEvent>(neutralEvents);
        unusedEvents[EventCategory.Good] = new List<WetlandEvent>(goodEvents);
    }

    public void ForceNextEvent(string forcedEventId)
    {
        WetlandEvent nextEvent = null;

        foreach (WetlandEvent evt in forcedEvents)
        {
            if (evt.eventId == forcedEventId)
            {
                nextEvent = evt;
                break;
            }
        }

        WetlandEvent[] events = eventQue.ToArray();
        eventQue.Clear();

        eventQue.Enqueue(nextEvent);

        foreach (WetlandEvent evt in events)
            eventQue.Enqueue(evt);
    }

    public WetlandEvent GetNextEvent()
    {
        if (eventQue.Count == 0)
        {
            GenerateNewEvent();
        }

        WetlandEvent nextEvent = eventQue.Dequeue();
        GenerateNewEvent();

        return nextEvent;
    }

    private void GenerateNewEvent()
    {
        AdjustWeights();
        EventCategory category = SelectCategory();
        WetlandEvent newEvent = SelectRandomEvent(category);

        if (newEvent != null)
        {
            eventQue.Enqueue(newEvent);
            UpdateEventsCooldowns(category);
            Debug.Log($"Generated event type: {category} with name: {newEvent.name}");
        }
    }

    private EventCategory SelectCategory()
    {
        float totalWeight = 0;

        foreach (var weight in currentWeights)
        {
            totalWeight += weight.Value;
        }

        float randomValue = Random.Range(0, totalWeight);
        float checkWeight = 0;

        foreach (var weight in currentWeights)
        {
            checkWeight += weight.Value;

            if (randomValue <= checkWeight)
            {
                return weight.Key;
            }
        }

        return EventCategory.Neutral;
    }

    // ✅ MODIFIED (core change)
    private WetlandEvent SelectRandomEvent(EventCategory category)
    {
        if (!unusedEvents.ContainsKey(category))
            return null;

        // Refill when all used
        if (unusedEvents[category].Count == 0)
        {
            switch (category)
            {
                case EventCategory.Catastrophic:
                    unusedEvents[category] = new List<WetlandEvent>(catastrophicEvents);
                    break;
                case EventCategory.Bad:
                    unusedEvents[category] = new List<WetlandEvent>(badEvents);
                    break;
                case EventCategory.Neutral:
                    unusedEvents[category] = new List<WetlandEvent>(neutralEvents);
                    break;
                case EventCategory.Good:
                    unusedEvents[category] = new List<WetlandEvent>(goodEvents);
                    break;
            }
        }

        var list = unusedEvents[category];

        if (list.Count == 0)
        {
            Debug.Log("no events");
            return null;
        }

        int randomIndex = Random.Range(0, list.Count);
        WetlandEvent selected = list[randomIndex];

        // Prevent repeat this cycle
        list.RemoveAt(randomIndex);

        return selected;
    }

    private void UpdateEventsCooldowns(EventCategory category)
    {
        switch (category)
        {
            case EventCategory.Catastrophic:
                categoryCooldowns[category] = catastrophicCooldown;
                break;
            case EventCategory.Bad:
                categoryCooldowns[category] = badCooldown;
                break;
            case EventCategory.Neutral:
                categoryCooldowns[category] = neutralCooldown;
                break;
            case EventCategory.Good:
                categoryCooldowns[category] = goodCooldown;
                break;
        }

        foreach (var key in categoryCooldowns.Keys.ToList())
        {
            if (categoryCooldowns[key] > 0)
            {
                categoryCooldowns[key]--;
            }
        }
    }

    private void AdjustWeights()
    {
        currentWeights[EventCategory.Catastrophic] = baseWeights[EventCategory.Catastrophic];
        currentWeights[EventCategory.Bad] = baseWeights[EventCategory.Bad];
        currentWeights[EventCategory.Neutral] = baseWeights[EventCategory.Neutral];
        currentWeights[EventCategory.Good] = baseWeights[EventCategory.Good];

        foreach (var cooldown in categoryCooldowns)
        {
            if (cooldown.Value > 0)
            {
                currentWeights[cooldown.Key] = 0;
            }
        }

        bool hasValidCategory = false;

        foreach (var weight in currentWeights)
        {
            if (weight.Value > 0)
            {
                hasValidCategory = true;
            }
        }

        if (!hasValidCategory)
        {
            currentWeights[EventCategory.Neutral] = baseWeights[EventCategory.Neutral];
        }
    }

    public WetlandEvent CheckNextEvent()
    {
        if (eventQue.Count > 0)
        {
            Debug.Log("peeking eventque");

            int j = 0;
            foreach (WetlandEvent e in eventQue)
            {
                Debug.Log($"Event: {e.name} at id: {j}");
                j++;
            }

            Debug.Log($"when peeking we get {eventQue.Peek()}");
            return eventQue.Peek();
        }
        else
        {
            GenerateNewEvent();
            return eventQue.Peek();
        }
    }
}