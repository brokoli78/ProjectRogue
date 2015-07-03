using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectRogue
{
    [Serializable]
    public class FloorTile : Tile
    {
        public FloorTile()
            : base() { }

        public FloorTile(int x, int y)
            : base(x, y, TilePaint.floor, true, 1D, true)
        {
        }

        public override void Load(Queue<string> saveStrings)
        {
            base.Load(saveStrings);
            color = TilePaint.floor;
            walkable = true;
            movementCost = 1D;
            translucent = true;
        }

    }
}
