using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSwap
{
    public int tileNum;
    public GameObject swapPrefab;
    public GameObject guaranteedItemDrop;
    public int overrideTileNum = -1;
}
public class TileCamera : MonoBehaviour
{
    // Set up all the variables...
    static private int W, H;
    // was private...TEST
    static public int[,] MAP;
    static public Sprite[] SPRITES;
    static public Transform TILE_ANCHOR;
    static public Tile[,] TILES;
    static public string COLLISIONS;

    [Header("Set in Inspector")]
    public TextAsset mapData;
    public Texture2D mapTiles;
    public TextAsset mapCollisions;
    public Tile tilePrefab;

    public int defaultTileNum;
    public List<TileSwap> tileSwaps;
    private Dictionary<int, TileSwap> tileSwapDict;
    private Transform enemyAnchor, itemAnchor;
    
    // Load the map
    void Awake()
    {
        // "mapCollisions" is passed through "RemoveLineEndings" leaving an array of characters
        // We do this because it it then matches with the array of sprites 0-255
        COLLISIONS = Utils.RemoveLineEndings(mapCollisions.text);

        PrepareTileSwapDict();
        enemyAnchor = (new GameObject("Enemy Anchor")).transform;
        itemAnchor = (new GameObject("Item Anchor")).transform;
        LoadMap();
    }

    public void LoadMap()
    {
        // This creates the "TILE_ANCHOR" so that all the tiles created will have this as their parent
        GameObject go = new GameObject("TILE_ANCHOR");
        TILE_ANCHOR = go.transform;

        // This will load all of the sprites from the "mapTiles" in the resource folder
        SPRITES = Resources.LoadAll<Sprite>(mapTiles.name);

        // This reads the map data and puts it into an array of string with each word being on a new line
        // Since "DelverData.txt" is assigned to the "mapData" field, we can access its text with "mapData.text"
        string[] lines = mapData.text.Split('\n');

        // This represents the total number of "lines"
        H = lines.Length;
        // The first line is then split on spaces (' ') which will put a 2-digit hexadecimal code -
        // into each element of the "tileNums" array
        string[] tileNums = lines[0].Split(' ');
        // Represents the total number of elements in "tileNums"
        W = tileNums.Length;

        // We need this so we can interpret the 2-character hexadecimals strings within "tileNums" as actual hexadecimals
        System.Globalization.NumberStyles hexNum = System.Globalization.NumberStyles.HexNumber; ;

        // We put the map data from "tileNums" into a 2D array for faster access to each tile
        MAP = new int[W, H];
        for (int j = 0; j < H; j++)
        {
            tileNums = lines[j].Split(' ');
            for (int i = 0; i < W; i++)
            {
                // Checking if the element at the current index is "..", because if it is, then it will be directly converted to "0"
                if (tileNums[i] == "..")
                {
                    MAP[i, j] = 0;
                }
                else
                {
                    // Parses the string into an int, and with the "hexNum" parameter, it knows to look for a hexadecimal number hence the (.. to 0)
                    MAP[i, j] = int.Parse(tileNums[i], hexNum);
                }

                CheckTileSwaps(i, j);
            }
        }
        print("Parsed " + SPRITES.Length + "sprites.");
        print("Map size: " + W + "wide by " + H + "high.");

        ShowMap();
    }

    // This method is called within "LoadMap()" to actually put the Tiles into the scene
    void ShowMap()
    {
        TILES = new Tile[W, H];

        // Go through the entire map and instantiate the Tiles where they need to be
        // This is confusing, but the instantiate is a different kind of instantiate because we are calling it on the tilePrefab and not the GameObject -
        // In short, we only need the prefab and not the GameObject, so we instantiate the tile prefab as a tile and then pass it into a local variable ("ti") -
        // The containing GameObject is still put into the scene, we just don't have to deal with it in the code
        for (int j = 0; j < H; j++)
        {
            for (int i = 0; i < W; i++)
            {
                if (MAP[i, j] != 0)
                {
                    Tile ti = Instantiate<Tile>(tilePrefab);

                    ti.transform.SetParent(TILE_ANCHOR);
                    // "ti" is called with only the location paremeters and not the "eTileNum" parameter from the "Tile" script
                    // The reason being is so the "tileNum" from "TileCamera.Map" is used
                    ti.SetTile(i, j);

                    TILES[i, j] = ti;
                }
            }
        }
    }

    void PrepareTileSwapDict() { 
        tileSwapDict = new Dictionary<int, TileSwap>();
        foreach (TileSwap ts in tileSwaps) {
            tileSwapDict.Add(ts.tileNum, ts);
        }
    }
    
    void CheckTileSwaps(int i, int j) { 
        int tNum = GET_MAP(i,j);
        if ( !tileSwapDict.ContainsKey(tNum) ) return;

        TileSwap ts = tileSwapDict[tNum];
        if (ts.swapPrefab != null) { 
            GameObject go = Instantiate(ts.swapPrefab);
            Enemy e = go.GetComponent<Enemy>();
            if (e != null) {
                go.transform.SetParent( enemyAnchor );
            } else {
                go.transform.SetParent( itemAnchor );
            }
            go.transform.position = new Vector3(i,j,0);
            if (ts.guaranteedItemDrop != null) { 
                
                if (e != null) {
                    e.guaranteedItemDrop = ts.guaranteedItemDrop;
                }
            }
        }

        if (ts.overrideTileNum == -1) { 
            SET_MAP( i, j, defaultTileNum );
        } else {
            SET_MAP( i, j, ts.overrideTileNum );
        }
    }

    // From what I understand "GET_MAP" with int and float, as well as "SET_MAP" are all for the protection of "getting" and "setting" the map -
    // while not having to worry about "IndexOutOfRangeException" errors
    static public int GET_MAP(int x, int y)
    {
        if (x < 0 || x >= W || y < 0 || y >= H)
        {
            return -1;
        }
        return MAP[x, y];
    }

    static public int GET_MAP(float x, float y)
    {
        int tX = Mathf.RoundToInt(x);
        // The .25 is for the partial amount of your character where half of their body could be outside of a singular tile but is still considered to be within that one tile
        int tY = Mathf.RoundToInt(y - .25f);
        return GET_MAP(tX, tY);
    }

    static public void SET_MAP(int x, int y, int tNum)
    {
        if (x < 0 || x >= W || y < 0 || y >= H)
        {
            return;
        }
        MAP[x, y] = tNum;
    }
    
    
}
