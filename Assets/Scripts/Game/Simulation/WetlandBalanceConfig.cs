using UnityEngine;

[CreateAssetMenu(fileName = "WetlandBalanceConfig", menuName = "PikeGame/Wetland Balance Config")]
public class WetlandBalanceConfig : ScriptableObject
{
    [Header("Start State")]
    [Min(1)] public int startingDay = 1;
    [Min(0)] public int startingCoins = 0;
    [Min(0)] public int startingActionPoints = 4;

    [Range(0, 100)] public int startingWaterClarity = 20;
    [Range(0, 100)] public int startingOxygen = 18;
    [Range(0, 100)] public int startingPlantBalance = 15;
    [Range(0, 100)] public int startingAnimalBalance = 0;

    [Min(0)] public int startingInvasiveCount = 6;
    [Min(0)] public int startingGrassCount = 0;
    [Min(0)] public int startingReedCount = 0;
    [Min(0)] public int startingFloatingLeafCount = 0;
    [Min(0)] public int startingSubmergedCount = 0;

    [Header("Action Economy")]
    [Min(0)] public int quizCorrectCoinReward = 5;
    [Min(0)] public int quizWrongCoinReward = 1;

    [Header("Daily Water And Oxygen")]
    [Min(0)] public int baseDailyWaterDecay = 5;
    [Min(0)] public int grassCancelWaterDecayThreshold = 5;
    [Min(0)] public int grassWaterBonusPerPlant = 1;

    [Min(0)] public int reedWaterBonusPerPlant = 2;
    [Min(0)] public int reedWaterBonusPerDayCap = 12;

    [Min(0)] public int submergedOxygenBonusPerPlant = 3;
    [Min(0)] public int submergedOxygenBonusPerDayCap = 15;

    [Min(0)] public int floatingOxygenBonusPerPlant = 1;
    [Min(0)] public int floatingBonusPerDayCap = 8;

    [Min(0)] public int invasiveWaterPenaltyPerPlant = 2;
    [Min(0)] public int invasiveOxygenPenaltyPerPlant = 1;

    [Min(0)] public int tooOpenCoverageThreshold = 4;
    [Min(0)] public int tooOpenWaterPenalty = 8;

    [Header("Plant Balance")]
    [Min(0)] public int plantBalanceBase = 10;
    [Min(0)] public int plantBalancePerUniqueType = 12;
    [Min(0)] public int plantBalanceBeneficialPlantCap = 20;
    [Min(0)] public int plantBalanceInvasivePenaltyPerPlant = 4;

    [Min(1)] public int monoPenaltyMinBeneficialPlants = 6;
    [Range(0f, 1f)] public float monoPenaltyShareThreshold = 0.70f;
    [Min(0)] public int monoPenaltyValue = 10;

    [Header("Invasive Spawn")]
    [Range(0, 100)] public int invasiveLowBalanceThreshold = 50;
    [Range(0, 100)] public int invasiveMediumBalanceThreshold = 70;
    [Min(0)] public int invasiveSpawnWhenLowBalance = 2;
    [Min(0)] public int invasiveSpawnWhenMediumBalance = 1;
    [Min(0)] public int maxInvasiveCount = 50;

    [Header("Animal Rules")]
    [Range(0, 100)] public int smallFishWaterThreshold = 45;
    [Range(0, 100)] public int smallFishOxygenThreshold = 40;
    [Range(0, 100)] public int smallFishAnimalScore = 30;

    [Range(0, 100)] public int pikeWaterThreshold = 70;
    [Range(0, 100)] public int pikeOxygenThreshold = 65;
    [Min(0)] public int pikeRequiredSubmergedPlants = 3;
    [Min(0)] public int pikeMaxInvasivePlants = 2;
    [Range(0, 100)] public int pikeAnimalScore = 40;

    [Min(0)] public int animalFloatingBonusPerPlant = 2;
    [Min(0)] public int animalFloatingBonusCap = 20;
    [Min(0)] public int animalInvasivePenaltyPerPlant = 2;
    [Min(0)] public int animalInvasivePenaltyCap = 20;

    [Header("Weather Chances")]
    [Min(0)] public int clearChance = 55;
    [Min(0)] public int rainChance = 25;
    [Min(0)] public int heavyRainChance = 10;
    [Min(0)] public int sunnyCalmChance = 10;

    [Header("Weather Effects")]
    [Min(0)] public int rainWaterPenalty = 8;
    [Min(0)] public int rainInvasiveSpawnBonus = 1;
    [Min(0)] public int heavyRainWaterPenalty = 12;
    [Min(0)] public int heavyRainInvasiveSpawnBonus = 2;
    [Min(0)] public int sunnyCalmWaterBonus = 2;
    [Min(0)] public int sunnyCalmOxygenBonus = 5;

    [Header("Milestones And Coins")]
    [Range(0, 100)] public int firstMilestoneWaterThreshold = 50;
    [Range(0, 100)] public int firstMilestoneOxygenThreshold = 45;
    [Min(0)] public int firstMilestoneCoinReward = 20;
    [Min(0)] public int firstPikeCoinReward = 30;
    [Range(0, 100)] public int dailyRewardMinStability = 50;
    [Min(0)] public int dailyRewardCoins = 2;

    [Header("Win Lose")]
    [Range(0, 100)] public int victoryStabilityThreshold = 90;
    [Range(0, 100)] public int victoryWaterThreshold = 75;
    [Range(0, 100)] public int victoryOxygenThreshold = 70;
    [Min(0)] public int victoryMaxInvasivePlants = 2;
    [Min(1)] public int victoryRequiredStreakDays = 6;

    [Range(0, 100)] public int failureStabilityThreshold = 20;
    [Min(1)] public int failureRequiredStreakDays = 3;

    [Header("General Limits")]
    [Range(0, 100)] public int minStatValue = 0;
    [Range(0, 100)] public int maxStatValue = 100;
    [Min(1)] public int maxActionPointsPerDay = 8;
}
