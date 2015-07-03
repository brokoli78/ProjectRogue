using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Xna.Framework;


namespace ProjectRogue
{
    [Serializable]
    public abstract class Tile : GameObject
    {
        const string itemSeparator = ";";
        private int? creatureId = null; 
        public Creature creature
        {
            get
            {
                if (creatureId == null)
                    return null;
                return (Creature) GameObject.gameObjectDatabase[(int)creatureId];
            }
            set
            {
                if (value == null)
                {
                    creatureId = null;
                }
                else
                {
                    creatureId = value.id;
                }
            }

        }

        private List<int> itemIds = new List<int>();
        public List<Item> items
        {
            get
            {
                return itemIds.Select(i => (Item)gameObjectDatabase[i]).ToList();
            }
            set
            {
                itemIds = value.Select(i => i.id).ToList();
            }

        }

        public void AddItem(Item item)
        {
            itemIds.Add(item.id);
        }

        public void RemoveItem(Item item)
        {
            itemIds.Remove(item.id);
        }

        #region member variables

        protected bool translucent;

        public bool Translucent
        {
            get { return translucent; }
        }

        public bool visible = false;
        public bool wasVisible = false;
        public string lastDisplayString = null;
        public Color? lastDisplayStringColor = null;

        public bool explorable = true;
        private bool Walkable;
        
        public bool walkable
        {
            get { return Walkable; }
            protected set { Walkable = value; }
        }

        protected double movementCost;

        public double MovementCost
        {
            get { return movementCost; }
        }

        protected TilePaint color;

        public TilePaint paint
        {
            get { return color; }
        }

        #endregion

        protected Tile(int x, int y, TilePaint color, bool walkable, double movementCost, bool translucent)
            :base(x,y)
        {
            this.color = color;
            this.walkable = walkable;
            this.movementCost = movementCost;
            this.translucent = translucent;
        }

        public Tile()
            : base() { }

        public override void Load(Queue<string> saveStrings)
        {
            base.Load(saveStrings);
            wasVisible = Convert.ToBoolean(saveStrings.Dequeue());
        }

        public override List<string> saveString
        {
            get
            {
                List<string> s = new List<string>();
                s.AddRange(base.saveString);
                s.Add(wasVisible.ToString());
                return s;
            }
        }
    }
}
