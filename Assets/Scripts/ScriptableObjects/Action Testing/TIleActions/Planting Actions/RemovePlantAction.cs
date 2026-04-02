using UnityEngine;

[CreateAssetMenu(fileName = "RemovePlantAction", menuName = "RemovePlantAction")]
public class RemovePlantAction : tileAction
{
    public override bool affectTile(gameTile tile)
    {
        /*
        foreach (GameObject plant in tile.plants)
        {   
            Destroy(plant);
        }
        tile.plants.Clear();
        */

        if (TurnManager.Instance.gameState.currentActionPoints >= 1)
        {
            var weedScript = tile.GetComponent<tileWeedsGrowth>();

            if (weedScript != null)
            {
                // Only spend AP and trigger success if the component actually exists
                TurnManager.Instance.gameState.currentActionPoints -= 1;
                TurnManager.Instance.onActionPointsChanged?.Invoke(TurnManager.Instance.gameState.currentActionPoints);

                bool removedInvasive = weedScript.growStage > 1;
                weedScript.growStage = 1;
                weedScript.UpdateWeedObject();


                if (removedInvasive && WetlandProgressionManager.Instance != null)
                {
                    WetlandProgressionManager.Instance.RegisterInvasiveRemoved(1);
                }

                if (TurnManager.Instance != null && TurnManager.Instance.milestoneHandler != null)
                {
                    TurnManager.Instance.milestoneHandler.RefreshBiodiversityNow();
                }

                return true; // Action was successful
            }
            else
            {
                Debug.LogWarning("RemovePlantAction: tileWeedsGrowth component missing on tile.");
                return false;
            }
        }
        else
        {
            Debug.Log("Not enough AP");
            // Optional: Trigger a UI warning here as well if you have one for Remove
            return false;
        }

        


        //TurnManager.Instance.onActionPointsChanged.Invoke(2);
    }

}
