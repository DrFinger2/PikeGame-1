using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PikeRelease : MonoBehaviour
{
    float pikeTimelimit = 0;

    // Swapped from Vector3 to Transforms for easier scene placement
    [SerializeField] Transform playAreaPoint;
    [SerializeField] Transform outsideAreaPoint;
    
    [SerializeField] List<GameObject> playAreaPikes = new List<GameObject>();
    [SerializeField] List<GameObject> outsideAreaPikes = new List<GameObject>();
    
    [SerializeField] GameObject pikeHolder;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Simplified timer logic: runs as long as the timer is above 0
        if (pikeTimelimit > 0)
        {
            PikeTimer();
        }
    }


    public void ReleasePike()
    {
        // Check if we have pikes waiting outside and no timer blocking
        if (outsideAreaPikes.Count > 0 && pikeTimelimit <= 0)
        {
            // 1. Grab the next available pike
            GameObject pikeToRelease = outsideAreaPikes[0];

            // 2. Move it from the outside list to the play area list
            outsideAreaPikes.RemoveAt(0);
            playAreaPikes.Add(pikeToRelease);

            // 3. Activate and move its child to the playArea Transform
            pikeToRelease.SetActive(true);
            pikeToRelease.transform.GetChild(1).position = playAreaPoint.position;
        }
    }
    

    public void KickPike()
    {
        // Only kick if the timer is inactive and there are actually pikes in the play area
        if (pikeTimelimit <= 0 && playAreaPikes.Count > 0)
        {
            // Grab the most recently added pike in the play area
            int lastIndex = playAreaPikes.Count - 1;
            GameObject pikeToKick = playAreaPikes[lastIndex];

            // Move its child to the outsideArea Transform
            pikeToKick.transform.GetChild(1).position = outsideAreaPoint.position;
            
            // Set the 15-second timer
            pikeTimelimit = 15f;
        }
    }

    void PikeTimer()
    {
        pikeTimelimit -= Time.deltaTime;

        // Once the timer hits zero or lower, disable the pike and officially return it to the pool
        if (pikeTimelimit <= 0)
        {
            if (playAreaPikes.Count > 0)
            {
                int lastIndex = playAreaPikes.Count - 1;
                GameObject pikeToDisable = playAreaPikes[lastIndex];

                pikeToDisable.SetActive(false);

                // Move it back to the outside list
                playAreaPikes.RemoveAt(lastIndex);
                outsideAreaPikes.Add(pikeToDisable);
            }

            // Lock timer cleanly back to 0
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
            cow.transform.GetChild(1).position = targetPos;
        }
        cowsReleased = true;

    }
    

    public void SpawnPike()
    {
        GameObject pike = Instantiate(pikeHolder, new(-20, 0.5f, 5), pikeHolder.transform.rotation);
        pike.transform.GetChild(1).position = targetPos;
    }
    */
}