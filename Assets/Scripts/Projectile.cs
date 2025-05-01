using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject impactFX;
    public bool doesExplode;
    public float damage;
    public float speed;
    GameObject go;
    Quaternion q;
    Vector3 v3;
    bool hasHit = false;
    Rigidbody2D rb;
    Vector3 lastPos;
    public List<GameObject> targets;
    Dictionary<float, GameObject> distDic = new Dictionary<float, GameObject>();
    GameObject nearestTarget;
    Vector2 distance;

    void Awake()
    {
        go = GetComponent<GameObject>();
        rb = this.GetComponent<Rigidbody2D>();
        rb.AddForce(this.transform.forward * rb.mass / Time.fixedDeltaTime);
        // looks for the nearest enemy 
        #region Nearest Enemy
        // adds all gameobjects with the tag Player to the targets list

        targets.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        // remove duplicate objects from list
        targets = targets.Distinct().ToList();
        // remove emptty spots in list
        targets = targets.Where(target => target != null).ToList();

        foreach (GameObject obj in targets)
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            // if any of the players have died or are no more active simply don't add them to the lst ever again.
            if (obj.activeInHierarchy == false) targets.Remove(obj);
            // if they are active in the scene add them to the dictionary
            if (obj.activeInHierarchy == true) distDic.Add(dist, obj);
            //else don't add it to the list
            else return;

        }
        // we need a list that keeps all the distances so we can select the shortest gameobject to follow
        List<float> distances = distDic.Keys.ToList();
        // sort from nearest to furthest
        distances.Sort();
        // the target we follow is the target with the shortest distance
        nearestTarget = distDic[distances[0]];
        // we calculate the distance from this position to the nearest targets position
        distance = (transform.position - nearestTarget.transform.position);
        lastPos = new Vector2(nearestTarget.transform.position.x, nearestTarget.transform.position.y);
        var newRotation = Quaternion.LookRotation(transform.position - nearestTarget.transform.position, Vector3.forward);
        #endregion
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Ground")
        {
            rb.isKinematic = true;
            hasHit = true;
            Explode();
        }
        if (col.gameObject.tag == "Player")
        {
            rb.isKinematic = true;
            hasHit = true;
            col.gameObject.GetComponent<Player>().Health -= damage;
            Explode();
        }
        if (col.gameObject.tag == "Wall")
        {
            rb.isKinematic = true;
            hasHit = true;
            Explode();
        }
    }
    void Update()
    {
        // freese projectile
        if (hasHit)
        {
            //this.GetComponent<Rigidbody2D> ().bodyType = RigidbodyType2D.Static;
            transform.position = v3;
            transform.rotation = q;
        }
        else
        {
            // rotate the projectile depending on its velocity
            v3 = transform.position;
            q = transform.rotation;
            Vector2 v = rb.linearVelocity;
            float angle;
            angle = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;
        }
        // look at target
        var newRotation = Quaternion.LookRotation(transform.position - nearestTarget.transform.position, Vector3.forward);
        newRotation.x = 0.0f;
        newRotation.y = 0.0f;
        // if the transform reaches the place where the player was last we explode.
        if (transform.position.x == lastPos.x && transform.position.y == lastPos.y)
        {
            Explode();
            hasHit = true;
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, lastPos, speed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 8);
        }
    }
    public void Explode()
    {
        Instantiate(impactFX, transform.position, transform.rotation);
        if (doesExplode)
            Destroy(gameObject);
    }
}