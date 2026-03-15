using UnityEngine;

public class RaccoonDogManager : MonoBehaviour
{

    [SerializeField] GameObject raccoonDog;
    [SerializeField] Vector3[] spawnPositions = new Vector3[4];
    public bool spawn;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(spawn == true)
        {
            spawn = false;
            SpawnCoons();
        }
    }





    public void SpawnCoons()
    {
        Vector3 spawnPos;

        spawnPos = spawnPositions[Random.Range(0, 4)];

        spawnPos.x += Random.Range(-1, 2);
        spawnPos.z += Random.Range(-1, 2);


        Instantiate(raccoonDog, spawnPos, raccoonDog.transform.rotation);
    }




}
