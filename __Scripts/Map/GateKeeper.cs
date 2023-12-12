using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateKeeper : MonoBehaviour
{

    public InventoryUI inventoryUI;
    // Locked door tile numbers
    const int lockedR = 73;
    const int lockedUR = 57;
    const int lockedUL = 58;
    const int lockedL = 72;
    const int lockedDL = 90;
    const int lockedDR = 91;

    // Open door tile numbers
    const int openR = 70;
    const int openUR = 53;
    const int openUL = 52;
    const int openL = 67;
    const int openDL = 84;
    const int openDR = 85;

    private IkeyMaster keys;

    void Awake()
    {
        keys = GetComponent<IkeyMaster>();
    }

    // This method will return if dray has no keys and will avoid further execution if he doesnt have any as well
    // Basically this will make sure that a key doesnt get decremented from the key count unless the door is unlocked in a pure form
    // This means that the player can walk past without opening it. The player has to face the door to open it
    void OnCollisionStay(Collision coll)
    {
        // If there are no keys, there is no need to run this method
        if (keys.keyCount < 1)
        {
            return;
        }

        // This makes sure it only interacts with tiles
        Tile ti = coll.gameObject.GetComponent<Tile>();
        if (ti == null)
        {
            return;
        }

        // Only opens if dray is facing the right directions
        int facing = keys.GetFacing();
        // Check if door tile
        Tile ti2;
        // Cases within a switch statement can't be variables that is why they are declared as const at the top
        switch (ti.tileNum)
        {
            case lockedR:
                if (facing != 0) return;
                ti.SetTile(ti.x, ti.y, openR);
                break;

            case lockedUR:
                if (facing != 1) return;
                ti.SetTile(ti.x, ti.y, openUR);
                ti2 = TileCamera.TILES[ti.x - 1, ti.y];
                ti2.SetTile(ti2.x, ti2.y, openUL);
                break;

            case lockedUL:
                if (facing != 1) return;
                ti.SetTile(ti.x, ti.y, openUL);
                ti2 = TileCamera.TILES[ti.x + 1, ti.y];
                ti2.SetTile(ti2.x, ti2.y, openUR);
                break;

            case lockedL:
                if (facing != 2) return;
                ti.SetTile(ti.x, ti.y, openL);
                break;

            case lockedDL:
                if (facing != 3) return;
                ti.SetTile(ti.x, ti.y, openDL);
                ti2 = TileCamera.TILES[ti.x + 1, ti.y];
                ti2.SetTile(ti2.x, ti2.y, openDR);
                break;

            case lockedDR:
                if (facing != 3) return;
                ti.SetTile(ti.x, ti.y, openDR);
                ti2 = TileCamera.TILES[ti.x - 1, ti.y];
                ti2.SetTile(ti2.x, ti2.y, openDL);
                break;

            default:
                return;
        }

        keys.keyCount--;
        inventoryUI.UpdateKeyCount(keys.keyCount);
    }
}
