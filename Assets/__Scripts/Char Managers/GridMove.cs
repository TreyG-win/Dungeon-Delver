using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMove : MonoBehaviour
{
    private IFacingMover mover;
    void Awake()
    {
        mover = GetComponent<IFacingMover>();
    }

    void FixedUpdate()
    {
        //If there is no movement, then nothing happens here
        if (!mover.moving) { return; }

        int facing = mover.GetFacing();

        //Aligns the character to a grid when moving in a direction
        
        //First step, the grid location is gathered
        Vector2 rPos = mover.roomPos;
        Vector2 rPosGrid = mover.GetRoomPosOnGrid();
        //Utilizes IFacingMover to choose grid spacing

        //The character is then automatically moved towards the grid line
        float delta = 0;
        if (facing == 0 || facing == 2) {
            // Aligns to the y grid with horizontal movement
            delta = rPosGrid.y - rPos.y;
        }
        else
        {
            // Aligns to the x grid with vertical movement
            delta = rPosGrid.x - rPos.x;
        }
        //If the character is already aligned, then nothing happens
        if (delta == 0) { return; }

        float move = mover.GetSpeed() * Time.fixedDeltaTime;
        move = Mathf.Min(move, Mathf.Abs(delta));
        if (delta < 0) move = -move;

        if (facing == 0 || facing == 2)
        {
            //Rotates the character to align to the y grid
            rPos.y += move;
        }
        else
        {
            //Rotates the character to align to the x grid
            rPos.x += move;
        }
        mover.roomPos = rPos;
    }
}
