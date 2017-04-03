using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//reference
//https://www.youtube.com/watch?v=au6_95iI_gE


public class Tilemap : MonoBehaviour{

    public GameObject selectedUnit;
    public TileType[] tileTypes;

    int[,] tiles;
    Node[,] graph;

    int mapSizeX = 10;
    int mapSizeY = 10;

    // Use this for initialization
    void Start(){
        //setup the selectedUnit variable
        selectedUnit.GetComponent<unit>().tileX = (int)selectedUnit.transform.position.x;
        selectedUnit.GetComponent<unit>().tileY = (int)selectedUnit.transform.position.y;
        selectedUnit.GetComponent<unit>().map = this; // current preset position of unit

        GenerateMapData(); // Create map
        GeneratePathFindingGraph();
        GenerateMapVisual(); // Spawn visual prefabs
    }
   
    void GenerateMapData() {
        // Allocate map tiles
        tiles = new int[mapSizeX, mapSizeY];
        int x, y;

        // Initialize map tiles to be "path"
        for (x = 0; x < mapSizeX; x++){
            for (y = 0; y < mapSizeY; y++){
                tiles[x, y] = 0;
            }
        }

        //Make a big 'obstacle' area
        for (x = 3; x <= 5; x++){
            for (y = 0; y < 4; y++){
                tiles[x, y] = 1;
            }
        }
        
        // make u-shaped wall (hardcoded)
        //0 = path, 1 = obstacle, 2 = wall
        tiles[4, 4] = 2;
        tiles[5, 4] = 2;
        tiles[6, 4] = 2;
        tiles[7, 4] = 2;
        tiles[8, 4] = 2;

        tiles[4, 5] = 2;
        tiles[4, 6] = 2;
        tiles[8, 5] = 2;
        tiles[8, 6] = 2;
    }

    public float CostToEnterTile(int targetX, int targetY, int sourceX, int sourceY){
        //tile type variable (path, obstacle or wall)
        TileType tt = tileTypes[tiles[targetX, targetY]];

        if (UnitCanEnterTile(targetX, targetY) == false)        
            return Mathf.Infinity; //return infinity
        
        float cost = tt.movementCost;

        if (sourceX != targetX && sourceY != targetY)
        {
            //fix the strange diagonal movement, make it prefer moving straight
            cost += 0.001f;
        }
        return cost;
    }
    
    void GeneratePathFindingGraph(){
        //Initialize the array
        graph = new Node[mapSizeX, mapSizeY];

        //Initialize a node for each spot in the array
        for (int x = 0; x < mapSizeX; x++){
            for (int y = 0; y < mapSizeY; y++){                
                    graph[x, y] = new Node();
                    graph[x, y].x = x;
                    graph[x, y].y = y;                
            }
        }

        //now nodes exist calculate all their neighbours
        for (int x = 0; x < mapSizeX; x++){
            for (int y = 0; y < mapSizeY; y++)
            {

                /*we have a 4 way connected map
             if (x > 0)
                 graph[x, y].neighbours.Add(graph[x - 1, y]);
             if (x < mapSizeX - 1)
                 graph[x, y].neighbours.Add(graph[x + 1, y]);
             if (y > 0)
                 graph[x, y].neighbours.Add(graph[x, y - 1]);
             if (y < mapSizeY - 1)
                 graph[x, y].neighbours.Add(graph[x, y + 1]);
             */

                //this is the 8 way connection               
                if (x > 0){ //try left
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                    if (y > 0)
                        graph[x, y].neighbours.Add(graph[x - 1, y - 1]);
                    if (y < mapSizeY - 1)
                        graph[x, y].neighbours.Add(graph[x - 1, y + 1]);
                }
                              
                if (x < mapSizeX - 1){ //try right
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                    if (y > 0)
                        graph[x, y].neighbours.Add(graph[x + 1, y - 1]);
                    if (y < mapSizeY - 1)
                        graph[x, y].neighbours.Add(graph[x + 1, y + 1]);
                }

                //try up and down
                if (y > 0)
                    graph[x, y].neighbours.Add(graph[x, y - 1]);
                if (y < mapSizeY - 1)
                    graph[x, y].neighbours.Add(graph[x, y + 1]);
            }
        }
    }

    void GenerateMapVisual(){
        for (int x = 0; x < mapSizeX; x++){
            for (int y = 0; y < mapSizeY; y++){

                TileType tt = tileTypes[tiles[x, y]];
                GameObject go = (GameObject)Instantiate(tt.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity);

                //determine click position
                ClickableTile ct = go.GetComponent<ClickableTile>();
                ct.tileX = x;
                ct.tileY = y;
                ct.map = this;
            }
        }
    }

    public Vector3 TileCoordToWorldCoord(int x, int y){
        return new Vector3(x, y, 0);
    }

    public bool UnitCanEnterTile(int x, int y)
    {
        //we could test the hover type here against terrain flags to see if they can enter
        return tileTypes[tiles[x,y]].isWalkable;
    }

    public void GeneratePathTo(int x, int y){
        // Clear out old path
        selectedUnit.GetComponent<unit>().currentPath = null;

        if (UnitCanEnterTile(x, y) == false)
        {
            //if wall or obstacle is clicked then wuit
            return;
        }

        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        //list of nodes we havent checked yet
        List<Node> unvisited = new List<Node>();

        Node source = graph[
            selectedUnit.GetComponent<unit>().tileX,
            selectedUnit.GetComponent<unit>().tileY];

        Node target = graph[x, y];

        dist[source] = 0;
        prev[source] = null;


        // Initialize everything to have INFINITY distance, since we dont know
        //any better right now. Its also possilbe some nodes cant be reached from
        // the source, which makes infinity a reasonable value.
        foreach (Node v in graph){
            if (v != source){
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }
            unvisited.Add(v);
        }

        while (unvisited.Count > 0){
            //u is going to be the unvisited node with the smallest distance
            Node u = null;

            foreach (Node possibleU in unvisited){
                if (u == null || dist[possibleU] < dist[u]){
                    u = possibleU;
                }
            }

            if (u == target){
                break; //exit loop
            }

            unvisited.Remove(u);

            foreach (Node v in u.neighbours){
                // float alt = dist[u] + u.DistanceTo(v);
                float alt = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);

                if (alt < dist[v]){
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        //if we get there, either we found the shortest route or there is no route at all.
        if (prev[target] == null){
            //no route between our target and source.
            return;
        }

        List<Node> currentPath = new List<Node>();
        Node curr = target;

        while (curr != null){
            currentPath.Add(curr);
            curr = prev[curr];
        }

        //Right now, currentPath describes a route from our target to our source
        //so we invere it
        currentPath.Reverse();
        selectedUnit.GetComponent<unit>().currentPath = currentPath;
    }  
}