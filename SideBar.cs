using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectRogue
{
    public static class SideBar
    {

        public static Texture2D green;
        public static Texture2D grey;
        static int currentLine = 0;
        static Rectangle bounds;
        static int height;

        public static void DrawSideBar(SpriteBatch spriteBatch, Rectangle rect)
        {
            bounds = rect;
            currentLine = 0;
            height =  (int)GraphX.textFontHeight + 3;

            //draw headline
            DrawNextLine(spriteBatch, GameController.player.name + ", a warrior of Trog", Color.Gray);

            currentLine++;

            DrawNextLine(spriteBatch, "HP: " + (int) MyMath.Clamp(GameController.player.health, 0, GameController.player.maxHealth) + "/" + GameController.player.maxHealth, Color.GhostWhite);

            int healthWidth = (int) ((rect.Width) * ((double) MyMath.Clamp(GameController.player.health, 0, GameController.player.maxHealth) / (double)GameController.player.maxHealth));
            Rectangle health = new Rectangle(rect.X, rect.Y + currentLine * height, healthWidth, (int)GraphX.textFontHeight);
            spriteBatch.Draw(green, health, Color.White);

            Rectangle notHealth = new Rectangle(rect.X + healthWidth, rect.Y + currentLine * height, rect.Width - healthWidth, (int)GraphX.textFontHeight);
            spriteBatch.Draw(grey, notHealth, Color.White);
            currentLine++;

            DrawNextLine(spriteBatch, "Turn: " + TurnHandler.turnCounter, Color.GhostWhite);

            DrawNextLine(spriteBatch, "Floor: " + GameController.currentFloor, Color.GhostWhite);

            DrawNextLine(spriteBatch, "Gold: " + GameController.player.gold, Color.GhostWhite);

            currentLine++;

            //draw creature hints
            if (GameController.player.monsters.Count > 0)
            {
                foreach (List<Monster> monsters in GameController.player.monsters)
                {
                    Monster monster = monsters[0];
                    DrawNextLine(spriteBatch, monster.displayString + ": a " + monster.name, monster.displayColor);
                }
            }

            if (GameController.player.wizard)
            {
                Vector2 pos = new Vector2(rect.X, rect.Y + rect.Height - 2 * height);
                GraphX.textFont.DrawString(spriteBatch, "*WIZARD*", pos, Color.DeepPink);
            }

        }           

        static void DrawNextLine(SpriteBatch spriteBatch, string text, Color color)
        {
            Vector2 pos = new Vector2(bounds.X, bounds.Y + height * currentLine);
            GraphX.textFont.DrawString(spriteBatch, text, pos, color);
            currentLine++;
        }
    }
}
