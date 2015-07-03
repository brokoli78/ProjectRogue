using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using ProjectRogue;
using Microsoft.Xna.Framework;

[Serializable]
public class Node
{
    #region member variables
    public enum tileType { wall, floor };

    private tileType t;

    public tileType tile
    {
        get { return t; }
        set{ t = value; }
    }

    private int xCoord, yCoord;

    public int x
    {
        get { return xCoord; }
        protected set { xCoord = value; }
    }

    public int y
    {
        get { return yCoord; }
        protected set { yCoord = value; }
    }

    
    #endregion

    public Node(int x, int y, tileType tile)
    {
        this.x = x;
        this.y = y;
        this.tile = tile;
    }

    #region member functions

    public List<Node> getNeighbours(Map map, List<Point> dir) //ALL THE THINGS
    {
        List<Point> p = dir.Select(d => new Point(d.X + x, d.Y + y)).ToList();
        List<Node> n = new List<Node>();

        foreach (Point point in p)
        {
            if (map.inMap(point.X, point.Y))
                n.Add(map.map[point.X, point.Y]);
        }
        return n;
    }

    public int mapEdgeNeighbours(Map map, List<Point> dir)
    {
        List<Point> p = dir.Select(d => new Point(d.X + x, d.Y + y)).ToList();
        int num = 0;

        foreach (Point point in p)
        {
            if (!map.inMap(point.X, point.Y))
                num++;
        }
        return num;
    }

    #endregion
}
