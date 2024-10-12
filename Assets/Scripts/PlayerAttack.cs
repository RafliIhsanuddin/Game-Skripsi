using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    [SerializeField] GameObject AttackArea;
    private bool attacking = false;
    private float timeToAttack = 0.05f;
    private float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        AttackArea.SetActive(false); // Pastikan AttackArea tidak aktif saat awal
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Attack();
        }

        if (attacking)
        {
            timer += Time.deltaTime;

            if (timer >= timeToAttack)
            {
                timer = 0f;
                attacking = false;
                AttackArea.SetActive(false); // Nonaktifkan AttackArea setelah serangan
            }
        }
    }

    private void Attack()
    {
        attacking = true;
        AttackArea.SetActive(true); // Aktifkan AttackArea saat menyerang

        // Menambahkan debug log untuk menyerang
        //Debug.Log("Player is attacking!");
    }
}
