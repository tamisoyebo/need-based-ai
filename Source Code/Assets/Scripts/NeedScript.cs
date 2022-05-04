using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedScript : MonoBehaviour
{
    public float needValue;
    public float advertisedNeed;
    public GameObject player;
    AiController playerScript;
    public float satifyRange = 3f;
    bool needSatisfied = false;

    // Start is called before the first frame update
    
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<AiController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        advertisedNeed = CalculateNeedSatisfaction(playerScript.needs);
        if (Vector3.Distance(this.transform.position, player.transform.position) <= satifyRange && !needSatisfied)
        {
            SatisfyNeed(playerScript.needs);
        }

        if(Vector3.Distance(this.transform.position, player.transform.position) > satifyRange)
        {
            needSatisfied = false;
        }

    }

    float CalculateNeedSatisfaction(List<float> needLevels)
    {
        float currentNeedValue;
        switch (gameObject.tag)
        {
            case "hunger":
               currentNeedValue = needLevels[0];
               break;
            case "thirst":
                currentNeedValue = needLevels[1];
                break;
            case "fun":
                currentNeedValue = needLevels[2];
                break;
            default:
                currentNeedValue = 100;
                break;

        }
        
        return Attenuate(currentNeedValue) - Attenuate(currentNeedValue + needValue);

    }

    float Attenuate(float need)
    {
        return (10/need);
    }

    void SatisfyNeed(List<float> needLevels)
    {
        switch (gameObject.tag)
        {
        case "hunger":
            needLevels[0] += needValue;
            if (needLevels[0] > 100f)
            {
                needLevels[0] = 99f;
            }
            break;
        case "thirst":
            needLevels[1] += needValue;
            if (needLevels[1] > 100f)
            {
                needLevels[1] = 99f;
            }
            break;
        case "fun":
            needLevels[2] += needValue;
            if (needLevels[2] > 100f)
            {
                needLevels[2] = 99f;
            }
            break;
        default:
            break;
        }

        needSatisfied = true;

    }
}
