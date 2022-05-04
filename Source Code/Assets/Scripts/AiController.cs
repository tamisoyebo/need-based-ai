using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Linq;

public class AiController : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform food;
    public Transform water;
    public Transform entertainment;
    public LayerMask isGround, isNeed;
    public float thirst;
    public float hunger;
    public float fun;

    //hunger, thirst, fun
    public List<float> needs = new List<float>(){100f, 100f, 100f};
    public List<float> needDecayRate =  new List<float>(){0.04f, 0.045f, 0.02f};
    List<float> initialNeedDecayRate =  new List<float>(){0.04f, 0.045f, 0.02f};

    
    //patrolling variables
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    bool patrolling = true;

    //states
    public float sightRange;
    public bool needInSightRange;

    //healthbar
    public HealthBar hungerBar;
    public HealthBar thirstBar;
    public HealthBar funBar;

    public List<GameObject> actionQueue;
    int actionNumber  = 0;
    public GameObject needIndicators;
    public GameObject button;
    public Slider decay;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        needs[0] = 100f;
        hungerBar.SetMaxHealth( needs[0] );
        thirstBar.SetMaxHealth( needs[1] );
        funBar.SetMaxHealth( needs[2] );
        needIndicators.transform.GetChild(0).gameObject.SetActive(false);
        needIndicators.transform.GetChild(1).gameObject.SetActive(false);
        needIndicators.transform.GetChild(2).gameObject.SetActive(false);
        button.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //decrease needs by small amount
        needs[0] -= (needDecayRate[0] + needDecayRate[0]*decay.value);
        needs[1] -= (needDecayRate[1] + needDecayRate[1]*decay.value);
        needs[2] -= (needDecayRate[2] + needDecayRate[2]*decay.value);

        hungerBar.SetHealth( needs[0] );
        thirstBar.SetHealth( needs[1] );
        funBar.SetHealth( needs[2] );

        //if needs are above 80, just patrol around
        foreach(var need in needs)
        {
            if (need > 80)
            {
                continue;
            }
        //if any need is less than 80, perform action based selection
            else
            {
                patrolling = false;
                break;
            }
        }

        //if patrolling or perform action selection
        if (patrolling)
        {
            Patrolling();
        }
        else
        {
            if (actionQueue.Count == 0)
            {
                ActionSelection();
            }
            patrolling = ActionCompletion();
        }

        //check if need is 0
        foreach(var need in needs)
        {
            if (need <= 0)
            {
                agent.speed = 0;
                needDecayRate[0] = 0;
                needDecayRate[1] = 0;
                needDecayRate[2] = 0;
                button.SetActive(true);
            }
        }

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

    }

    void Patrolling()
    {
        if(!walkPointSet)
        {
            SearchWalkPoint();
        }

        if(walkPointSet)
        {
            agent.SetDestination(walkPoint);

        }

        Vector3 walkPointDistance = transform.position - walkPoint;

        //waypoint reached
        if (walkPointDistance.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    void SearchWalkPoint()
    {
        // calculate a random point in range
        float randomZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
        float randomX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
        
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ );

        if(Physics.Raycast(walkPoint, -transform.up, 2F, isGround))
            {
                walkPointSet = true;
            }
    }

    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(this.transform.position, sightRange);
    }*/

    void ActionSelection()
    {
        //get list of game objects in sight range
        Collider[] needColliders = Physics.OverlapSphere(this.transform.position, sightRange);
        List<GameObject> needsInRange = new List<GameObject>(); 
        foreach (Collider need in needColliders)
        {
            if(need.gameObject.layer == 9)
            {
                needsInRange.Add(need.gameObject);
            }
            
        }


        //get advertisers from each
        float[] advertisers;
        advertisers = new float[needsInRange.Count];
        for(int i = 0; i < advertisers.Length; i++)
        {
            advertisers[i] = needsInRange[i].GetComponent<NeedScript>().advertisedNeed;
        }

        
        //rank advertisers (winner-takes-all method)
        Array.Sort(advertisers);
        Array.Reverse(advertisers);

        //set walkpoint to that need
        foreach(float advertiser in advertisers)
        {
            foreach(GameObject need in needsInRange)
            {
                if(need.GetComponent<NeedScript>().advertisedNeed == advertiser && !actionQueue.Contains(need))
                {
                    actionQueue.Add(need);
                }

            }
        }

        Array.Clear(needColliders, 0, needColliders.Length);
        needsInRange.Clear();
        Array.Clear(advertisers, 0, advertisers.Length);
    }
    
    bool ActionCompletion()
    {
        if (actionNumber >= actionQueue.Count)
        {
            actionQueue.Clear();
            actionNumber = 0;
            return patrolling;
        }
        else
        {
            patrolling = false;
        }
        
        agent.SetDestination(actionQueue[actionNumber].transform.position);
        
        int needNumber;
        switch(actionQueue[actionNumber].tag)
        {
            case "hunger":
                needNumber = 0;
                break;
            case "thirst":
                needNumber = 1;
                break;
            case "fun":
                needNumber = 2;
                break;
            default:
                needNumber = 0;
                break;
        }
        needIndicators.transform.GetChild(needNumber).gameObject.SetActive(true);
        
        if(Vector3.Distance(gameObject.transform.position, actionQueue[actionNumber].transform.position) < 3f)
        {
            actionNumber++;
            needIndicators.transform.GetChild(needNumber).gameObject.SetActive(false);
        }
        
        return patrolling;
    }

    public void Reset()
    {
        for( int i = 0; i < needs.Count; i ++)
        {
            needs[i] = 100f;
        }
        this.transform.position = new Vector3 (0,1,0);

        needDecayRate[0] = initialNeedDecayRate[0];
        needDecayRate[1] = initialNeedDecayRate[1];
        needDecayRate[2] = initialNeedDecayRate[2];
       
       
        agent.speed = 2.5f;
        patrolling = true;
        button.SetActive(false);

        decay.value = 0;
    }



}
