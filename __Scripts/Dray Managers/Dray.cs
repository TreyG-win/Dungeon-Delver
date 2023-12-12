using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dray : MonoBehaviour, IFacingMover, IkeyMaster
{
    public HealthManager healthManager;
    public InventoryUI inventoryUI;
    public enum eMode { idle, move, attack, transition, knockback }

    [Header("Set in Inspector")]
    public float speed = 5.0f;
    //Sets how long the attack will last for
    public float attackDuration = 0.25f;
    //Sets a delay between attacks
    public float attackDelay = 0.5f;
    //Sets a delay for transitioning rooms
    public float transitionDelay = 0.5f;
    //Sets the maximum health
    public int maxHealth = 100;
    //How fast Dray will travel after being hit
    public float knockbackSpeed = 10;
    //How long the knockback effect will last for
    public float knockbackDuration = 0.25f;
    //Determines how long Dray will have "I-frames" for after taking damage
    public float invincibleDuration = 0.5f;

    [Header("Set Dynamically")]

    public int dirHeld = -1;
    public int facing = 1;
    public eMode mode = eMode.idle;
    public int numKeys = 0;
    public bool invincible = false;
    public bool hasGrappler = false;
    public Vector3 lastSafeLoc;
    public int lastSafeFacing;

    [SerializeField]
    private int _health;

    public int health
    {
        get { return _health; }
        set { _health = value; }
    }

    private float timeAtkDone = 0;
    private float timeAtkNext = 0;
    private float transitionDone = 0;
    private Vector2 transitionPos;
    private float knockbackDone = 0;
    private float invincibleDone = 0;
    private Vector3 knockbackVel;

    private SpriteRenderer sRend;
    private Rigidbody rb;
    private Animator anim;
    private InRoom inRm;

    //Controls the direction that Dray moves in
    private Vector3[] direction = new Vector3[]
    {
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    //Contains the keys to allow the player to move
    private KeyCode[] keys = new KeyCode[]
    {
        KeyCode.D, KeyCode.W, KeyCode.A, KeyCode.S
    };

    void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        inRm = GetComponent<InRoom>();
        health = maxHealth;
        lastSafeLoc = transform.position;
        lastSafeFacing = facing;
    }

    void Update()
    {
        //If Dray is invincible and the time is greater than the invincibleDone,
        //then invincibility will turn off
        if (invincible && Time.time > invincibleDone) { invincible = false; }
        //What color Dray's sprite will be when invincible
        sRend.color = invincible ? Color.red : Color.white;
        //Works with the knockback effect and duration
        if(mode == eMode.knockback)
        {
            rb.velocity = knockbackVel;
            if(Time.time < knockbackDone)
            {
                return;
            }
        }

        if (mode == eMode.transition)
        {
            rb.velocity = Vector3.zero;
            anim.speed = 0;
            //Keeps Dray locked in place
            roomPos = transitionPos;
            if (Time.time < transitionDone)
            {
                return;
            }
            //This line will only happen if Time.time exceeds transitionDone
            mode = eMode.idle;
        }

        dirHeld = -1;

        //Depending on which key is held, the program will read that input
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKey(keys[i])) dirHeld = i;
        }

        //Binding the attack button
        if (Input.GetMouseButtonDown(0) && Time.time >= timeAtkNext)
        {
            mode = eMode.attack;
            timeAtkDone = Time.time + attackDuration;
            timeAtkNext = Time.time + attackDelay;
            AudioManager.Instance.PlaySFX("SwordHit");
        }

        //Finishing the attack when it is finished
        if (Time.time >= timeAtkDone)
        {
            mode = eMode.idle;
        }

        //Chooses the proper mode if Dray is not currently attacking
        if (mode != eMode.attack)
        {
            if (dirHeld == -1)
            {
                mode = eMode.idle;
            }
            else
            {
                facing = dirHeld;
                mode = eMode.move;
            }
        }

        Vector3 vel = Vector3.zero;
        switch (mode)
        {

            case eMode.attack:
                anim.CrossFade("Dray_Attack_" + facing, 0);
                anim.speed = 0;
                break;
            //If no movement keys are held, then the animation will be frozen
            case eMode.idle:
                anim.CrossFade("Dray_Walk_" + facing, 0);
                anim.speed = 0;
                break;

            case eMode.move:
                vel = direction[dirHeld];
                anim.CrossFade("Dray_Walk_" + facing, 0);
                anim.speed = 1;
                break;
        }

        rb.velocity = vel * speed;

    }
    void LateUpdate()
    {
        //Grabs the half-grid location of this GameObject
        Vector2 rPos = GetRoomPosOnGrid(0.5f);

        //Checks to see if Dray is in a Door tile
        int doorNum;
        for (doorNum = 0; doorNum < 4; doorNum++) {
            if(rPos == InRoom.DOORS[doorNum])
            {
                break;
            }
        }
        if(doorNum > 3 || doorNum != facing)
        {
            return;
        }
        ChangeRoom(doorNum);
        
    }

    //Starts the process of transitioning to a new room 
    public void ChangeRoom(int doorNum)
    {
        Vector2 rm = roomNum;
        switch(doorNum)
        {
            case 0:
                rm.x += 1;
                break;
            case 1:
                rm.y += 1;
                break;
            case 2:
                rm.x -= 1;
                break;
            case 3:
                rm.y -= 1;
                break;
        }

        if(rm.x >= 0 && rm.x <= InRoom.MAX_RM_X)
        {
            if(rm.y >= 0 && rm.y <= InRoom.MAX_RM_Y)
            {
                roomNum = rm;
                transitionPos = InRoom.DOORS[(doorNum+2) % 4];
                roomPos = transitionPos;
                lastSafeLoc = transform.position;
                lastSafeFacing = facing;
                mode = eMode.transition;
                transitionDone = Time.time + transitionDelay;
            }
        }

    }

    void OnCollisionEnter(Collision coll)
    {
        //If Dray is currently invincible, then Dray cannot be harmed
        if (invincible)
        {
            return;
        }

        DamageEffect dEf = coll.gameObject.GetComponent<DamageEffect>();
        //A check in case there is no DamageEffect
        if(dEf == null) {
            return;
        }
        //Subtracts the amount of health from the set damage value is
        health -= dEf.damage;
        //Dray will become invincible upon being damaged
        invincible = true;
        invincibleDone = Time.time + invincibleDuration;

        //Apply damge to Dray
        healthManager.TakeDamage(dEf.damage);

        knockbackDray(dEf, coll);
    }
    //Handles the knockback effects for taking damage
    public void knockbackDray(DamageEffect dEf, Collision coll)
    {
        if (dEf.knockback)
        {
            //This debug log is for testing purposes until the health GUI is implemented
            Debug.Log($"Ouch! Current HP: {health}");

            

            Vector2 delta = transform.position - coll.transform.position;
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
            mode = eMode.knockback;
            knockbackDone = Time.time + knockbackDuration;
            
            
        }

    }
    //Allows Dray to pick up items
    void OnTriggerEnter(Collider collid)
    {
        PickUp pick = collid.GetComponent<PickUp>();
        if (pick == null) { return; }

        switch (pick.itemType)
        {
            case PickUp.eType.health:
                health = Mathf.Min(health + 2, maxHealth);
                break;

            case PickUp.eType.key:
                keyCount++;
                
                inventoryUI.UpdateKeyCount(keyCount);

                break;
            case PickUp.eType.grappler:
                
                hasGrappler = true;
                break;
        }
        Destroy(collid.gameObject);
    }

    //If Dray falls into the lava, then it will place him back
    //to the last safe spot in the room
    public void ResetInRoom(int healthLoss)
    {
        transform.position = lastSafeLoc;
        facing = lastSafeFacing;
        healthManager.TakeDamage(healthLoss);

        //Makes Dray invincible
        invincible = true;
        invincibleDone = Time.time + invincibleDuration;
    }
    //Implementation of IKeyMaster, if you guys are looking for it.
    // TY, Done 'ThumbsUp' -Jack
    public int keyCount
    {
        get { return numKeys; }
        set { numKeys = value; }
    }

    //The implementation of IFacingMover
    public int GetFacing()
    {
        return facing;
    }

    public bool moving
    {
        get
        {
            return (mode == eMode.move);
        }
    }

    public float GetSpeed()
    {
        return speed;
    }

    public float gridMult
    {
        get { return inRm.gridMult; }
    }

    public Vector2 roomPos
    {
        get { return inRm.roomPos; }
        set { inRm.roomPos = value; }
    }

    public Vector2 roomNum
    {
        get { return inRm.roomNum; }
        set { inRm.roomNum = value; }
    }

    public Vector2 GetRoomPosOnGrid(float mult = -1)
    {
        return inRm.GetRoomPosOnGrid(mult);
    }
}
