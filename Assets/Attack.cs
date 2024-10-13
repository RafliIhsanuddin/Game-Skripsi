using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAttack : MonoBehaviour
{
    public Animator animator; // Komponen animator untuk memicu animasi serangan
    public float comboDelay = 0/5f; // Waktu untuk reset combo jika tidak ada input serangan
    private int comboStep = 0; // Tahap combo, mulai dari 0
    private float lastAttackTime; // Waktu serangan terakhir
    private bool isAttacking = false; // Untuk mengecek apakah player sedang menyerang

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) // Jika mouse kiri ditekan dan player tidak sedang menyerang
        {
            if (Time.time - lastAttackTime > comboDelay)
            {
                // Reset combo jika melebihi batas waktu
                comboStep = 0;
            }

            // Panggil method untuk menyerang
            Attack();
        }

        // Reset combo jika waktu serangan terakhir melebihi comboDelay
        if (Time.time - lastAttackTime > comboDelay && comboStep > 0)
        {
            comboStep = 0;
            animator.SetInteger("attack", comboStep);  // Mengatur kembali animasi
        }
    }

    void Attack()
    {

        // Set waktu serangan terakhir
        lastAttackTime = Time.time;

        // Meningkatkan comboStep hingga maksimal 3
        comboStep++;

        if (comboStep > 3)
        {
            comboStep = 1;  // Kembali ke serangan pertama
        }

        // Memainkan animasi serangan berdasarkan comboStep
        animator.SetInteger("attack", comboStep);

        // Contoh serangan berdasarkan step combo (bisa menambah damage atau efek berbeda)
        if (comboStep == 1)
        {
            // Lakukan serangan pertama
            Debug.Log("Serangan Pertama!");
        }
        else if (comboStep == 2)
        {
            // Lakukan serangan kedua
            Debug.Log("Serangan Kedua!");
        }
        else if (comboStep == 3)
        {
            // Lakukan serangan ketiga
            Debug.Log("Serangan Ketiga!");
        }
/*

        if (comboStep == 1)
        {
            animator.SetInteger("attack", 1);
        }
        else if (comboStep == 2)
        {
            animator.SetInteger("attack", 2);
        }
        else if (comboStep == 3)
        {
            animator.SetInteger("attack", 3);
        }

        // Menjalankan Coroutine untuk memberi jeda sebelum combo selesai
        StartCoroutine(ComboCooldown());*/
    }

    IEnumerator ComboCooldown()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.5f); // Menunggu hingga animasi selesai (sesuaikan dengan durasi animasi)
        isAttacking = false;

        // Reset combo setelah 3 kali serangan
        if (comboStep >= 3)
        {
            ResetCombo();
        }
    }

    void ResetCombo()
    {
        comboStep = 0; // Reset langkah combo
        animator.SetInteger("attack", 0);
        isAttacking = false; // Mengizinkan serangan lagi
    }
}

