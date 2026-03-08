using UnityEngine;

[CreateAssetMenu(fileName = "RemovePlantAction", menuName = "RemovePlantAction")]
public class RemovePlantAction : tileAction
{
    public override void affectTile(gameTile tile)
    {
        if (TurnManager.Instance.gameState.currentActionPoints >= 1)
        {
            TurnManager.Instance.gameState.currentActionPoints -= 1;
            TurnManager.Instance.onActionPointsChanged?.Invoke(TurnManager.Instance.gameState.currentActionPoints);

            var weedScript = tile.GetComponent<tileWeedsGrowth>();
            if (weedScript != null)
            {
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
            }
            else
            {
                Debug.LogWarning("RemovePlantAction: tileWeedsGrowth component missing on tile.");
            }
        }
        else
        {
            Debug.Log("Not enough AP");
        }
    }
}
