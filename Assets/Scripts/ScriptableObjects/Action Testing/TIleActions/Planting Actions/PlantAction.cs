using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlantAction", menuName = "PlantAction")]
public class PlantAction : tileAction
{
    public Plant[] plants;

    [Header("Simulation Tracking")]
    public WetlandPlantType plantedType = WetlandPlantType.Grass;
    [SerializeField] private bool autoDetectTypeFromPlantName = true;

    public override bool affectTile(gameTile tile)
{
    if (plants == null || plants.Length == 0)
    {
        Debug.LogWarning("PlantAction has no Plant assets assigned.");
        return false;
    }

    if (TurnManager.Instance.gameState.currentActionPoints >= 1)
    {
        if (tile.grownPlant == null)
        {
            if (tile.overgrownState < 3)
            {
                // ALL SUCCESS LOGIC HERE
                TurnManager.Instance.gameState.currentActionPoints -= 1;
                TurnManager.Instance.onActionPointsChanged?.Invoke(TurnManager.Instance.gameState.currentActionPoints);

                int randomIndex = Random.Range(0, plants.Length);
                tile.grownPlant = plants[randomIndex];
                tile.grownPlant.plantGrowStage = 0;
                tile.plantPrefab = plants[randomIndex].organismPrefab;
                tile.UpdatePlant();

                if (WetlandProgressionManager.Instance != null)
                {
                    WetlandProgressionManager.Instance.RegisterPlantPlaced(ResolvePlacedType(tile.grownPlant));
                }

                if (TurnManager.Instance != null && TurnManager.Instance.milestoneHandler != null)
                {
                    TurnManager.Instance.milestoneHandler.RefreshBiodiversityNow();
                }

                return true; // Action was successful
            }
            else
            {
                TurnManager.Instance.warningMessages.ShowWarningOvergrown();
                return false;
            }
            
        }
        else
        {
            TurnManager.Instance.warningMessages.ShowWarningExistingPlant();
            return false;
        }
    }
    else
    {
        TurnManager.Instance.warningMessages.ShowWarningAP();
        return false;
    }
}

    private WetlandPlantType ResolvePlacedType(Plant plant)
    {
        if (!autoDetectTypeFromPlantName || plant == null)
        {
            return plantedType;
        }

        string plantIdentity = (plant.name + " " + plant.organismName).ToLowerInvariant();

        if (plantIdentity.Contains("reed") || plantIdentity.Contains("ruov"))
        {
            return WetlandPlantType.Reed;
        }

        if (plantIdentity.Contains("float") || plantIdentity.Contains("leaf") || plantIdentity.Contains("lumme"))
        {
            return WetlandPlantType.FloatingLeaf;
        }

        if (plantIdentity.Contains("submerged") || plantIdentity.Contains("underwater"))
        {
            return WetlandPlantType.Submerged;
        }

        return plantedType;
    }
}
