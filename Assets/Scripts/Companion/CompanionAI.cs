using UnityEngine;
using Pathfinding;
using UnityEditor.Search;

public class CompanionAI : MonoBehaviour
{

    public Transform target;

    public float speed = 200f;
    public float nextWaypointDistance = 3f;

    public Transform enemyGFX;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndofPath = false;

    Seeker seeker;
    Rigidbody2D rb;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0f, 0.2f);

    }

    void UpdatePath()
    {
        if (seeker.IsDone())
        seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (path == null) {
            return;
        }


        if (currentWaypoint >= path.vectorPath.Count) 
        {
            reachedEndofPath = true;
            return;
        } 
        else
        {
            reachedEndofPath = false;   
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force); 

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if(distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (rb.linearVelocity.x >= 0.01f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (rb.linearVelocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(-1, 1f, 1f);
        }
        Debug.Log(rb.linearVelocity.x);





    }
}
