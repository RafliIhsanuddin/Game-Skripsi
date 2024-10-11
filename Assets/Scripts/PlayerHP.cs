using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public int maxHP;
    public TMP_Text hpText;
    [SerializeField] private int currentHP;

    // Start is called before the first frame update
    void Start()
    {
        currentHP = maxHP;
    }

    void Update()
    {
        hpText.text = "HP : " + currentHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject, 0.5f);
    }
}
