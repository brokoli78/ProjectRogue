using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectRogue
{
    public class UpStairTile : Tile
    {
        public int connection;

        public UpStairTile()
            : base() { }

        public UpStairTile(int x, int y, int connection)
            : base(x, y, TilePaint.upStair, true, 1, true)
        {
            this.connection = connection;
        }

        public override void Load(Queue<string> saveStrings)
        {
            base.Load(saveStrings);
            color = TilePaint.upStair;
            walkable = true;
            movementCost = 1;
            translucent = true;
            connection = Convert.ToInt32(saveStrings.Dequeue());
        }

        public override List<string> saveString
        {
            get
            {
                List<string> s = new List<string>();
                s.AddRange(base.saveString);
                s.Add(connection.ToString());
                return s;
            }
        }

    }
}
