using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ProjectRogue
{
    public static class AStar
    {
        public static List<Vector2> Directions4 = new List<Vector2>()
        {
            new Vector2(0,-1),   //north
            new Vector2(0,1),    //south
            new Vector2(1,0),    //east
            new Vector2(-1,0),   //west
        };

        public static List<Vector2> DirectionsDiagonal = new List<Vector2>()
        {
            new Vector2(1,-1),   //northeast
            new Vector2(-1,-1),  //northwest
            new Vector2(-1,1),   //southwest
            new Vector2(1,1),    //southeast
        };


        public static bool CalculatePath(TileMap map, Tile startTile, Tile endTile, out List<Tile> path)
        {
            MapNode[,] nodeMap = new MapNode[map.mapX, map.mapY];
            path = new List<Tile>();
            if (startTile == null || startTile.walkable == false || endTile == null || endTile.walkable == false)
                return false;

            MapNode start = new MapNode(map, startTile);
            MapNode end = new MapNode(map, endTile);

            List<MapNode> openList = new List<MapNode>();
            List<MapNode> closedList = new List<MapNode>();

            start.score = new Score(0, 0 + hce(start, end));

            openList.Add(start); //add start node

            while (openList.Count > 0)
            {
                MapNode currentNode = Node_With_Min_f_Score(openList);

                if (currentNode.score.f_score == double.PositiveInfinity)
                {
                    return false;
                }

                if (currentNode == end)
                {
                    path = ConstructPath(currentNode, start);
                    return true;
                }

                closedList.Add(currentNode);
                currentNode.closed = true;

                if (currentNode.cost == double.PositiveInfinity)
                {
                    openList.Remove(currentNode);
                    currentNode.open = false;
                    continue;
                }

                foreach (MapNode neighbor in currentNode.getConnectedNodes(nodeMap)) //überprüfe jeden nachbar
                {

                    if (neighbor.closed)
                        continue;

                    double tentative_g_score = currentNode.score.g_score + currentNode.cost + neighbor.costModifier;
                    if (!neighbor.open) //wenn der nachbar noch nicht in der openlist ist oder der pfad kürzer...
                    {
                        neighbor.score = new Score(tentative_g_score, tentative_g_score + hce(neighbor, end));
                        openList.Add(neighbor);		//.....füge ihn hinzu bzw. aktualisiere ihn
                        neighbor.open = true;
                        //Debug.WriteLine(openList[neighbor].ToString());

                        neighbor.previousNode = currentNode;
                    }
                    else
                    {
                        if (tentative_g_score < openList.First(node => node == neighbor).score.g_score)
                        {
                            neighbor.score = new Score(tentative_g_score, tentative_g_score + hce(neighbor, end));
                            neighbor.previousNode = currentNode;
                        }
                    }
                }

                openList.Remove(currentNode);	// lösche die currentnode aus der openlist
                currentNode.open = false;
            }

            return false;
        }

        public static bool ExplorePath(TileMap map, Tile startTile, out List<Tile> path)
        {
            MapNode[,] nodeMap = new MapNode[map.mapX, map.mapY];
            path = new List<Tile>();

            if (startTile == null || startTile.walkable == false)
                return false;

            MapNode start = new MapNode(map, startTile);

            List<MapNode> openList = new List<MapNode>();
            List<MapNode> closedList = new List<MapNode>();

            start.score = new Score(0, 0);

            openList.Add(start); //add start node

            while (openList.Count > 0)
            {
                MapNode currentNode = Node_With_Min_f_Score(openList);

                if (currentNode.score.f_score == double.PositiveInfinity)
                {
                    return false;
                }

                if (((Tile)currentNode).explorable && !((Tile)currentNode).wasVisible && (Tile) currentNode != startTile)
                {
                    path = ConstructPath(currentNode, start);
                    return true;
                }

                closedList.Add(currentNode);
                currentNode.closed = true;

                if (currentNode.cost == double.PositiveInfinity)
                {
                    openList.Remove(currentNode);
                    currentNode.open = true;
                    continue;
                }

                foreach (MapNode neighbor in currentNode.getConnectedNodes(nodeMap)) //überprüfe jeden nachbar
                {

                    if (neighbor.closed)
                        continue;

                    double tentative_g_score = currentNode.score.g_score + currentNode.cost + neighbor.costModifier;

                    if (!neighbor.open) //wenn der nachbar noch nicht in der openlist ist oder der pfad kürzer...
                    {
                        neighbor.score = new Score(tentative_g_score, tentative_g_score);
                        openList.Add(neighbor);		//.....füge ihn hinzu bzw. aktualisiere ihn
                        neighbor.open = true;
                        //Debug.WriteLine(openList[neighbor].ToString());

                        neighbor.previousNode = currentNode;
                    }
                    else
                    {
                        if (tentative_g_score < openList.First(node => node == neighbor).score.g_score)
                        {
                            neighbor.score = new Score(tentative_g_score, tentative_g_score);
                            neighbor.previousNode = currentNode;
                        }
                    }
                }

                openList.Remove(currentNode);	// lösche die currentnode aus der openlist
                currentNode.open = false;
            }

            return false;
        }

        private static List<Tile> ConstructPath(MapNode end, MapNode start)
        {
            List<Tile> path = new List<Tile>();
            MapNode currentNode = end;
            while(currentNode != start)
            {
                path.Add((Tile)currentNode);
                currentNode = currentNode.previousNode;
            }

            return path;
        }

        static double hce(MapNode start, MapNode end) //heuristic_cost_estimate (luftliniendistanz) TODO verify
        {
            return (double)Math.Sqrt(Math.Pow(start.x - end.x, 2F) + Math.Pow(start.y - end.y, 2));
        }

        static MapNode Node_With_Min_f_Score(List<MapNode> list)
        {
            double min = double.PositiveInfinity;
            MapNode minNode = null;
            foreach (MapNode k in list)
            {
                if (min >= k.score.f_score)
                {
                    min = k.score.f_score;
                    minNode = k;
                }
            }

            //Debug.WriteLine(list[minNode].f_score);
            //Debug.WriteLine(min);
            if (minNode == null)
                throw new ArgumentNullException("no Minimum node exists");
            return minNode;
        }

        internal class Score
        {
            public double g_score = double.PositiveInfinity;
            public double f_score = double.PositiveInfinity;

            public Score(double g_score, double f_score)
            {
                this.g_score = g_score;
                this.f_score = f_score;
            }

            public override string ToString()
            {
                return g_score.ToString() + "; " + f_score.ToString();
            }
        }

        internal class MapNode
        {
            internal bool open = false;
            internal bool closed = false;

            internal Score score;
            internal double costModifier;

            int xCoord, yCoord;
            public Vector2 position
            {
                get { return new Vector2(xCoord, yCoord); }

                set {
                    xCoord = (int) value.x;
                    yCoord = (int) value.y;
                }
            }
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

            internal readonly double cost;

            TileMap map;

            internal List<MapNode> getConnectedNodes(MapNode[,] nodeMap)
            {
                List<MapNode> connectedNodes = new List<MapNode>();
                connectedNodes.AddRange(DirectionsDiagonal.Where(v2 => map.inMap((int)v2.x + x, (int)v2.y + y)).Select(v2 => new MapNode(map, map[(int)v2.x + x, (int)v2.y + y], 0.00001)));
                connectedNodes.AddRange(Directions4.Where(v2 => map.inMap((int)v2.x + x, (int)v2.y + y)).Select(v2 => new MapNode(map, map[(int)v2.x + x, (int)v2.y + y], 0)));
                for (int i = 0; i < connectedNodes.Count; i++)
                {
                    if (nodeMap[connectedNodes[i].x, connectedNodes[i].y] == null)
                    {
                        nodeMap[connectedNodes[i].x, connectedNodes[i].y] = connectedNodes[i];
                    }
                    else
                    {
                        connectedNodes[i] = nodeMap[connectedNodes[i].x, connectedNodes[i].y];
                    }
                }
                return connectedNodes;
            }

            public MapNode previousNode;

            internal MapNode(TileMap map, Vector2 position, double cost)
                : this(map, (int)position.x, (int)position.y, cost) { }

            internal MapNode(TileMap map, Tile tile)
                : this(map, tile.x, tile.y, tile.MovementCost) { }

            internal MapNode(TileMap map, Tile tile, double costModifier)
                : this(map, tile.x, tile.y, tile.MovementCost) 
            {
                this.costModifier = costModifier;
            }

            internal MapNode(TileMap map, int x, int y, double cost)
            {
                this.map = map;
                this.x = x;
                this.y = y;
                this.cost = cost;
            }

            
            public override bool Equals(Object obj)
            {
                MapNode node = obj as MapNode;
                if ((object)node == null)
                    return false;

                return this == node;
            }

            public override int GetHashCode()
            {
                return (int)(x + y + cost);
            }

            public static bool operator ==(MapNode node1, MapNode node2)
            {
                // If both are null, or both are same instance, return true.
                if (Object.ReferenceEquals(node1, node2))
                {
                    return true;
                }

                // If one is null, but not both, return false.
                if (((object)node1 == null) || ((object)node2 == null))
                {
                    return false;
                }

                // Return true if the fields match:
                return node1.x == node2.x && node1.y == node2.y;
            }

            public static bool operator !=(MapNode a, MapNode b)
            {
                return !(a == b);
            }

            public static explicit operator Tile(MapNode node)
            {
                return node.map[node.x, node.y];
            }

        }

    }
}
