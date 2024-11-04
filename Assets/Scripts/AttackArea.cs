using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    private int damage = 3;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object has a Health component
        if (collision.GetComponent<Health>() != null)
        {
            // Get the Health component
            Health health = collision.GetComponent<Health>();
            // Call the Damage method with the damage amount
            health.Damage(damage);
        }
    }



}
