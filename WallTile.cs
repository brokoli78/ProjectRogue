using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectRogue
{
    [Serializable]
    public class WallTile : Tile
    {
        public WallTile()
            : base() { }

        public WallTile(int x, int y)
            : base(x, y, TilePaint.wall, false, double.PositiveInfinity, false)
        {
            
        }

        public override void Load(Queue<string> saveStrings)
        {
            base.Load(saveStrings);
            color = TilePaint.wall;
            walkable = false;
            movementCost = double.PositiveInfinity;
            translucent = false;
        }
    }
}
