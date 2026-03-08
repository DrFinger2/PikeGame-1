using UnityEngine;

[System.Serializable]
public class WetlandSimulationState
{
    [Min(1)] public int day = 1;
    [Min(0)] public int coins = 0;
    [Min(0)] public int actionPoints = 4;

    [Range(0, 100)] public int waterClarity = 20;
    [Range(0, 100)] public int oxygen = 18;
    [Range(0, 100)] public int plantBalance = 15;
    [Range(0, 100)] public int animalBalance = 0;
    [Range(0, 100)] public int stability = 13;

    [Min(0)] public int invasiveCount = 6;
    [Min(0)] public int grassCount = 0;
    [Min(0)] public int reedCount = 0;
    [Min(0)] public int floatingLeafCount = 0;
    [Min(0)] public int submergedCount = 0;

    public bool smallFishPresent;
    public bool pikePresent;

    public bool milestoneWaterReadyReached;
    public bool milestoneFirstPikeReached;

    [Min(0)] public int victoryStreakDays;
    [Min(0)] public int failureStreakDays;

    public bool gameWon;
    public bool gameLost;

    public int UniquePlantTypes
    {
        get
        {
            int uniqueCount = 0;
            if (grassCount > 0) uniqueCount++;
            if (reedCount > 0) uniqueCount++;
            if (floatingLeafCount > 0) uniqueCount++;
            if (submergedCount > 0) uniqueCount++;
            return uniqueCount;
        }
    }

    public int BeneficialPlantCount => grassCount + reedCount + floatingLeafCount + submergedCount;

    public void ClampAll(int minStatValue, int maxStatValue)
    {
        waterClarity = Mathf.Clamp(waterClarity, minStatValue, maxStatValue);
        oxygen = Mathf.Clamp(oxygen, minStatValue, maxStatValue);
        plantBalance = Mathf.Clamp(plantBalance, minStatValue, maxStatValue);
        animalBalance = Mathf.Clamp(animalBalance, minStatValue, maxStatValue);
        stability = Mathf.Clamp(stability, minStatValue, maxStatValue);

        coins = Mathf.Max(0, coins);
        actionPoints = Mathf.Max(0, actionPoints);

        invasiveCount = Mathf.Max(0, invasiveCount);
        grassCount = Mathf.Max(0, grassCount);
        reedCount = Mathf.Max(0, reedCount);
        floatingLeafCount = Mathf.Max(0, floatingLeafCount);
        submergedCount = Mathf.Max(0, submergedCount);

        victoryStreakDays = Mathf.Max(0, victoryStreakDays);
        failureStreakDays = Mathf.Max(0, failureStreakDays);

        if (gameWon)
        {
            gameLost = false;
        }
    }
}
