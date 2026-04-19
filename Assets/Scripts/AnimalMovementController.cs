using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class AnimalMovementController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float minIdle = 2f;
    [SerializeField] private float maxIdle = 6f;
    [SerializeField] private float minWalk = 1f;
    [SerializeField] private float maxWalk = 8f;

    [SerializeField] private Transform boundary; //scale is size, pos position

    [SerializeField] private string walkAnimation = "Walk";
    [SerializeField] private string idleAnimation = "Idle";

    [SerializeField] private bool showBounds = false;

    private enum State
    {
        Idle,
        Walking
    }
    private State currentState = State.Idle;
    private Vector3 targetPos;
    private Animator anim;
    private float stateTime;

    private void Start()
    {
        anim = GetComponent<Animator>();
        EnterIdle();
    }
    private void Update()
    {
        stateTime -= Time.deltaTime;
        switch(currentState)
        {
            case State.Idle:
                if(stateTime <= 0)
                {
                    EnterWalk();
                }
                break;
            case State.Walking:
                if(stateTime <= 0 || ReachedTargetPos())
                {
                    EnterIdle();
                }
                else
                {
                    MoveToTarget();
                }
                break;
        }
    }
    private void EnterIdle()
    {
        currentState = State.Idle;
        stateTime = Random.Range(minIdle, maxIdle);
        if (gameObject.name != "smallFish")
        {
            anim.SetTrigger(idleAnimation);
        }
            
    }
    private void EnterWalk()
    {
        currentState = State.Walking;
        stateTime = Random.Range(minWalk, maxWalk);
        targetPos = GetRandomPointInBoundary();

        if(gameObject.name != "smallFish")
        {
            anim.SetTrigger(walkAnimation);
        }
        
    }
    private Vector3 GetRandomPointInBoundary()
    {
        Vector3 boundarySize = boundary.localScale;
        Vector3 randomOffset = new Vector3(Random.Range(-boundarySize.x/2,boundarySize.x/2), Random.Range(-boundarySize.y/2,boundarySize.y/2),Random.Range(-boundarySize.z/2,boundarySize.z/2));
        return boundary.position + randomOffset;
    }
    private void MoveToTarget()
    {
        Vector3 dir = targetPos - transform.position;
        if(Mathf.Approximately(boundary.localScale.y,0))
        {
            dir.y = 0;
        }
        if(dir.magnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }
    private bool ReachedTargetPos()
    {
        Vector3 currentPos = transform.position;
        Vector3 target = targetPos;
        if(Mathf.Approximately(boundary.localScale.y,0))
        {
            target.y = 0;
            currentPos.y = 0;
        }
        return Vector3.Distance(currentPos, target) < 0.2f;
    }
    private void OnDrawGizmos()
    {
        if(showBounds)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boundary.position, boundary.localScale);
        }
    }
}
