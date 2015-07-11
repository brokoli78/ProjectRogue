using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectRogue
{
    public abstract class Item : GameObject
    {
        public enum ItemCreationMode { Map, Inventory}

        public ItemCreationMode creationMode;

        public string displayString = "";
        public Color displayColor;

        public Item()
            : base() { }
        
        public Item(int x, int y, string displayString, Color displayColor, ItemCreationMode creationMode)
            : base(x, y) 
        {
            this.displayString = displayString;
            this.displayColor = displayColor;
            this.creationMode = creationMode;
        }

        public override void Load(Queue<string> saveStrings)
        {
            base.Load(saveStrings);
            switch((ItemCreationMode)Convert.ToInt32(saveStrings.Dequeue()))
            {
                case ItemCreationMode.Map:
                    map[x, y].AddItem(this);
                    break;
                default:
                    break;
            }
        }

        public override List<string> saveString
        {
            get
            {
                List<string> s = new List<string>();
                s.AddRange(base.saveString);
                s.Add(((int)creationMode).ToString());
                return s;
            }
        }

        public virtual void PickUp(Creature c)
        {
            //TODO: "real" items can be picked up, generic if possible
        }

        public abstract string[] tags { get; }
    }
}
