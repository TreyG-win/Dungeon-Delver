using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    public enum eMode { none, gOut, gInMiss, gInHit }

    [Header("Set in Inspector")]
    public float grappleSpd = 10;
    public float grappleLength = 7;
    public float grappleInLength = 0.5f;
    public int unsafeTileHealthPenalty = 2;
    public TextAsset mapGrappleable;

    [Header("Set Dynamically")]
    public eMode mode = eMode.none;

    //Determines the TileNums that can be grappled onto
    public List<int> grappleTiles;
    public List<int> unsafeTiles;

    private Dray dray;
    private Rigidbody rb;
    private Animator anim;
    private Collider drayCollid;
    private GameObject grapHead;
    private LineRenderer grapLine;

    //p0 is the starting point, p1 is where the grappler is connecting
    private Vector3 p0, p1;
    private int facing;

    private Vector3[] directions = new Vector3[]
    {
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };
    void Awake()
    {
        string gTiles = mapGrappleable.text;
        gTiles = Utils.RemoveLineEndings(gTiles);
        grappleTiles = new List<int>();
        unsafeTiles = new List<int>();
        for (int i = 0; i < gTiles.Length; i++)
        {
            switch (gTiles[i])
            {
                case 'S':
                    grappleTiles.Add(i);
                    break;
                case 'X':
                    unsafeTiles.Add(i);
                    break;
            }
        }
        dray = GetComponent<Dray>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        drayCollid = GetComponent<Collider>();

        //This transform will search for an object with the name "Grappler"
        Transform trans = transform.Find("Grappler");
        //The head of the grappler is set to the transform.gameObject
        grapHead = trans.gameObject;
        //Generates the line behind the head of the grappler with a line renderer
        grapLine = grapHead.GetComponent<LineRenderer>();
        //Is currently set to be inactive so that it does not appear in the scene, unless instructed to
        grapHead.SetActive(false);
    }

    void Update()
    {
        if(!dray.hasGrappler) { return; }
        
        switch (mode)
        {
            //If the grappler's mode is currently not doing anything
            //then the player can use the grappler
            case eMode.none:
                //For now, this is the key to use the grappler (E)
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartGrapple();
                }
                break;
        }
        
    }

    void StartGrapple()
    {
        //Grabs the direction that Dray is facing
        facing = dray.GetFacing();
        //Removes the ability to control Dray
        dray.enabled = false;
        //Allows for a more seamless transition from attacking
        anim.CrossFade("Dray_Attack_" + facing, 0);
        //Removes Dray's collider, making him able to clip through anything in the path
        drayCollid.enabled = false;
        rb.velocity = Vector3.zero;

        //Makes the grappler's head active and visible
        grapHead.SetActive(true);

        p0 = transform.position + (directions[facing] * 0.5f);
        p1 = p0;
        grapHead.transform.position = p1;
        grapHead.transform.rotation = Quaternion.Euler(0, 0, 90 * facing);

        grapLine.positionCount = 2;
        grapLine.SetPosition(0, p0);
        grapLine.SetPosition(1, p1 );
        mode = eMode.gOut;
    }

    void FixedUpdate()
    {
        switch (mode)
        {
            case eMode.gOut:
                p1 += directions[facing] * grappleSpd * Time.fixedDeltaTime;
                grapHead.transform.position = p1;
                grapLine.SetPosition(1, p1);

                //A check to see if the grapple hit any objects
                int tileNum = TileCamera.GET_MAP(p1.x, p1.y);
                if (grappleTiles.IndexOf(tileNum) != -1)
                {
                    //Successfully connected to a grappleable tile
                    mode = eMode.gInHit;
                    break;
                }
                //If the length of the grapple has been reached and nothing has
                //connected, then it will be a miss
                if((p1-p0).magnitude >= grappleLength)
                {
                    mode = eMode.gInMiss;
                }
                break;

            case eMode.gInMiss:
                //If the grappler misses, then it will return at double the speed
                p1 -= directions[facing] * 2 * grappleSpd * Time.fixedDeltaTime;
                if(Vector3.Dot((p1-p0), directions[facing]) > 0)
                {
                    //The grapple is still in front of Dray
                    grapHead.transform.position = p1;
                    grapLine.SetPosition(1, p1);
                } else { 
                    StopGrapple();
                }
                break;

            case eMode.gInHit:
                float dist = grappleInLength + grappleSpd * Time.fixedDeltaTime;
                if (dist > (p1 - p0).magnitude)
                {
                    p0 = p1 - (directions[facing] * grappleInLength);
                    transform.position = p0;
                    StopGrapple();
                    break;
                }
                p0 += directions[facing] * grappleSpd * Time.fixedDeltaTime;
                transform.position = p0;
                grapLine.SetPosition(0, p0);
                grapHead.transform.position = p1;
                break;
        }        
    }

    void StopGrapple()
    {
        dray.enabled = true;
        drayCollid.enabled = true;

        //Checks to see if the tile is unsafe
        int tileNum = TileCamera.GET_MAP(p0.x, p0.y);
        if (mode == eMode.gInHit && unsafeTiles.IndexOf(tileNum) != -1) {

            dray.ResetInRoom(unsafeTileHealthPenalty);
        }

        grapHead.SetActive(false);
        mode = eMode.none;
    }

    void OnTriggerEnter2D (Collider2D coll)
    {
        Enemy e = coll.GetComponent<Enemy>();
        if (e == null) { return; }
        mode = eMode.gInMiss;
    }
}
