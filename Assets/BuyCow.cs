using UnityEngine;
using System.Collections.Generic;

public class BuyCowButton : MonoBehaviour
{
    public GameObject cowPrefab;
    public int cost = 2;

    private List<Transform> spawnPoints;
    private int currentSpawnIndex = 0;

    void Awake()
    {
        spawnPoints = new List<Transform>();

        GameObject parent = GameObject.Find("CowSpawnPoints");

        if (parent == null)
        {
            Debug.LogError("CowSpawnPoints not found!");
            return;
        }

        foreach (Transform child in parent.transform)
        {
            spawnPoints.Add(child);
        }
    }

    public void BuyCow()
    {
        GameState gameState = TurnManager.Instance.gameState;

        if (gameState.currentActionPoints < cost)
        {
            Debug.Log("Not enough energy!");
            return;
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points!");
            return;
        }

        
        gameState.currentActionPoints -= cost;
        TurnManager.Instance.onActionPointsChanged?.Invoke(gameState.currentActionPoints);

        
        Transform spawnPoint = spawnPoints[currentSpawnIndex];

        
        Instantiate(cowPrefab, spawnPoint.position, Quaternion.identity);

        
        currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Count;
    }
}