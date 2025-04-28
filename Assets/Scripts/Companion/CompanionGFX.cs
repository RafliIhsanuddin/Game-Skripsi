using UnityEngine;
using Pathfinding;

public class CompanionGFX : MonoBehaviour
{

    public AIPath aiPath;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (aiPath.desiredVelocity.x >= 0.01f)  
        {
            transform.localScale = new Vector3(4.6012f, 4.6012f, 4.6012f);
        } else if (aiPath.desiredVelocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(-4.6012f, 4.6012f, 4.6012f);
        }
        Debug.Log(aiPath.desiredVelocity.x);


        /*if (rb.linearVelocity.x >= 0.01f)
        {
            transform.localScale = new Vector3(4.6012f, 4.6012f, 4.6012f);
        }
        else if (rb.linearVelocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(-4.6012f, 4.6012f, 4.6012f);
        }
        Debug.Log(rb.linearVelocity.x);*/
    }
}
