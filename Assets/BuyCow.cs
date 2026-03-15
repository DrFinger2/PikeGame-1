using UnityEngine;

public class BuyCowButton : MonoBehaviour
{
    public GameObject cowPrefab;     
    public Transform spawnPoint;     
    public int cost = 2;             

    public void BuyCow()
    {
        GameState gameState = TurnManager.Instance.gameState;

        if (gameState.currentActionPoints < cost)
        {
            Debug.Log("Not enough energy!");
            return;
        }

        // Vähennä energiaa
        gameState.currentActionPoints -= cost;
        TurnManager.Instance.onActionPointsChanged?.Invoke(gameState.currentActionPoints);

        // Spawnaa lehmä
        Instantiate(cowPrefab, spawnPoint.position, Quaternion.identity);
    }
}