using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RemovePlantAction", menuName = "RemovePlantAction")]
public class RemovePlantAction : tileAction
{
    public override bool affectTile(gameTile tile)
    {
        List<gameTile> tilesToCheck = new List<gameTile>();
        gameTile[] allTiles = FindObjectsOfType<gameTile>();

        foreach (gameTile t in allTiles)
        {
            if (Mathf.Abs(t.gridPosition.x - tile.gridPosition.x) <= 1 &&
                Mathf.Abs(t.gridPosition.y - tile.gridPosition.y) <= 1)
            {
                tilesToCheck.Add(t);
            }
        }

        tilesToCheck = tilesToCheck.OrderBy(t => Vector3.Distance(t.transform.position, tile.transform.position)).ToList();

        // 3. Loop through the candidates (Center first, then the 8 neighbors)
        foreach (gameTile currentTile in tilesToCheck)
        {
            var weedScript = currentTile.GetComponent<tileWeedsGrowth>();

            // Validation: Does THIS tile have a weed that can be cut?
            if (weedScript != null && weedScript.growStage > 1)
            {
                // Found a valid weed! Now check AP.
                if (TurnManager.Instance.gameState.currentActionPoints >= 1)
                {
                    // Execution: Deduct AP and update the state
                    TurnManager.Instance.gameState.currentActionPoints -= 1;
                    TurnManager.Instance.onActionPointsChanged?.Invoke(TurnManager.Instance.gameState.currentActionPoints);

                    bool removedInvasive = true;
                    weedScript.growStage = 1;
                    weedScript.UpdateWeedObject();

                    if (WetlandProgressionManager.Instance != null)
                    {
                        WetlandProgressionManager.Instance.RegisterInvasiveRemoved(1);
                    }

                    if (TurnManager.Instance?.milestoneHandler != null)
                    {
                        TurnManager.Instance.milestoneHandler.RefreshBiodiversityNow();
                    }

                    PlantEvents.TriggerPlantRemoved(removedInvasive);

                    // We successfully cut a weed, so stop searching and return true!
                    return true;
                }
                else
                {
                    // FIXED: This is now correctly attached to the AP check!
                    Debug.LogWarning($"[RemoveTool] Failed: You only have {TurnManager.Instance.gameState.currentActionPoints} AP!");
                    TurnManager.Instance.warningMessages?.ShowWarningAP();
                    return false;
                }
            }
        }

        // 4. If we checked the target tile AND all 8 neighbors, and found nothing to cut:
        return false;
    }
}