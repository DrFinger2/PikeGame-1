using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public class tileManager : SingletonMonoBehaviour<tileManager>
{

    public enum TileType
    {
        Water,
        Wetland,
        Forest
    };
    
    public Material materialWetland;
    public Material materialWater;
    public Material materialForest;
    
    public gameTile selectedTile;

    public WetlandAction[] actionsWetland;

    public tileAction tileAction;
    public tileAction removePlantAction;

    public bool toolBeingUsed = false;


    //overall stats
    public int waterHealth;


    void Start()
    {
        Instantiate(materialWetland);
        Instantiate(materialWater);
        Instantiate(materialForest);
    }

    public void OverwriteAllWeeds(int growthStage)
    {
        gameTile[] allTiles = FindObjectsOfType<gameTile>();

        foreach (gameTile t in allTiles)
        {
            if (t.tileType == TileType.Forest) continue;
            tileWeedsGrowth weeds = t.GetComponent<tileWeedsGrowth>();
            if (weeds != null)
            {
                if (weeds.growStage == growthStage) continue; 
                weeds.growStage = growthStage;
                weeds.UpdateWeedObject();
            }
        }
    }



    /// <summary>
    /// Spawns weeds at the specified growth stage if the current count on the map is below the target.
    /// Only spawns the amount needed to reach the target.
    /// </summary>
    public void SpawnMinimumWeeds(int targetCount, int growthStage = 2)
    {
        gameTile[] allTiles = FindObjectsOfType<gameTile>();
        int currentCountAtStage = allTiles.Count(t => t.overgrownState == growthStage);

        int amountToSpawn = targetCount - currentCountAtStage;

        if (amountToSpawn <= 0)
        {
            Debug.Log($"[Weed Spawner]: Minimum weeds met ({currentCountAtStage}/{targetCount}). No spawning needed.");
            return;
        }

        Debug.Log($"[Weed Spawner]: Found {currentCountAtStage} weeds. Spawning {amountToSpawn} more to hit target of {targetCount}.");
        SpawnWeeds(amountToSpawn, growthStage);
    }
    public void SpawnWeeds(int amount, int growthStage = 2, float maxDistanceFromCenter = 3f)
    {
        gameTile[] allTiles = FindObjectsOfType<gameTile>();

        List<gameTile> validTiles = allTiles.Where(t =>
            (t.tileType == TileType.Wetland || t.tileType == TileType.Water) &&
            t.grownPlant == null &&
            (t.overgrownState == 0 || t.overgrownState == 1)
        ).ToList();

        if (validTiles.Count == 0) return;
        int actualSpawnAmount = Mathf.Min(amount, validTiles.Count);

        // 1. Calculate the center of the entire map
        Vector3 centerPoint = Vector3.zero;
        foreach (gameTile t in allTiles)
        {
            centerPoint += t.transform.position;
        }
        centerPoint /= allTiles.Length;

        // 2. Sort by distance to center, applying the new maxDistanceFromCenter parameter
        validTiles = validTiles.OrderBy(t => Vector3.Distance(t.transform.position, centerPoint) + Random.Range(0f, maxDistanceFromCenter)).ToList();

        for (int i = 0; i < actualSpawnAmount; i++)
        {
            gameTile targetTile = validTiles[0];

            tileWeedsGrowth weeds = targetTile.GetComponent<tileWeedsGrowth>();
            if (weeds != null)
            {
                weeds.growStage = growthStage;
                weeds.UpdateWeedObject();
            }

            validTiles.RemoveAt(0);
        }

        Debug.Log($"[Weed Spawner]: Spawned {actualSpawnAmount} weeds near the center of the map (Fuzz: {maxDistanceFromCenter}).");
    }
    



    void Update()
    {   
        /*
        if (Input.GetMouseButtonDown(0) && selectedTile != null)
        {
            tileAction.affectTile(selectedTile);
        }
        if (Input.GetMouseButtonDown(1) && selectedTile != null)
        {
            removePlantAction.affectTile(selectedTile);
        }
        */
    }

   
    


}
