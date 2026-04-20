using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class tileWeedsGrowth : MonoBehaviour
{
    public gameTile tile;

    public int growStage;
    private GameObject currentGrowth;
    public GameObject growStage1;
    public GameObject growStage2;
    public GameObject growStage3;

    private TurnManager tm;


    void Start()
    {
        tm = TurnManager.Instance;
        tm.onTurnChanged.AddListener(SpreadPlants);
        tile = transform.GetComponent<gameTile>();

        growStage = 1;
        UpdateWeedObject();

        if (this.gameObject.activeSelf == false)
        {
            this.enabled = false;
        }
    }
    
    
    private void GrowWeeds(int random)
    {
        if (this.gameObject.activeSelf == true)
        {
            float randomValue = Random.Range(0.0f, 1.0f);

            switch (tile.tileType) // <-- use different random values for water / wet areas (this feature was discarded)
            {
                case tileManager.TileType.Water:

                    if (randomValue > 2f)
                    {
                        if (growStage < 3)
                        {
                            growStage++;
                            UpdateWeedObject();
                        }
                    }

                    break;


                case tileManager.TileType.Wetland:
                    if (randomValue > 0.96f)
                    {
                        if (growStage < 3)
                        {
                            growStage++;
                            UpdateWeedObject();
                        }
                    }

                    break;


                case tileManager.TileType.Forest:
                    break;
            }
        }
    }

    public void UpdateWeedObject()
    {
        switch (growStage)
        {
            case 0:
                Destroy(currentGrowth);
                break;
            case 1:
                Destroy(currentGrowth);
                currentGrowth = Instantiate(growStage1, tile.transform.position, growStage1.transform.rotation * Quaternion.Euler(0, 0, Random.Range(0f, 180f)));
                break;
            case 2:
                Destroy(currentGrowth);
                currentGrowth = Instantiate(growStage2, tile.transform.position, growStage2.transform.rotation * Quaternion.Euler(0, 0, Random.Range(0f, 180f)));
                break;
            case 3:
                Destroy(currentGrowth);
                currentGrowth = Instantiate(growStage3, tile.transform.position, growStage3.transform.rotation * Quaternion.Euler(0, 0, Random.Range(0f, 180f)));
                break;
        }

        tile.overgrownState = growStage;
    }


    public void CutWeeds()
    {
        growStage = 1;
        UpdateWeedObject();

    }

    public void SpreadPlants(int random)
    {   
        if (growStage > 0)
        {
            var adjacents = tile.adjacentTiles;
            int randomChoice = Random.Range(0, adjacents.Count - 1);
            var chosenTile = adjacents[randomChoice];
            chosenTile.GetComponent<tileWeedsGrowth>().GrowWeeds(0);
        }  
    }
        
}
