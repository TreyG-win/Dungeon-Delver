using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRoom : MonoBehaviour
{
    static public float ROOM_W = 16;
    static public float ROOM_H = 11;
    static public float WALL_T = 2;

    //These MAX values will determine the maximum boundary of the map
    //Should we want a larger map, this will need to be changed to accomadate
    static public int MAX_RM_X = 9;
    static public int MAX_RM_Y = 9;

    //An array of room relative positions of each possible door
    static public Vector2[] DOORS = new Vector2[]
    {
        new Vector2(14, 5),
        new Vector2(7.5f, 9),
        new Vector2(1, 5),
        new Vector2(7.5f, 1)
    };

    [Header("Set in Inspector")]
    //Prevents the character from leaving the room when set to true
    public bool keepInRoom = true;

    //Changes the grid multiplier, this will affect how the character is aligned to a tile
    public float gridMult = 1;

    private void LateUpdate()
    {
        if (keepInRoom)
        {
            Vector2 rPos = roomPos;
            rPos.x = Mathf.Clamp(rPos.x, WALL_T, ROOM_W - 1 - WALL_T);
            rPos.y = Mathf.Clamp(rPos.y, WALL_T, ROOM_H - 1 - WALL_T);
            roomPos = rPos;
        }
    }

    //Where in the room, locally, the character is.
    public Vector2 roomPos
    {
        get
        {
            Vector2 tPos = transform.position;
            tPos.x %= ROOM_W;
            tPos.y %= ROOM_H;
            return tPos;
        }

        set
        {
            Vector2 rm = roomNum;
            rm.x *= ROOM_W;
            rm.y *= ROOM_H;
            rm += value;
            transform.position = rm;
        }
    }

    //Determines which room this character is in
    public Vector2 roomNum
    {
        get
        {
            Vector2 tPos = transform.position;
            tPos.x = Mathf.Floor(tPos.x / ROOM_W);
            tPos.y = Mathf.Floor(tPos.y / ROOM_H);
            return tPos;
        }

        set
        {
            Vector2 rPos = roomPos;
            Vector2 rm = value;
            rm.x *= ROOM_W;
            rm.y *= ROOM_H;
            transform.position = rm + rPos;
        }
    }

    //Determines the closest grid location to the character
    public Vector2 GetRoomPosOnGrid(float mult = -1)
    {
        if (mult == -1)
        {
            mult = gridMult;
        }

        Vector2 rPos = roomPos;
        rPos /= mult;
        rPos.x = Mathf.Round(rPos.x);
        rPos.y = Mathf.Round(rPos.y);
        rPos *= mult;
        return rPos;
    }


}
