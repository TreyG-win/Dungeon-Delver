using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    public Image healthBar;
    public float healthAmount = 10f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (healthAmount <= 0)
        {
            SceneManager.LoadScene("LoseScreen");
        }
        

    }

    public void TakeDamage(float damage)
    {
        healthAmount -= damage;
        healthBar.fillAmount = healthAmount / 5f;
    }


    //not ready to use
    public void Heal(float healingAmount)
    {
        healthAmount += healingAmount;
        healthAmount = Mathf.Clamp(healthAmount, 0 , 10);

        healthBar.fillAmount = healthAmount / 10f;
    }
}
