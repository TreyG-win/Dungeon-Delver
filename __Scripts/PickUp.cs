using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public enum eType { key, health, grappler }
    public static float COLLIDER_DELAY = 0.5f;

    [Header("Set in Inspector")]
    public eType itemType;
    public float healingAmount = 2f;

    private HealthManager healthManager;

    //Awake() and Activate() disable the PickUp's Collider for 0.5 seconds, can be adjust with
    //COLLIDER_DELAY float

    void Start()
    {
        healthManager = FindHealthManager();
    }

    void Awake()
    {
        GetComponent<Collider>().enabled = false;
        Invoke("Activate", COLLIDER_DELAY);
    }
    void Activate()
    {
        GetComponent<Collider>().enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Check if the colliding object is the player
        {
            if (itemType == eType.health && healthManager != null)
            {
                healthManager.Heal(healingAmount); // Call the Heal method in HealthManager
                Destroy(gameObject); // Destroy the health item after picking it up
            }
        }
    }

    HealthManager FindHealthManager()
    {
        HealthManager[] healthManagers = FindObjectsOfType<HealthManager>();
        if (healthManagers.Length > 0)
        {
            return healthManagers[0];
        }
        else
        {
            Debug.LogError("HealthManager not found in the scene.");
            return null;
        }
    }
}
