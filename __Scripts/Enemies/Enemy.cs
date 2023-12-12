using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Dray;

public class Enemy : MonoBehaviour
{
    /*
     * This class will be the base class for all
     * enemies going forward by using inheritance.
     */
    protected static Vector3[] direction = new Vector3[] {
        Vector3.right, Vector3.up, Vector3.left, Vector3.down };

    [Header("Set in Inspector: Enemy")]

    public float maxHealth = 1.0f;
    public float knockbackSpeed = 10;
    public float knockbackDuration = 0.25f;
    public float invincibleDuration = 0.5f;
    //Determines the list of random items to drop, right now it is set to drop a health
    //item 1/3 of the time. This is based on how large the list of items are, including null.
    public GameObject[] randomItemDrops;
    //Determines if a drop should always come from the enemies
    public GameObject guaranteedItemDrop = null;

    [Header("Set Dynamically: Enemy")]
    public float health;
    public bool invincible = false;
    public bool knockback = false;

    private float invincibleDone = 0;
    private float knockbackDone = 0;
    private Vector3 knockbackVel;

    protected Animator animator;

    protected Rigidbody rb;

    protected SpriteRenderer sRend;

    protected virtual void Awake()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        sRend = GetComponent<SpriteRenderer>();
    }

    protected virtual void Update()
    {
        //If the enemy is invincible and the time is greater than the invincibleDone,
        //then invincibility will turn off
        if (invincible && Time.time > invincibleDone) { invincible = false; }
        //What color the enemy's sprite will be when invincible
        sRend.color = invincible ? Color.blue : Color.white;
        //Works with the knockback effect and duration
        if (knockback)
        {
            rb.velocity = knockbackVel;
            if (Time.time < knockbackDone)
            {
                return;
            }
            animator.speed = 1;
            knockback = false;
        }
    }

    void OnTriggerEnter(Collider coll)
    {
        //If the enemy is currently invincible, then it cannot be harmed
        if (invincible)
        {
            return;
        }

        DamageEffect dEf = coll.gameObject.GetComponent<DamageEffect>();
        //A check in case there is no DamageEffect
        if (dEf == null)
        {
            return;
        }
        //Subtracts the amount of health from the set damage value is
        health -= dEf.damage;
        //If the health runs out, then the enemy dies
        if (health < 0)
        {
            Die();
        }
        //The enemy will become invincible upon being damaged
        invincible = true;
        invincibleDone = Time.time + invincibleDuration;



        knockbackDray(dEf, coll);
    }
    //Handles the knockback effects for taking damage
    public void knockbackDray(DamageEffect dEf, Collider coll)
    {
        if (dEf.knockback)
        {
            Vector2 delta = transform.position - coll.transform.root.position;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                //Checks to see if the knockback should be horizontal
                delta.x = (delta.x > 0) ? 1 : -1;
                delta.y = 0;
            }
            else
            {
                //Checks to see if the knockback should be vertical
                delta.x = 0;
                delta.y = (delta.y > 0) ? 1 : -1;
            }
            //Applies the knockback speed to the Rigidbody
            knockbackVel = delta * knockbackSpeed;
            rb.velocity = knockbackVel;

            //Sets the mode to knockback and sets a time to stop the knockback effect
            knockback = true;
            knockbackDone = Time.time + knockbackDuration;
            animator.speed = 0;
        }

    }
    
    

    protected virtual void Die()
    {
        DropItems(); 
    }
    //Destroys the game object and spawns an item if the guaranteedItemDrop isn't null
    protected void DropItems()
    {
        GameObject go;
        if (guaranteedItemDrop != null)
        {
            go = Instantiate<GameObject>(guaranteedItemDrop);
            go.transform.position = transform.position;
        }
        else if (randomItemDrops.Length > 0)
        {
            int n = Random.Range(0, randomItemDrops.Length);
            GameObject prefab = randomItemDrops[n];
            if (prefab != null)
            {
                go = Instantiate<GameObject>(prefab);
                go.transform.position = transform.position;
            }
        }
        Destroy(gameObject);
    }
}
