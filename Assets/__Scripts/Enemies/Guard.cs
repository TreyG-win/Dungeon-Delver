using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using UnityEngine.SceneManagement;

public class Guard : Enemy, IFacingMover
{
    public enum eMode { idle, move, attack, transition, knockback }
    [Header("Set in Inspector")]
    public float speed = 5.0f;
    public float timeThinkMin = 1f;
    public float timeThinkMax = 4f;
    public float timeToStopAnim = 0.05f;

    [Header("Set Dynamically")]
    public int facing = 1;
    public float timeNextDecision = 0;
    public eMode mode = eMode.idle;

    private InRoom inRm;

    protected override void Awake()
    {
        base.Awake();
        inRm = GetComponent<InRoom>();

    }

    override protected void Update()
    {
        base.Update();
        if (knockback)
        {
            return;
        }
        if (Time.time >= timeNextDecision)
        {
            DecideDirection();

        }

        //If the enemy is moving slow enough, it will pause the animation
        if (rb.velocity.magnitude <= timeToStopAnim)
        {
            animator.speed = 0;
        } 
        rb.velocity = direction[facing] * speed;
    }

    //The time (between two constants) that it takes for the enemy to change directions
    void DecideDirection()
    {
        facing = Random.Range(0, 4);
        animator.speed = 1;
        timeNextDecision = Time.time + Random.Range(timeThinkMin, timeThinkMax);

        animator.CrossFade("Guard_Walk_" + facing, 0);

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

    protected override void Die()
    {
        base.Die(); // Calls the base class Die() method to handle item dropping

        // Additional logic specific to the Guard enemy's death
        SceneManager.LoadScene("WonScreen");
    }
}
