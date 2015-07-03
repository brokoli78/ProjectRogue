using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectRogue
{
    public class TileMap
    {
        public int mapX;
        public int mapY;
        private int[,] map;
        private int[] empty;
        Dictionary<int, int> upConnections = new Dictionary<int, int>();
        Dictionary<int, int> downConnections = new Dictionary<int, int>();

        private Tile getTile(int x, int y)
        {
            return (Tile)GameController.gameObjectDatabase[map[x, y]];
        }

        private void setTile(int x, int y, int value)
        {
            map[x, y] = value;
        }

        public Tile this[int x, int y]
        {
            get { return this.getTile(x, y); }
            set { this.setTile(x, y, value.id); }
        }

        public TileMap(Map map)
        {
            this.mapX = map.mapX;
            this.mapY = map.mapY;
            this.map = new int[mapX, mapY];

            foreach(Tile t in map.tileMap)
            {
                this[t.x, t.y] = t;
                if (t.GetType() == typeof(UpStairTile))
                {
                    upConnections.Add(((UpStairTile)t).connection, t.id);
                }
                if (t.GetType() == typeof(DownStairTile))
                {
                    downConnections.Add(((DownStairTile)t).connection, t.id);
                }
            }

            emptyTiles = map.emptyTiles;
        }

        public TileMap(int mapX, int mapY, List<Tile> map)
        {
            this.mapX = mapX;
            this.mapY = mapY;
            this.map = new int[mapX, mapY];

            foreach (Tile t in map)
            {
                this[t.x, t.y] = t;
                if (t.GetType() == typeof(UpStairTile))
                {
                    upConnections.Add(((UpStairTile)t).connection, t.id);
                }
                if (t.GetType() == typeof(DownStairTile))
                {
                    downConnections.Add(((DownStairTile)t).connection, t.id);
                }
            }

            emptyTiles = map.Where(x => x.GetType() == typeof(FloorTile)).ToList();

        }

        public Tile upConnection(int i)
        {
            return (Tile) GameObject.gameObjectDatabase[upConnections[i]];
        }

        public Tile downConnection(int i)
        {
            return (Tile)GameObject.gameObjectDatabase[downConnections[i]];
        }


        public List<Tile> emptyTiles 
        {
            get { return empty.Select(x => (Tile)GameController.gameObjectDatabase[x]).ToList(); }
            set { empty = value.Select(x => x.id).ToArray(); }
        }

        public bool inMap(int x, int y)
        {
            return 0 <= x && 0 <= y && x < mapX && y < mapY;
        }
    }
}
