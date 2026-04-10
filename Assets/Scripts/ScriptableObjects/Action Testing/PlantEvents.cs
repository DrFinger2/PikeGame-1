using System;

public static class PlantEvents
{
    public static event Action<WetlandPlantType, string> OnPlantPlaced;
    public static event Action<bool> OnPlantRemoved; 

    public static void TriggerPlantPlaced(WetlandPlantType plantType, string plantName) => OnPlantPlaced?.Invoke(plantType, plantName);
    public static void TriggerPlantRemoved(bool wasInvasive) => OnPlantRemoved?.Invoke(wasInvasive);
}