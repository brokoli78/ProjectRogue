using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ProjectRogue
{
    public static class Dice
    {
        static Random r = new Random();
        /// <summary>
        /// Rolls a number of dice
        /// </summary>
        /// <param name="num">Number of dice</param>
        /// <param name="dice">Max die value</param>
        /// <returns></returns>
        public static int d(int num, int dice)
        {
            int current = 0;
            for (int i = 0; i < num; i++)
            {
                current += r.Next(0, dice) + 1;
            }
            return current;
        }
    }
}
