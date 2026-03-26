using UnityEngine;

public static class WetlandDayResolver
{
    public static WetlandDayResolution Resolve(WetlandSimulationState state, WetlandBalanceConfig config, int nextDayNumber)
    {
        WetlandDayResolution resolution = new WetlandDayResolution();

        if (state == null || config == null)
        {
            return resolution;
        }

        state.day = Mathf.Max(1, nextDayNumber);

        resolution.weather = RollWeather(config);

        int previousWater = state.waterClarity;
        int previousOxygen = state.oxygen;

        int waterDelta = 0;
        int oxygenDelta = 0;

        int weatherInvasiveBonus = ApplyWeather(resolution.weather, config, ref waterDelta, ref oxygenDelta);

        if (state.grassCount < config.grassCancelWaterDecayThreshold)
        {
            waterDelta -= config.baseDailyWaterDecay;
        }

        waterDelta += state.grassCount * config.grassWaterBonusPerPlant;
        waterDelta += Mathf.Min(config.reedWaterBonusPerDayCap, state.reedCount * config.reedWaterBonusPerPlant);

        oxygenDelta += Mathf.Min(config.submergedOxygenBonusPerDayCap, state.submergedCount * config.submergedOxygenBonusPerPlant);

        int floatingBonus = Mathf.Min(config.floatingBonusPerDayCap, state.floatingLeafCount * config.floatingOxygenBonusPerPlant);
        oxygenDelta += floatingBonus;

        waterDelta -= state.invasiveCount * config.invasiveWaterPenaltyPerPlant;
        oxygenDelta -= state.invasiveCount * config.invasiveOxygenPenaltyPerPlant;

        int soilCoverage = state.grassCount + state.reedCount + state.invasiveCount;
        if (soilCoverage < config.tooOpenCoverageThreshold)
        {
            waterDelta -= config.tooOpenWaterPenalty;
        }

        state.waterClarity = ClampStat(previousWater + waterDelta, config);
        state.oxygen = ClampStat(previousOxygen + oxygenDelta, config);

        resolution.waterDelta = state.waterClarity - previousWater;
        resolution.oxygenDelta = state.oxygen - previousOxygen;

        int previousInvasive = state.invasiveCount;
        int invasiveSpawnFromBalance = GetInvasiveSpawnFromBalance(state.plantBalance, config);
        int totalInvasiveSpawn = invasiveSpawnFromBalance + weatherInvasiveBonus;
        state.invasiveCount = Mathf.Clamp(state.invasiveCount + totalInvasiveSpawn, 0, config.maxInvasiveCount);
        resolution.invasiveDelta = state.invasiveCount - previousInvasive;

        RecalculatePlantBalance(state, config, resolution);
        RecalculateAnimalBalance(state, config, resolution);
        RecalculateStability(state, config, resolution);

        ApplyMilestonesAndDailyCoins(state, config, resolution);
        UpdateOutcomeStreaks(state, config, resolution);

        state.ClampAll(config.minStatValue, config.maxStatValue);
        return resolution;
    }

    private static void RecalculatePlantBalance(WetlandSimulationState state, WetlandBalanceConfig config, WetlandDayResolution resolution)
    {
        int previousPlantBalance = state.plantBalance;

        int beneficialPlants = state.BeneficialPlantCount;
        int monoPenalty = 0;

        if (beneficialPlants >= config.monoPenaltyMinBeneficialPlants)
        {
            int biggestType = Mathf.Max(Mathf.Max(state.grassCount, state.reedCount), Mathf.Max(state.floatingLeafCount, state.submergedCount));
            float share = beneficialPlants > 0 ? (float)biggestType / beneficialPlants : 0f;
            if (share >= config.monoPenaltyShareThreshold)
            {
                monoPenalty = config.monoPenaltyValue;
            }
        }

        int rawPlantBalance = config.plantBalanceBase;
        rawPlantBalance += state.UniquePlantTypes * config.plantBalancePerUniqueType;
        rawPlantBalance += Mathf.Min(config.plantBalanceBeneficialPlantCap, beneficialPlants);
        rawPlantBalance -= state.invasiveCount * config.plantBalanceInvasivePenaltyPerPlant;
        rawPlantBalance -= monoPenalty;

        state.plantBalance = ClampStat(rawPlantBalance, config);
        resolution.plantBalanceDelta = state.plantBalance - previousPlantBalance;
    }

    private static void RecalculateAnimalBalance(WetlandSimulationState state, WetlandBalanceConfig config, WetlandDayResolution resolution)
    {
        int previousAnimalBalance = state.animalBalance;

        state.smallFishPresent = state.waterClarity >= config.smallFishWaterThreshold && state.oxygen >= config.smallFishOxygenThreshold;
        state.pikePresent =
            state.smallFishPresent &&
            state.waterClarity >= config.pikeWaterThreshold &&
            state.oxygen >= config.pikeOxygenThreshold &&
            state.submergedCount >= config.pikeRequiredSubmergedPlants &&
            state.invasiveCount <= config.pikeMaxInvasivePlants;

        int rawAnimalBalance = 0;
        if (state.smallFishPresent)
        {
            rawAnimalBalance += config.smallFishAnimalScore;
        }

        if (state.pikePresent)
        {
            rawAnimalBalance += config.pikeAnimalScore;
        }

        rawAnimalBalance += Mathf.Min(config.animalFloatingBonusCap, state.floatingLeafCount * config.animalFloatingBonusPerPlant);
        rawAnimalBalance -= Mathf.Min(config.animalInvasivePenaltyCap, state.invasiveCount * config.animalInvasivePenaltyPerPlant);

        state.animalBalance = ClampStat(rawAnimalBalance, config);
        resolution.animalBalanceDelta = state.animalBalance - previousAnimalBalance;
    }

    private static void RecalculateStability(WetlandSimulationState state, WetlandBalanceConfig config, WetlandDayResolution resolution)
    {
        int previousStability = state.stability;
        int rawStability = Mathf.RoundToInt((state.waterClarity + state.oxygen + state.plantBalance + state.animalBalance) / 4f);
        state.stability = ClampStat(rawStability, config);
        resolution.stabilityDelta = state.stability - previousStability;
    }

    private static void ApplyMilestonesAndDailyCoins(WetlandSimulationState state, WetlandBalanceConfig config, WetlandDayResolution resolution)
    {
        int previousCoins = state.coins;

        if (!state.milestoneWaterReadyReached &&
            (state.waterClarity >= config.firstMilestoneWaterThreshold || state.oxygen >= config.firstMilestoneOxygenThreshold))
        {
            state.milestoneWaterReadyReached = true;
            state.coins += config.firstMilestoneCoinReward;
            resolution.firstMilestoneReached = true;
        }

        if (!state.milestoneFirstPikeReached && state.pikePresent)
        {
            state.milestoneFirstPikeReached = true;
            state.coins += config.firstPikeCoinReward;
            resolution.firstPikeMilestoneReached = true;
        }

        if (state.day >= 6 && state.stability >= config.dailyRewardMinStability)
        {
            state.coins += config.dailyRewardCoins;
        }

        resolution.coinsDelta = state.coins - previousCoins;
    }

    private static void UpdateOutcomeStreaks(WetlandSimulationState state, WetlandBalanceConfig config, WetlandDayResolution resolution)
    {
        bool isVictoryDay =
            state.stability >= config.victoryStabilityThreshold &&
            state.waterClarity >= config.victoryWaterThreshold &&
            state.oxygen >= config.victoryOxygenThreshold &&
            state.pikePresent &&
            state.invasiveCount <= config.victoryMaxInvasivePlants;

        if (isVictoryDay)
        {
            state.victoryStreakDays++;
        }
        else
        {
            state.victoryStreakDays = 0;
        }

        bool isFailureDay = state.stability < config.failureStabilityThreshold;
        if (isFailureDay)
        {
            state.failureStreakDays++;
        }
        else
        {
            state.failureStreakDays = 0;
        }

        if (!state.gameWon && state.victoryStreakDays >= config.victoryRequiredStreakDays)
        {
            state.gameWon = true;
            resolution.wonThisDay = true;
        }

        if (!state.gameLost && state.failureStreakDays >= config.failureRequiredStreakDays)
        {
            state.gameLost = true;
            resolution.lostThisDay = true;
        }
    }

    private static int GetInvasiveSpawnFromBalance(int plantBalance, WetlandBalanceConfig config)
    {
        if (plantBalance < config.invasiveLowBalanceThreshold)
        {
            return config.invasiveSpawnWhenLowBalance;
        }

        if (plantBalance < config.invasiveMediumBalanceThreshold)
        {
            return config.invasiveSpawnWhenMediumBalance;
        }

        return 0;
    }

    private static int ApplyWeather(WeatherType weather, WetlandBalanceConfig config, ref int waterDelta, ref int oxygenDelta)
    {
        switch (weather)
        {
            case WeatherType.Rain:
                waterDelta -= config.rainWaterPenalty;
                return config.rainInvasiveSpawnBonus;
            case WeatherType.HeavyRain:
                waterDelta -= config.heavyRainWaterPenalty;
                return config.heavyRainInvasiveSpawnBonus;
            case WeatherType.SunnyCalm:
                waterDelta += config.sunnyCalmWaterBonus;
                oxygenDelta += config.sunnyCalmOxygenBonus;
                return 0;
            default:
                return 0;
        }
    }

    private static WeatherType RollWeather(WetlandBalanceConfig config)
    {
        int clearChance = Mathf.Max(0, config.clearChance);
        int rainChance = Mathf.Max(0, config.rainChance);
        int heavyRainChance = Mathf.Max(0, config.heavyRainChance);
        int sunnyCalmChance = Mathf.Max(0, config.sunnyCalmChance);

        int totalChance = clearChance + rainChance + heavyRainChance + sunnyCalmChance;
        if (totalChance <= 0)
        {
            return WeatherType.Clear;
        }

        int roll = Random.Range(0, totalChance);

        if (roll < clearChance)
        {
            return WeatherType.Clear;
        }

        roll -= clearChance;
        if (roll < rainChance)
        {
            return WeatherType.Rain;
        }

        roll -= rainChance;
        if (roll < heavyRainChance)
        {
            return WeatherType.HeavyRain;
        }

        return WeatherType.SunnyCalm;
    }

    private static int ClampStat(int value, WetlandBalanceConfig config)
    {
        int minValue = Mathf.Min(config.minStatValue, config.maxStatValue);
        int maxValue = Mathf.Max(config.minStatValue, config.maxStatValue);
        return Mathf.Clamp(value, minValue, maxValue);
    }
}
