using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRogue
{
    [Serializable]
    public class SaveGame
    {
        public readonly int maxFloors;
        public TileMap[] map;
        public Dictionary<int, GameObject>[] gameObjectDatabase;
        public List<Creature>[] creatureDatabase;
        public int[] lastId;
        public Player player;
        public List<Message> gameLog = new List<Message>();
        public int turnCounter = 1;
        public int currentFloor = 0;

        public SaveGame(int maxFloors)
        {
            this.maxFloors = maxFloors;
            gameObjectDatabase = new Dictionary<int, GameObject>[maxFloors];
            map = new TileMap[maxFloors];
            creatureDatabase = new List<Creature>[maxFloors];
            lastId = new int[maxFloors];

            for (int i = 0; i < maxFloors; i++)
            {
                gameObjectDatabase[i] = new Dictionary<int, GameObject>();
                creatureDatabase[i] = new List<Creature>();
                lastId[i] = 0; 
            }
        }
    }
}
