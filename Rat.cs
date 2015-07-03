using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
namespace ProjectRogue
{
    [Serializable]
    public class Rat : Monster
    {
        public Rat(int x, int y)
            :base(x, y, "rat", 10, 5, 0.1, "r", Color.Brown, false) 
        {
            gold = 1;
        }

        public override void Load(Queue<string> saveStrings)
        {
            base.Load(saveStrings);
            gold = 1;
        }
        
        public Rat()
            : base() { }

        protected override int dealMeleeDamage()
        {
            return Dice.d(1, 2);
        }

    }
}
