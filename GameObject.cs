using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ProjectRogue
{
    [Serializable()]
    public class GameObject
    {
        private int gameObjectId;

        public int id
        {
            get { return gameObjectId; }
            private set { gameObjectId = value; }
        }

        public static TileMap map
        {
            get { return GameController.map; }
        }

        public static Player player
        {
            get { return GameController.player; }
        }

        public static Dictionary<int, GameObject> gameObjectDatabase
        {
            get { return GameController.gameObjectDatabase; }
            set { GameController.gameObjectDatabase = value; }
        }

        public static List<Creature> creatureDatabase
        {
            get { return GameController.creatureDatabase; }
            set { GameController.creatureDatabase = value; }
        }

        static int lastId
        {
            get { return GameController.lastId; }
            set { GameController.lastId = value; }
        }
         
        static List<int> cemetery = new List<int>();
        static Dictionary<Type, Queue<string>> create = new Dictionary<Type, Queue<string>>();

        protected GameObject(int x, int y)
        {
            this.x = x;
            this.y = y;
            Register();
        }

        protected void Register()
        {
            gameObjectDatabase.Add(lastId++, (GameObject)this);
            id = lastId - 1;
            Creature c = this as Creature;
            if(c != null)
            {
                creatureDatabase.Add(c);;
            }

        }//end turn??????, after destruction

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
            set { xCoord = value; }
        }

        public int y
        {
            get { return yCoord; }
            set { yCoord = value; }
        }

        public GameObject()
        {
            Register();
        }

        public virtual List<string> saveString
        {
            get { return new List<string>{this.GetType().Name, x.ToString(), y.ToString()}; }
        }

        public virtual void Load(Queue<string> saveStrings) 
        {
            this.x = Convert.ToInt32(saveStrings.Dequeue());
            this.y = Convert.ToInt32(saveStrings.Dequeue());
        }

        /// <summary>
        /// Called at the start of the turn
        /// </summary>
        protected virtual void EarlyTurn() { }
        /// <summary>
        /// Main Turn Update, do all the stuff here
        /// </summary>
        protected virtual void Turn() { }
        /// <summary>
        /// EndTurn, before cleanup
        /// </summary>
        protected virtual void LateTurn() { }
        /// <summary>
        /// Called after Cleanup
        /// </summary>
        protected virtual void EndTurn() { }

        public void Destroy()
        {
            cemetery.Add(id);
        }

        public void DestroyNow()
        {
            int tempId = this.id;
            if (gameObjectDatabase[tempId] as Creature != null)
            {
                map[gameObjectDatabase[tempId].x, gameObjectDatabase[tempId].y].creature = null;
                creatureDatabase.Remove((Creature)gameObjectDatabase[tempId]);
            }
            gameObjectDatabase[tempId] = null;
            gameObjectDatabase.Remove(tempId);
        }

        private static void EndTurnCleanup()
        {
            foreach (int id in cemetery)
            {
                if (gameObjectDatabase[id] as Creature != null)
                {
                    map[gameObjectDatabase[id].x, gameObjectDatabase[id].y].creature = null;
                    creatureDatabase.Remove((Creature)gameObjectDatabase[id]);
                }
                gameObjectDatabase[id] = null;
                gameObjectDatabase.Remove(id);
            }

            cemetery = new List<int>();
        }

        public void Create(Type type, Queue<string> values)
        {
            create.Add(type, values);
        }

        static void CreateNew()
        {
            foreach(KeyValuePair<Type, Queue<string>> go in create)
            {
                var obj = Activator.CreateInstance(go.Key);
                ((GameObject)obj).Load(go.Value);
            }
            create = new Dictionary<Type, Queue<string>>();
        }

        public static void newTurn()
        {
            TurnHandler.turnCounter++;
            foreach(GameObject go in gameObjectDatabase.Values)
            {
                go.EarlyTurn();
            }
            foreach (GameObject go in gameObjectDatabase.Values)
            {
                go.Turn();
            }

            foreach(Creature c in creatureDatabase)
            {
                c.MoveAction();
            }

            foreach (GameObject go in gameObjectDatabase.Values)
            {
                go.LateTurn();
            }

            EndTurnCleanup();
            CreateNew();

            foreach (GameObject go in gameObjectDatabase.Values)
            {
                go.EndTurn();
            }
        }
    }
}
