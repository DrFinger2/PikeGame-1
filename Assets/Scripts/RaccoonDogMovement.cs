using UnityEngine;
public class RaccoonDogMovement : MonoBehaviour
{
    Rigidbody rb;
    Animator anim;


    float speed = 3;
    Vector3 targetPos;
    Vector3 dir;
    Quaternion rot;
    Vector3 roteuler;

    float eatCooldown = 10;
    float hitCooldown = 1;

    bool eating = false;
    bool scared = false;

    public bool touched = false;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        rb = transform.GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        targetPos = new Vector3(-5.5f, transform.position.y, 3.5f);
        dir = (targetPos - transform.position).normalized;
        rb.linearVelocity = dir * speed;
        rot = Quaternion.FromToRotation(transform.forward, dir);
        roteuler = rot.eulerAngles;
        Debug.Log(roteuler);
        transform.rotation = rot;
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

        if (touched == true)
        {
            RaccoonTouched();
        }

    }


    void MoveTowardsCenter()
    {

        if((targetPos-transform.position).magnitude < 11)
        {
            rb.linearVelocity = Vector3.zero;
            anim.SetInteger("DogState", 1);
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
            //"Eat" a bird/egg = remove a point from the biodiversity score
            eatCooldown = 10;
        }
    }


    
    void RaccoonTouched()
    {
        Debug.Log("GOT HIS ASS");
        anim.SetInteger("DogState", 2);

        if (hitCooldown > 0 && !scared)
        {
            hitCooldown -= Time.deltaTime;
        }
        else if (!scared)
        {
            scared = true;
            transform.rotation = Quaternion.Euler(roteuler.x, roteuler.y+180, roteuler.z);
            Debug.Log(Quaternion.Euler(roteuler.x, roteuler.y + 180, roteuler.z));
            rb.linearVelocity = -dir * speed;
            hitCooldown = 7;
        }
        else if (scared)
        {
            hitCooldown -= Time.deltaTime;
            if(hitCooldown <= 0)
            {
                Destroy(gameObject);
            }
        }
            
    }


}
