using UnityEngine;

public class RaccoonDogMovement : MonoBehaviour
{
    Rigidbody rb;
    float speed = 3;
    Vector3 targetPos;
    Vector3 dir;

    float eatCooldown = 10; 

    bool eating = false;

    bool touched = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        targetPos = new Vector3(-5.5f, transform.position.y, 3.5f);
        dir = (targetPos - transform.position).normalized;
        rb.linearVelocity = dir * speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!eating && !touched)
        {
            MoveTowardsCenter();
        }

        if (eating && !touched)
        {
            EatPlants();
        }

    }


    void MoveTowardsCenter()
    {
        Debug.Log((targetPos - transform.position).magnitude);

        if((targetPos-transform.position).magnitude < 11)
        {
            rb.linearVelocity = Vector3.zero;
            eating = true;
        }
        
    }




    void EatPlants()
    {
        if(eatCooldown > 0)
        {
            eatCooldown -= Time.deltaTime;
        }
        else
        {
            //"Eat" a plant = remove a point from the biodiversity score
            eatCooldown = 10;
        }
    }


    /*
    void RaccoonTouched()
    {
        if(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
            RaycastHit hit;

            if(Physics.Raycast (ray, out hit))
            {
                if(hit.collider != null)
                {
                    Debug.Log("GOT HIS ASS");
                    touched = true;
                    rb.linearVelocity = -dir * speed;
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    Debug.Log("GOT HIS ASS");
                    touched = true;
                    rb.linearVelocity = -dir * speed;
                }
            }
        }

    }*/


}
