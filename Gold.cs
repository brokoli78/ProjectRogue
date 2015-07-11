using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectRogue
{
    public class Gold : Item
    {
        int value;

        public Gold()
            : base() { }

        public Gold(int x, int y, int value)
            : base(x, y, "$", Color.Gold, ItemCreationMode.Map) 
        {
            this.value = value;
        }

        public override void Load(Queue<string> saveStrings)
        {
            base.Load(saveStrings);
            this.displayString = "$";
            this.displayColor = Color.Gold;
            this.value = Convert.ToInt32(saveStrings.Dequeue());
        }

        public override List<string> saveString
        {
            get
            {
                List<string> s = base.saveString;
                s.Add(value.ToString());
                return s;
            }
        }

        public override void PickUp(Creature c)
        {
            c.gold += value;

            if (c as Player != null)
                GameLog.newMessage("You now have " + c.gold + " gold pieces (gained " + value + ").");

            map[x, y].RemoveItem(this);
            this.Destroy();
        }

        public override string[] tags
        {
            get
            {
                return new string[]
                {
                    "gold",
                    "autopickup"
                };
            }
        }
    }
}
