using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
namespace ProjectRogue
{
    public static class Messages
    {
        public static Message enemyCloseMessage(Monster monster)
        {
            if (monster == null)
            {
                return new Message(new List<string> { "The ", " is now to close for your liking." }, new List<string> { "hugo" }, Color.Red);
            }

            return new Message(new List<string> { "The ", " is now to close for your liking." }, new List<string> { monster.name }, Color.Red);
        }

        public static Message enemiesNearby()
        {
            return new Message("There are enemies nearby!", Color.IndianRed);
        }

        public static Message hpRestored()
        {
            return new Message("HP restored.");
        }

    }
}
