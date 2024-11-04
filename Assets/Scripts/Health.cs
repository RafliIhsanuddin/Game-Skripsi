using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int health = 100;
    private int MAX_HEALTH = 100;

    // Start is called before the first frame update
    void Start()
    {
        // Inisialisasi kesehatan di sini jika diperlukan
    }

    // Update is called once per frame
    void Update()
    {
        // Pembaruan yang diperlukan di sini
    }

    public void Damage(int amount)
    {
        if (amount < 0)
        {
            throw new System.ArgumentOutOfRangeException("Cannot have negative damage");
        }

        // Mengurangi kesehatan
        this.health -= amount;
        Debug.Log(gameObject.name + " received damage: " + amount + ". Remaining health: " + health);

        // Memeriksa apakah kesehatan telah mencapai 0 atau kurang
        if (health <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount < 0)
        {
            throw new System.ArgumentOutOfRangeException("Cannot have negative healing");
        }

        // Menambah kesehatan tanpa melebihi batas maksimum
        this.health = Mathf.Min(health + amount, MAX_HEALTH);
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " died");
        Destroy(gameObject);
    }
}
