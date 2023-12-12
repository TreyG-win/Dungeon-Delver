using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Set Dynamically")]
    // initializing variables
    public int x, y, tileNum;

    // Reference to the box collider of the instance tile
    private BoxCollider bColl;

    void Awake()
    {
        bColl = GetComponent<BoxCollider>();
    }


    // This method has an "optional" parameter for "eTileNum"
    // The reason for this is so if nothing is passed in for eTileNum, "-1", then the default tile will be read from "TileCamera.Get_Map()"
    public void SetTile(int eX, int eY, int eTileNum = -1)
    {
        x = eX;
        y = eY;
        transform.localPosition = new Vector3(x, y, 0);
        // This toString call on gameobject.name basically outputs a decimal ("D") that is 3 characters long (hence "D3")
        // An example would be like x = 15 and y = 2, the output for this would be "015x002", it fills in zero's as needed
        gameObject.name = x.ToString("D3") + "x" + y.ToString("D3");

        // Like previously mentioned, if eTileNum = -1 then the default tile is read from "TileCamera.Get_Map()"
        if (eTileNum == -1)
        {
            // was GET_MAP ...TEST
            eTileNum = TileCamera.GET_MAP(x, y);
        }
        else
        {
            TileCamera.SET_MAP(x, y, eTileNum);
        }


        // Assign the corresponding sprite to the tile
        tileNum = eTileNum;
        GetComponent<SpriteRenderer>().sprite = TileCamera.SPRITES[tileNum];

        // After setting the tile the collider is then set as well
        SetCollider();
    }

    // Arranging collider for each tile
    void SetCollider()
    {
        // Info is pulled from the "DelverCollisions.txt" file
        bColl.enabled = true;
        // "tileNum" is used to access the correct collision character 'S', 'W', 'A', or 'D'
        // Every sprite has a character overlay-ed to it
        char c = TileCamera.COLLISIONS[tileNum];
        switch (c)
        {
            case 'S': // Whole tile collision
                bColl.center = Vector3.zero;
                bColl.size = Vector3.one;
                break;
            case 'W': // Top tile collision
                bColl.center = new Vector3(0, 0.25f, 0);
                bColl.size = new Vector3(1, 0.5f, 1);
                break;
            case 'A': // Left side tile collision
                bColl.center = new Vector3(-0.25f, 0, 0);
                bColl.size = new Vector3(0.5f, 1, 1);
                break;
            case 'D': // Right side tile collision
                bColl.center = new Vector3(0.25f, 0, 0);
                bColl.size = new Vector3(0.5f, 1, 1);
                break;



            // optional stuff ... TEST

            case 'Q': // Top, Left
                bColl.center = new Vector3(-0.25f, 0.25f, 0);
                bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;
            case 'E': // Top, Right
                bColl.center = new Vector3(0.25f, 0.25f, 0);
                bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;
            case 'Z': // Bottom, left
                bColl.center = new Vector3(-0.25f, -0.25f, 0);
                bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;
            case 'X': // Bottom
                bColl.center = new Vector3(0, -0.25f, 0);
                bColl.size = new Vector3(1, 0.5f, 1);
                break;
            case 'C': // Bottom, Right
                bColl.center = new Vector3(0.25f, -0.25f, 0);
                bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;



            default: // Anything else in the data file a.k.a '_' or '|'
                bColl.enabled = false;
                break;
        }
    }
}
