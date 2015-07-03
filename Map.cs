using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using ProjectRogue;
using Microsoft.Xna.Framework;

[Serializable()]
public class Map
{
    Random r;
    public Tile[,] tileMap;

    public List<Tile> emptyTiles = new List<Tile>();
    public List<Tile> notPopulatedTiles = new List<Tile>();
    
    #region important stuff
    int floorProb = 45;         //prob to build floor in step 1

    float averageRevisits = 5;       //average revisits in step 2     
    int allowedFloors = 4;       //allowed floors to border, if exeeded: build floor in step 2, if not, be a wall

    int maxCaveBorderFloors = 2;
    int cleanPasses = 5;
    int maxWallsAroundFloor = 3;

    public Node[,] map;
    public int mapX, mapY;

    int lowerCaveLimit = 16;
    int upperCaveLimit = 500;

    private List<List<Node>> caves;
    private List<List<Node>> corridors = new List<List<Node>>();

    int corridorSpacing = 2;
    int corridorMaxTurns = 10;
    int corridorMin = 2;
    int corridorMax = 5;

    int breakOut = 100000;

    public static List<Point> Directions4 = new List<Point>
    {
        new Point(0,-1),  //north
        new Point(0,1),   //south
        new Point(1,0),   //east
        new Point(-1,0),  //west
    };


    public static List<Point> Directions8 = new List<Point>()
    {
        new Point(0,-1),   //north
        new Point(0,1),    //south
        new Point(1,0),    //east
        new Point(-1,0),   //west
        new Point(1,-1),   //northeast
        new Point(-1,-1),  //northwest
        new Point(-1,1),   //southwest
        new Point(1,1),    //southeast
        new Point(0,0),    //centre
    };
    #endregion

    public Tile this[int x, int y]
    {
        get { return this.tileMap[x, y]; }
        set { this.tileMap[x, y] = value; }
    }

    public Map(int mapX, int mapY, Random r, int floor) //TODO: improve performance
    {
        this.r = r;
        map = new Node[mapX, mapY];
        this.mapX = mapX;
        this.mapY = mapY;

        generateMap();

        caves = findCaves();
        caves = clearCaves();
        generateCorridors(); //3 connections each
        generateCorridors();
        generateCorridors();
        finalize(floor);
    }

    #region node checks
    /// <summary>
    /// checks if x, y are in the map boundaries
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool inMap(int x, int y)
    {
        return 0 <= x && 0 <= y && x < mapX && y < mapY;
    }
    #endregion

    #region map generation caves

    #region generation

    private void generateMap()
    {
        for (int x = 0; x < mapX; x++) //build random qrcode
        {
            for (int y = 0; y < mapY; y++)
            {
                if (r.Next(100) <= floorProb)
                    map[x, y] = new Node(x, y, Node.tileType.floor);
                else
                    map[x, y] = new Node(x, y, Node.tileType.wall);
            }
        }

        int iterations = (int)((float)mapX * (float)mapY * averageRevisits);

        for(int i = 0; i < iterations; i++) //make caves
        {
            Node node = map[r.Next(mapX), r.Next(mapY)];

            if (node.getNeighbours(this, Directions8).Where(n => n.tile == Node.tileType.floor).Count() > allowedFloors) //switch for labyrinth
                map[node.x, node.y].tile = Node.tileType.floor;
            else
                map[node.x, node.y].tile = Node.tileType.wall;
        }

        for( int ctr = 0; ctr < cleanPasses; ctr++) // clean caves by flooring any walls with 3 bordering floors, do several passes
        {
            for (int x = 0; x < mapX; x++) 
            {
                for (int y = 0; y < mapY; y++)
                {
                    Node node = map[x, y];

                    if (node.getNeighbours(this, Directions4).Where(n => n.tile == Node.tileType.floor).Count() > maxCaveBorderFloors && node.tile == Node.tileType.wall)
                        map[node.x, node.y].tile = Node.tileType.floor;
                }
            }
        }

        for (int x = 0; x < mapX; x++)// remove any floors surrounded by walls
        {
            for (int y = 0; y < mapY; y++)
            {
                Node node = map[x, y];

                if (node.getNeighbours(this, Directions4).Where(n => n.tile == Node.tileType.wall).Count() + node.mapEdgeNeighbours(this, Directions4) > maxWallsAroundFloor 
                    && node.tile == Node.tileType.floor)
                    map[node.x, node.y].tile = Node.tileType.wall;
            }
        }
    }

    #endregion

    #region cleanup

    private List<List<Node>> findCaves() //finds all caves
    {
        List<List<Node>> c = new List<List<Node>>();

        for (int x = 0; x < mapX; x++)//examin each node 
        {
            for (int y = 0; y < mapY; y++)
            {
                Node node = map[x, y];

                if (node.tile == Node.tileType.floor && c.Count(s => s.Contains(node)) == 0) //if it is not in a cave and is a floor, a new cave was found
                {
                    List<Node> cave = new List<Node>();

                    locateCaveNodes(node, ref cave);

                    c.Add(cave);
                }
            }
        }

        return c;
    }

    private void locateCaveNodes(Node node, ref List<Node> placeholderCave) //recursive finder of caves (flood) TODO: remove all contains (where possible)
    {
        if(!placeholderCave.Contains(node))
        {
            placeholderCave.Add(node);

            foreach(Node n in node.getNeighbours(this, Directions4).Where(t => t.tile == Node.tileType.floor))
            {
                locateCaveNodes(n, ref placeholderCave);
            }
        }
    }

    private List<List<Node>> clearCaves()//delete all caves that do not fit the parameters
    {
        List<List<Node>> dummy = new List<List<Node>>();

        foreach(List<Node> cave in caves)
        {
            if (!(lowerCaveLimit <= cave.Count && cave.Count <= upperCaveLimit))
                dummy.Add(cave);
        }

        caves.RemoveAll(r => dummy.Contains(r));

        foreach(List<Node> d in dummy)
        {
            foreach(Node n in d)
            {
                map[n.x, n.y].tile = Node.tileType.wall;
            }
        }

        return caves;
    }

    #endregion

    #region corridors

    public bool generateCorridors()
    {
        List<List<Node>> cavesDummy = new List<List<Node>>();
        cavesDummy.AddRange(caves);

        if (cavesDummy.Count == 0)//safety net
        {
            throw new ArgumentNullException("caves", "There are no caves on this map!!");
        }

        List<Node> cave;
        List<List<Node>> connectedCaves = new List<List<Node>>();

        Node corridorNode;
        Point corridorDirection;

        int breakOutCtr = 0;

        List<Node> potentialCorridor = new List<Node>();

        //random cave selection

        cave = cavesDummy[r.Next(cavesDummy.Count)];

        connectedCaves.Add(cave);
        cavesDummy.Remove(cave);
    
        do //start building
        {
            //if we have never done this before, start with a cave
            if(corridors.Count == 0)
            {
                cave = connectedCaves[r.Next(connectedCaves.Count)];
                getRandomCaveEdge(cave, out corridorNode, out corridorDirection);
            }
            else
            {
                if(r.Next(100) < 50)
                {
                    cave = null ;
                    getRandomCorridorNode(out corridorNode, out corridorDirection);
                }
                else
                {
                    cave = connectedCaves[r.Next(connectedCaves.Count)];
                    getRandomCaveEdge(cave, out corridorNode, out corridorDirection);
                }
            }

            if (corridorAttempt(corridorNode, corridorDirection, true, out potentialCorridor)) //attempt to build cooridor
            {
                foreach(List<Node> ctr in cavesDummy) //evaluate all caves
                {
                    if(ctr.Contains(potentialCorridor.Last()))
                    {
                        if(cave == null || cave != ctr) //is non trivial point
                        {
                            potentialCorridor.Remove(potentialCorridor.Last()); //last is floor, so remove
                            corridors.Add(potentialCorridor);

                            foreach (Node n in potentialCorridor)
                                n.tile = Node.tileType.floor;

                            connectedCaves.Add(ctr);
                            cavesDummy.Remove(ctr);

                            break;
                        }
                    }
                }
            }

            if (breakOutCtr++ > breakOut)
                return false;
               
        } while (cavesDummy.Count > 0);

        return true;
    }

    private void getRandomCaveEdge(List<Node> cave, out Node corridorNode, out Point corridorDirection)
    {
        while(true)
        {
            //random point + direction in cave
            corridorNode = cave[r.Next(cave.Count)];
            corridorDirection = Directions4[r.Next(Directions4.Count)];

            while(true)
            {
                if(!(inMap(corridorNode.x + corridorDirection.X, corridorNode.y + corridorDirection.Y)))
                {
                    break;
                }
                else
                {
                    corridorNode = map[corridorNode.x + corridorDirection.X, corridorNode.y + corridorDirection.Y];

                    if(corridorNode.tile == Node.tileType.wall)
                    {
                        return;
                    }
                }
            }
        }
    }

    private void getRandomCorridorNode(out Node corridorNode, out Point corridorDirection)
    {
        List<Point> validDirections = new List<Point>();

        do
        {
            int count = 0;
            foreach (List<Node> c in corridors)
            {
                if (c.Count > 2)
                    count += c.Count - 2;

            }

            if (count > 0)
            {
                int rnd = r.Next(count);
                int c = 0;
                Point t = new Point(0, 0);

                for (int i = 0; i < corridors.Count; i++)
                {
                    if (corridors[i].Count > 2)
                    {
                        for (int j = 1; j < corridors[i].Count - 1; j++)
                        {
                            if (c == rnd)
                            {
                                t = new Point(corridors[i][j].x, corridors[i][j].y);
                            }
                            c++;
                        }
                    }
                }

                corridorNode = map[t.X, t.Y];
            }
            else
            {
                corridorNode = corridors[0][0];
            }

            foreach (Point p in Directions4)
            {
                if (inMap(corridorNode.x + p.X, corridorNode.y + p.Y))
                {
                    if (map[corridorNode.x + p.X, corridorNode.y + p.Y].tile == Node.tileType.wall)
                    {
                        validDirections.Add(p);
                    }
                }
            }
        } while (validDirections.Count == 0);

        corridorDirection = validDirections[r.Next(validDirections.Count)];
        corridorNode = map[corridorNode.x + corridorDirection.X, corridorNode.y + corridorDirection.Y];
    }

    private bool corridorAttempt(Node start, Point direction, bool preventBackTracking, out List<Node> potentialCorridor) //TODO: fix diagonal corridor intersections
    {
        potentialCorridor = new List<Node>();
        potentialCorridor.Add(start);

        int length;
        Point startDirection = direction;

        int allowedTurns = corridorMaxTurns;

        while(allowedTurns >= 0)
        {
            allowedTurns--;

            length = r.Next(corridorMin,corridorMax);
            
            while (length > 0)
            {
                length--;

                if (inMap(start.x + direction.X, start.y + direction.Y)) //in map?
                {
                    start = map[start.x + direction.X, start.y + direction.Y];

                    if(start.tile == Node.tileType.floor) //found something?
                    {
                        potentialCorridor.Add(start);
                        return true;
                    }
                    else if(!(corridorSpacingCheck(start, direction))) //to close to other corridor?
                    {
                        return false;
                    }

                    potentialCorridor.Add(start);

                }
                else
                {
                    return false;
                }
            }

            if (allowedTurns > 1)
            {
                if (preventBackTracking)
                {
                    direction = randomDirection4(direction, startDirection);
                }
                else
                {
                    direction = randomDirection4(direction);
                }
            }
        }

        return false;
    }

    private bool corridorSpacingCheck(Node node, Point direction)
    {
        for(int i = -corridorSpacing; i <= corridorSpacing; i++)
        {
            if(i != 0)
            {
                if(direction.X == 0) // north or south, so check east and west
                {
                    if(inMap(node.x + i, node.y))
                        if(map[node.x + i, node.y].tile == Node.tileType.floor)
                            return false;
                }
                else if(direction.Y == 0) // east or west, so check north and south
                {
                    if(inMap(node.x, node.y + i))
                        if(map[node.x, node.y + i].tile == Node.tileType.floor)
                            return false;
                }
            }
        }

        return true;
    }

    private Point randomDirection4(Point exclude1, Point? exclude2nullable = null) //random direction not equal to opposite od provided dir
    {
        /*if (r.Next(4) == 0)
            return exclude1;*/

        int num;
        List<Point> dummyDirection = new List<Point>() ;
        dummyDirection.AddRange(Directions4);

        Point exclude2 = exclude2nullable ?? Point.Zero;

        if(exclude2 == Point.Zero)
        {
            num = r.Next(3);
        }
        else
        {
            num = r.Next(2);
            dummyDirection.Remove(new Point(-exclude2.X, -exclude2.Y));
        }

        dummyDirection.Remove(new Point(-exclude1.X, -exclude1.Y));

        //Debug.WriteIf(dummyDirection[num] != exclude1, "!");

        return dummyDirection[num];
    }
    #endregion

    #endregion

    public void finalize(int floor)
    {
        tileMap = new Tile[mapX + 2, mapY + 2];

        for (int x = 1; x < mapX + 1; x++)
        {
            for (int y = 1; y < mapY + 1; y++)
            {
                Tile t;
                switch (map[x - 1, y - 1].tile)
                {
                    case Node.tileType.floor:
                        t = new FloorTile(x, y);
                        break;
                    case Node.tileType.wall:
                        t = new WallTile(x, y);
                        break;
                    default:
                        t = null;
                        break;
                }

                tileMap[x, y] = t;
            }
        }

        for (int x = 0; x < mapX + 2; x++)
        {
            tileMap[x, 0] = new WallTile(x, 0);
            tileMap[x, mapY + 1] = new WallTile(x, mapY + 1);

        }

        for (int y = 0; y < mapY + 2; y++)
        {
            tileMap[0, y] = new WallTile(0, y);
            tileMap[mapY + 1, y] = new WallTile(mapY + 1, y);

        }

        mapX += 2;
        mapY += 2;

        foreach (Tile t in tileMap)
        {
            if (t.GetType() == typeof(FloorTile))
            {
                emptyTiles.Add(t);
                notPopulatedTiles.Add(t);
            }
            if (Directions8.Where(d => inMap(t.x + d.X, t.y + d.Y) && tileMap[t.x + d.X, t.y + d.Y].GetType() == typeof(WallTile)).ToList().Count == Directions8.Where(d => inMap(t.x + d.X, t.y + d.Y)).Count())
                t.explorable = false;
        }

        if(floor != 0)
        {
            for (int i = 0; i < 3; i++)
            {
                int rand = r.Next(emptyTiles.Count);
                tileMap[emptyTiles[rand].x, emptyTiles[rand].y].DestroyNow();
                tileMap[emptyTiles[rand].x, emptyTiles[rand].y] = new UpStairTile(emptyTiles[rand].x, emptyTiles[rand].y, i);
                emptyTiles.RemoveAt(rand);
                notPopulatedTiles.RemoveAt(rand);

            }
        }

        if (floor != GameController.saveGame.maxFloors - 1)
        {
            for (int i = 0; i < 3; i++)
            {
                int rand = r.Next(emptyTiles.Count);
                tileMap[emptyTiles[rand].x, emptyTiles[rand].y].DestroyNow();
                tileMap[emptyTiles[rand].x, emptyTiles[rand].y] = new DownStairTile(emptyTiles[rand].x, emptyTiles[rand].y, i);
                emptyTiles.RemoveAt(rand);
                notPopulatedTiles.RemoveAt(rand);
            }
        }

        map = null;
    }

}
