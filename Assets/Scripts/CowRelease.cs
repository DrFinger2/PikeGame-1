using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CowRelease : MonoBehaviour
{

    [SerializeField] bool releaseCows = false;
    int maxPikes;
    bool cowsReleased = false;
    int currentPike = 0;
    int activePikes = 0;
    
    float pikeTimelimit = 0;

    [SerializeField] Vector3 targetPos = new(0, 0, 0);
    [SerializeField] Vector3 targetPos2 = new(0, 0, 0);
    [SerializeField] List<GameObject> pikes = new();
    [SerializeField] GameObject pikeHolder;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxPikes = pikes.Count;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(releaseCows && !cowsReleased)
        {
            ReleasePike();
        }
        */

        if (pikeTimelimit > 1)
        {
            PikeTimer();
        }
        

    }


    

    public void ReleasePike()
    {
        if (activePikes < maxPikes && pikeTimelimit <= 0)
        {
            pikes[activePikes].SetActive(true);
            pikes[activePikes].transform.GetChild(1).transform.position = targetPos;

            if(activePikes == 0)
            {
                activePikes++; // 1 & 0
            }
            else
            {
                currentPike++; // 4
                activePikes++; // 5
            }
            
        }
    }


    public void KickPike()
    {
        if (pikeTimelimit <= 0)
        {
            if(activePikes > 0)
            {
                pikes[currentPike].transform.GetChild(1).transform.position = targetPos2;
                pikeTimelimit = 15;
            }

            
        }
        
    }


    void PikeTimer()
    {
        pikeTimelimit -= Time.deltaTime;

        if (pikeTimelimit < 1 && pikeTimelimit > 0)
        {
            pikes[currentPike].SetActive(false);
            activePikes--;
            currentPike--;

            if(currentPike < 0)
            {
                currentPike = 0;
            }

            pikeTimelimit = 0;
        }

    }


    //Old system, might delete later idk
    /*
    void ReleasePike()
    {
        foreach (GameObject cow in cows)
        {
            cow.SetActive(true);
            cow.transform.GetChild(1).transform.position = targetPos;
        }
        cowsReleased = true;

    }
    

    public void SpawnPike()
    {
        GameObject pike = Instantiate(pikeHolder, new(-20, 0.5f, 5), pikeHolder.transform.rotation);
        pike.transform.GetChild(1).transform.position = targetPos;
    }
    */


}
