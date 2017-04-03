using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unit : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public Tilemap map;

    int moveSpeed = 1;

    public List<Node> currentPath = null;

    void Update()
    {
        if (currentPath != null)
        {
            int currNode = 0;
            while (currNode < currentPath.Count - 1)
            {
                Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y) +
                    new Vector3(0, 0, -1f);
                Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].y) +
                    new Vector3(0, 0, -1f);

                Debug.DrawLine(start, end, Color.red);

                currNode++;
            }
        }
    }

    public void MoveNextTile()
    {
        float remainingMovement = moveSpeed;


        while (remainingMovement > 0)
        {
            if (currentPath == null)
                return;


            //get cost from current tile to next tile
            remainingMovement -= map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y);

            //Move us to next tile in the sequence
            tileX = currentPath[1].x;
            tileY = currentPath[1].y;
            transform.position = map.TileCoordToWorldCoord(tileX, tileY);//update unity world position

            //remove old 'current' tile
            currentPath.RemoveAt(0);

            if (currentPath.Count == 1)
            {
                //only one tile left in path, and that is our destination
                //we are standing on it so lets clear the pathfinding info.
                currentPath = null;
            }
        }
    }
}
