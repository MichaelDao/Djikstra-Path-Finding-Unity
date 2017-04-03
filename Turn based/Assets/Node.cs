using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//node is also known as edges
public class Node
{
    public List<Node> neighbours;
    public int x, y;

    public Node()
    {
        neighbours = new List<Node>();
    }

    public float DistanceTo(Node n)
    {
        return Vector2.Distance(new Vector2(x, y), new Vector2(n.x, n.y));
    }

}


