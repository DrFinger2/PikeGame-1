using UnityEngine;

public class RaccoonDogManager : MonoBehaviour
{

    [SerializeField] GameObject raccoonDog;
    [SerializeField] Vector3[] spawnPositions = new Vector3[4];
    public bool spawn;
    public bool isSpawning = false;
    [SerializeField] float spawnCooldown = 80;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawning)
        {
            if (spawn == true)
            {
                spawn = false;
                SpawnRacs();
            }

            CoonCooldown();
        }
        
    }





    public void SpawnRacs()
    {
        Vector3 spawnPos;

        spawnPos = spawnPositions[Random.Range(0, 4)];

        spawnPos.x += Random.Range(-1, 2);
        spawnPos.z += Random.Range(-1, 2);


        Instantiate(raccoonDog, spawnPos, raccoonDog.transform.rotation);
    }

    public void CoonCooldown()
    {
        if(spawnCooldown > 0)
        {
            spawnCooldown -= Time.deltaTime;
        }
        else
        {
            spawnCooldown = 80;
            SpawnRacs();
        }
    }


    public void SpawnRaccoonInLocation(Transform pos)
    {
        Instantiate(raccoonDog, pos.position, raccoonDog.transform.rotation);
    }


}
