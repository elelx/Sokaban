using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridController : MonoBehaviour
{

    //refrences
    public Tile block;
    public Tile wall;
    public Tile background;
    public Tile goal;


    //singleton so can call GridController.instance
    public static GridController instance;



    private Grid grid;
    private Tilemap tilemap;
    private Tilemap goalsTilemap;
    public Tilemap respawnTilemap;


    //audio

    public AudioSource audioSource;

    public AudioClip goalClip;
    public AudioClip respawnClip;


    private void Awake()
    {
        // if there's already an instance in the scene, destroy ourselves
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        // set the singleton 
        instance = this;

        grid = GetComponent<Grid>();//find grid on player game obj

        //fnd "blcktilemap", --> get tile map component
        tilemap = transform.Find("BlocksTileMap").GetComponent<Tilemap>();

        goalsTilemap = transform.Find("GoalsTileMap").GetComponent<Tilemap>();

    }


    // converts grid coordinates x y to world position(vector 3 ) for player

    public Vector3 GetWorldPos(int x, int y)
    {
        return grid.CellToWorld(new Vector3Int(x, y, 0));
    }


    // returns true if there is a block at this space

    //look at tile map at x,y and return/give us what tile it is
    public TileType GetTile(int x, int y)
    {

        //check what tile is at the tilebloc
        var tile = tilemap.GetTile(new Vector3Int(x, y, 0));

        //if no tile/bg tile --> treat it as empty
        if (tile == null || tile == background)
            return TileType.Empty;

        Debug.Log($"got tile: {tile.name}");


        // if tile is block --> its a block
        if (tile == block)

            return TileType.Block;

        // if tile is wall --> its a wall
        if (tile == wall)
            return TileType.Wall;


        Debug.LogError($"unknown tile type: {tile.name}. Did you forget to update GetTile()?");
        return TileType.Empty;
    }

    //iis this block a wall?
    public bool IsWall(int x, int y)
    {
        var tile = tilemap.GetTile(new Vector3Int(x, y, 0));
        return tile == wall;
    }

    public bool IsGoal(int x, int y)
    {
        var tile = goalsTilemap.GetTile(new Vector3Int(x, y, 0));
         
            return tile == goal;
     
    }

    // pushes a block from "start" to coordinates "destination"

    public void PushBlock(Vector3Int start, Vector3Int destination)
    {

        // If the destination already has ANOTHER block, push that one first (chain push).
        if (GetTile(destination.x, destination.y) == TileType.Block)
        {
            //which way to push?
            var direction = destination - start;

            //push the next block the same way as the previous
            PushBlock(destination, destination + direction);
        }


        //after path is clear, move this block:

        // set start position to be empty (background sprite)
        //1.remove block from its start , null = empty

        tilemap.SetTile(start, null);


        //2. place blokc tile @ destination tile
        // set destination sprite to be the block

        tilemap.SetTile(destination, block);

        if (IsGoal(destination.x, destination.y))
        {
            if (audioSource && goalClip)
                audioSource.PlayOneShot(goalClip);
        }


    }


    //can we push block from start to dest?
    public bool CanPushBlock(Vector3Int start, Vector3Int destination)
    {

    

        //if destination is empty, then return true qnd we cn push

        if (GetTile(destination.x, destination.y) == TileType.Empty)
            return true;

        //theres a wall .. its unpushable ):

        if (GetTile(destination.x, destination.y) == TileType.Wall)
            return false;


        //if destination is a block --> ask recursiviley:
        //can THAT block be pushed one setp further in the same direction?
        //ti s is a block we can push it

        var direction = destination - start;
        return CanPushBlock(destination, destination + direction);

    }

    public bool AllGoalsCovered()
    {
        if (goalsTilemap == null) return false;

        bool hasAnyGoal = false;
        foreach (var pos in goalsTilemap.cellBounds.allPositionsWithin)
        {
            if (!goalsTilemap.HasTile(pos)) continue;   // only care about goal 
            hasAnyGoal = true;

            // this goal must currently have a block on the Blocks tilemap
            if (GetTile(pos.x, pos.y) != TileType.Block)
                return false;
        }

        return hasAnyGoal; // true only if there was at least 1 goal and all had blocks
    }


    //*************** THIS I NEEEDED HELP ON
    //takes the first block it finds on the grid and moves it to a random empty respawn spot.

   
    public bool MoveAnyBlockToRandomRespawn()
    {
        // if no respawn tilemap is assigned, stop and return false
        if (respawnTilemap == null) return false;


        // STEP 1: Find all possible respawn spots

        List<Vector3Int> spots = new List<Vector3Int>();   // list of available spots


        var respawnBounds = respawnTilemap.cellBounds;                // bounds of the respawn grid

        // go through every cell in the respawn area
        for (int y = respawnBounds.yMin; y < respawnBounds.yMax; y++)
        {
            for (int x = respawnBounds.xMin; x < respawnBounds.xMax; x++)
            {
                var p = new Vector3Int(x, y, 0);

                // skip if this cell is NOT a respawn square
                if (!respawnTilemap.HasTile(p)) continue;

                // skip if this cell is NOT empty (already has something in it)
                if (GetTile(x, y) != TileType.Empty) continue;

                // otherwise, it's a valid empty respawn spot → add it to the list
                spots.Add(p);
            }
        }

        // if no empty respawn spots exist, stop and return false
        if (spots.Count == 0) return false;


        // STEP 2: Find any block that we can move


        var bb = tilemap.cellBounds;   // bounds of the main block grid

        Vector3Int from = default;     // where the block will move from

        bool found = false;

        // go through every cell in the main tilemap
        for (int y = bb.yMin; y < bb.yMax && !found; y++)
        {
            for (int x = bb.xMin; x < bb.xMax && !found; x++)
            {
                var p = new Vector3Int(x, y, 0);

                // if this cell has a block, remember its position and mark it found
                if (tilemap.GetTile(p) == block)
                {
                    from = p;
                    found = true;
                }
            }
        }

        // if no block was found at all, stop and return false
        if (!found) return false;


        // STEP 3: Move the block to a random empty respawn spot


        // pick a random spot from the list of available respawns
        var to = spots[UnityEngine.Random.Range(0, spots.Count)];

        // clear the old spot
        tilemap.SetTile(from, null);

        // place the block in the new spot
        tilemap.SetTile(to, block);



        tilemap.SetTile(to, block);


        if (audioSource && respawnClip)
            audioSource.PlayOneShot(respawnClip);



        return true;
    }



    // Converts a world position (Vector3) into grid cell coordinates (Vector3Int)
    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        return grid.WorldToCell(worldPos);
    }


}

//categories of tile we care


public enum TileType { Wall, Block, Empty, Goal };


//public class Thing

//{
//    public void PushBlock(Vector3Int start, Vector3Int destination)
//    {



//    }
//}


//recurrsions




//    //so this will keep going until it can, this will tell u if u can push a block


////f(n) = f(n-2)+f(n-1) the fibonacci sequence 
////1,1,2,3,5,8,13 adding it all to itsself

////will call itself forever unless u stop 
//public int DoubleBlocks(int n)
//{

//    if (n <= 1) // this is how u stop it 
//    {
//        return 1;
//    }

//    return DoubleBlocks(n - 2) + DoubleBlocks(n - 1);
//}

//-- MY PAST CODES----------------------

//    //singletons

//    public Tile block;
//    public Tile wall;

//    public Tile bg;

//    public static GridController instance;

//    private Grid grid;
//    private Tilemap tilemap;

//    // Start is called before the first frame update
//    void Awake()// awake run b4 start
//    {

//        //if theres already an instance in scene, destroy oursevle

//        if (instance != null)
//        {
//            Destroy(this);
//            return;
//        }

//        instance = this;

//        grid = GetComponent<Grid>();
//        tilemap = transform.Find("BlocksTileMap").GetComponent<Tilemap>();

//    }


//    public Vector3 GetWorldPos(int x, int y)
//    {
//        return grid.CellToWorld(new Vector3Int(x, y, 0));
//    }


//    //public bool IsBlock(int x, int y) //need this so they know wht is being pushed
//    //    {
//    //    Vector3Int cell = new Vector3Int(x, y, 0);
//    //    TileBase t = tilemap.GetTile(cell);
//    //    return t != null && t != bg;
//    //}

//    //    public bool IsEmpty(int x, int y)
//    //{
//    //    Vector3Int cell = new Vector3Int(x, y, 0);
//    //    TileBase t = tilemap.GetTile(cell);
//    //    return t == null || t == bg;
//    //}



//    //public bool IsOccupied(int x, int y)
//    //{
//    //    var tile = tilemap.GetTile(new Vector3Int(x, y, 0));
//    //    return tile != null;
//    //}

//    public TileType GetTile(int x, int y)
//    {
//        var tile = tilemap.GetTile(new Vector3Int(x, y, 0));

//        if (tile == null || tile == background)
//            return TileType.Empty;

//        if (tile == wall)
//            return TileType.Wall;

//        if (tile == block)
//            return TileType.Block;

//        return TileType.Empty;



//    }


//    //pushing block

//    public void PushBlock(Vector3Int start, Vector3Int destination)
//    {
//        //set srt pos to be emptio ( bg sprite_
//        tilemap.SetTile(start, bg);

//        //set dest, sprite to be the blokc
//        tilemap.SetTile(destination, block);

//    }


//    public enum TileType { Wall, Block, Empty }; //ftuture can add goal, bg
//    public class thing

//    {
//        public void DoStuff(TileType msg)
//        {
//            if (msg == TileType.Wall)
//            {

//            }
//            if (msg == TileType.Block)
//            {

//            }

//            if (msg == TileType.Empty)
//            {

//            }
//        }
//    }



//    //enums = enumberable --> example a bad thing is typoes, prone to mistakes
//    //public class thing{
//    //public void DoStuff(string msg)
//    //{
//    //    if (msg == thing1)
//    //    {
//    //wtv
//    //    }
//    //    if (msg == thing2)
//    //    {
//    //wtv
//    //    }

//    //    if (msg == thing3)
//    //    {
//    //wtv
//    //    }
//    //}

//    //bter way it helps with no typos
//    //public enum thingOption{thing1,thing2,thing3};
//    //pbulic class thing { 
//    //public void DoStuff(thingOption msg)
//    //{
//    //    if (msg == thingOption.thing1)
//    //    {
//    //  
//    //    }
//    //    if (msg == thingOption.thing2)
//    //    {
//    //wtv
//    //    }

//    //    if (msg == thingOption.thing3)
//    //    {
//    //wtv
//    //    }
//    //}
//    //}

//}



