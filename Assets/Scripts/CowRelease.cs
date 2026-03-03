using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CowRelease : MonoBehaviour
{

    [SerializeField] bool releaseCows = false;
    bool cowsReleased = false;
    

    [SerializeField] Vector3 targetPos = new(0, 0, 0);
    [SerializeField] List<GameObject> cows = new();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
        if(releaseCows && !cowsReleased)
        {
            ReleaseCows();
        }

    }



    void ReleaseCows()
    {
        foreach (GameObject cow in cows)
        {
            cow.SetActive(true);
            cow.transform.GetChild(1).transform.position = targetPos;
        }
        cowsReleased = true;

    }




}
