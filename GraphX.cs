using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProjectRogue
{
    public static class GraphX
    {
        public static bool resized = false;
        public static readonly int tilesVisibleY = 21;
        public static int tilesVisibleX;
        public static int tileLength;
        public static readonly int maxMessageBoxHeight = 200;
        public static int messageBoxHeight;
        public static readonly int maxSideBarWidth = 300;
        public static int sideBarWidth;

        static List<Tile> onScreenArea;
        static List<Tile> visibleTiles = new List<Tile>();
        public static Texture2D shadow;

        public static Texture2D autopickup;

        public static VectorFont font;
        public static VectorFont textFont;
        public static VectorFont smallFont;

        public static ushort textFontHeight = 15;
        public static ushort textFontAdvance;

        public static int sideBarOffset = 5;

        static Tile centerTile;

        public static void URBLINDNOW()
        {
            foreach (Tile t in visibleTiles)
                GameController.map[t.x, t.y].visible = false;
        }

        public static void UpdateVisibleArea()
        {
            URBLINDNOW();

            visibleTiles = GameController.player.FieldOfVision();

            foreach (Tile tile in visibleTiles)
            {
                GameController.map[tile.x, tile.y].visible = true;
                GameController.map[tile.x, tile.y].wasVisible = true;
                if (GameController.map[tile.x, tile.y].creature != null)
                {
                    if(tile.x != GameController.player.x && tile.y != GameController.player.y)
                    {
                        GameController.map[tile.x, tile.y].lastDisplayString = GameController.map[tile.x, tile.y].creature.displayString;
                        GameController.map[tile.x, tile.y].lastDisplayStringColor = GameController.map[tile.x, tile.y].creature.displayColor;
                    }
                }
                else
                {
                    GameController.map[tile.x, tile.y].lastDisplayString = "";
                    GameController.map[tile.x, tile.y].lastDisplayStringColor = Color.Transparent;
                }
            }

            UpdateOnScreenArea(GameController.map[GameController.player.x, GameController.player.y]);

        }


        public static void UpdateOnScreenArea(Tile centerTile)
        {
            GraphX.centerTile = centerTile;
            onScreenArea = new List<Tile>();
            for (int x = centerTile.x - (tilesVisibleX - 1) / 2; x < centerTile.x + (tilesVisibleX - 1) / 2 + 1; x++)
            {
                if (0 <= x && x < GameController.map.mapX)
                {
                    for (int y = centerTile.y - (tilesVisibleY - 1) / 2; y < centerTile.y + (tilesVisibleY - 1) / 2 + 1; y++)
                    {
                        if (0 <= y && y < GameController.map.mapY)
                        {
                            onScreenArea.Add(GameController.map[x, y]);
                        }
                    }
                }
            }

        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if(resized)
            {
                font.SetSize((uint)tileLength);
                smallFont.SetSize((uint)tileLength / 3);
                UpdateVisibleArea();
                resized = false;
            }

            // Draw GameLog
            Rectangle gameLog = new Rectangle(0, tileLength * tilesVisibleY, tilesVisibleX * tileLength, messageBoxHeight);
            GameLog.DrawLog(spriteBatch, gameLog);

            //Draw SideBar
            Rectangle sideBar = new Rectangle(tileLength * tilesVisibleX + sideBarOffset, 0, sideBarWidth, tilesVisibleY * tileLength + messageBoxHeight);
            SideBar.DrawSideBar(spriteBatch, sideBar);

            Dictionary<TilePaint, List<Rectangle>> recsToDraw = new Dictionary<TilePaint, List<Rectangle>>();

            Rectangle dummyRect;

            int drawOder = 0;
 
            foreach (Tile t in onScreenArea)
            {
                if (t.wasVisible)
                {
                    Vector2 v = TileToCoords(new Vector2((t.x - centerTile.x + (tilesVisibleX - 1) / 2), (t.y - centerTile.y + (tilesVisibleY - 1) / 2)));
                    dummyRect = new Rectangle((int)v.x, (int)v.y, tileLength, tileLength);
                    spriteBatch.Draw(t.paint.background, dummyRect, Color.White);
                } 
            }

            drawOder++;

            // Draw the text
            foreach (Tile tile in onScreenArea)
            {
                Vector2 vec = TileToCoords(new Vector2((tile.x - centerTile.x + (tilesVisibleX - 1) / 2), (tile.y - centerTile.y + (tilesVisibleY - 1) / 2)));
                Rectangle v = new Rectangle((int)vec.x, (int)vec.y, tileLength, tileLength);

                if (tile.visible)
                {
                    if (tile.creature != null)
                    {
                        font.DrawCharCentered(spriteBatch, tile.creature.displayString, v, tile.creature.displayColor);
                    }
                    else if (tile.items.Count > 0)
                    {
                        font.DrawCharCentered(spriteBatch, tile.items[0].displayString, v, tile.items[0].displayColor);

                        if (tile.items.Where(i => i.tags.Contains("autopickup")).Count() > 0)
                            spriteBatch.Draw(autopickup, vec, Color.White);

                        if (tile.items.Count > 1)
                        {
                            v.X += 2 * tileLength / 3;
                            v.Width = tileLength / 3;
                            v.Height = tileLength / 3;

                            smallFont.DrawCharCentered(spriteBatch, "+", v, Color.Red);
                        }
                    }
                    else if (tile.paint.displayString != "")
                    {
                        font.DrawCharCentered(spriteBatch, tile.paint.displayString, v, tile.paint.displayStringColor);
                    }
                }
                else if (tile.wasVisible)
                {
                    if (tile.lastDisplayString != "")
                    {
                        font.DrawCharCentered(spriteBatch, tile.lastDisplayString, v, (Color)tile.lastDisplayStringColor);
                    }
                    else if (tile.items.Count > 0)
                    {
                        font.DrawCharCentered(spriteBatch, tile.items[0].displayString, v, tile.items[0].displayColor);

                        if (tile.items.Where(i => i.tags.Contains("autopickup")).Count() > 0)
                            spriteBatch.Draw(autopickup, vec, Color.White);

                        if (tile.items.Count > 1)
                        {
                            v.X += 2 * tileLength / 3;
                            v.Width = tileLength / 3;
                            v.Height = tileLength / 3;

                            smallFont.DrawCharCentered(spriteBatch, "+", v, Color.Red);
                        }
                    }
                    else if (tile.paint.displayString != "")
                    {
                        font.DrawCharCentered(spriteBatch, tile.paint.displayString, v, tile.paint.displayStringColor);
                    }
                }
            }

            drawOder++;

            foreach(Tile tile in onScreenArea)
            {
                if(tile.wasVisible && !tile.visible)
                {
                    Vector2 v = TileToCoords(new Vector2((tile.x - centerTile.x + (tilesVisibleX - 1) / 2), (tile.y - centerTile.y + (tilesVisibleY - 1) / 2)));
                    dummyRect = new Rectangle((int)v.x, (int)v.y, tileLength, tileLength);
                    spriteBatch.Draw(shadow, dummyRect, Color.White);
                }
            }            
        }

        public static Vector2 TileToCoords(Vector2 t)
        {
            return new Vector2((int)(t.x * tileLength), (int)(t.y * tileLength));
        }
    }
}
