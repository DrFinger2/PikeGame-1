using UnityEngine;
using System.Collections.Generic;

public class BuyCowButton : MonoBehaviour
{
    public GameObject cowPrefab;
    public int cost = 2;

    private int currentSpawnIndex = 0;

    // SpawnPoints
    private List<Vector3> spawnPositions = new List<Vector3>()
    {
        new Vector3(0, 0, 9),
        new Vector3(0, 0, 0),
        new Vector3(-6, 0, 5)
    };

    public void BuyCow()
    {
        GameState gameState = TurnManager.Instance.gameState;

        if (gameState.currentActionPoints < cost)
        {
            Debug.Log("Not enough energy!");
            return;
        }

        if (spawnPositions.Count == 0)
        {
            Debug.LogError("No spawn positions!");
            return;
        }

        
        gameState.currentActionPoints -= cost;
        TurnManager.Instance.onActionPointsChanged?.Invoke(gameState.currentActionPoints);

        // Choose spawn position
        Vector3 spawnPos = spawnPositions[currentSpawnIndex];

        // Spawn cow
        Instantiate(cowPrefab, spawnPos, Quaternion.identity);

        
        currentSpawnIndex = (currentSpawnIndex + 1) % spawnPositions.Count;
    }
}