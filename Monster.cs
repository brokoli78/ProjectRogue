using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace ProjectRogue
{
    [Serializable]
    public abstract class Monster : Creature
    {
        protected bool sleeping;
        protected List<Tile> path = new List<Tile>();

        public Monster()
            : base() { }

        protected Monster(int x, int y, string name, int visionRadius, int maxHealth, double hpRegenRate, string displayString, Color displayColor, bool sleeping)
            :base(x, y, name, visionRadius, maxHealth, hpRegenRate, displayString, displayColor, Factions.monster)
        {
            this.sleeping = sleeping;
        }

        public override void Load(Queue<string> saveStrings)
        {
            base.Load(saveStrings);
            sleeping = Convert.ToBoolean(saveStrings.Dequeue());
        }

        public override List<string> saveString
        {
            get
            {
                List<string> s = new List<string>();
                s.AddRange(base.saveString);
                s.Add(sleeping.ToString());
                return s;
            }
        }

        protected override void Turn()
        {
            if(FieldOfVision().Contains(map[player.x, player.y]))
            {
                if(AStarMonster.CalculatePath(map, map[x,y], map[player.x, player.y], out path) && path.Count < 7)
                {
                    move(path.Last().x, path.Last().y);
                }
                else if (AStar.CalculatePath(map, map[x, y], map[player.x, player.y], out path, false))
                {
                    move(path.Last().x, path.Last().y);
                }

            }
            else if (path.Count > 0 && path.First().position != this.position && path.First().GetType() != typeof(UpStairTile) && path.First().GetType() != typeof(DownStairTile))
            {
                List<Tile> tempPath = new List<Tile>();
                tempPath.AddRange(path);

                if (AStarMonster.CalculatePath(map, map[x, y], map[tempPath.First().x, tempPath.First().y], out path) && path.Count < 10)
                {
                    move(path.Last().x, path.Last().y);
                }
                else if (AStar.CalculatePath(map, map[x, y], map[tempPath.First().x, tempPath.First().y], out path, false))
                {
                    move(path.Last().x, path.Last().y);
                }
            }
        }
    }
}
