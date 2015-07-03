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

        public static VectorFont font;
        public static VectorFont textFont;
        public static VectorFont smallFont;

        public static uint textFontHeight = 15;

        public static int sideBarOffset = 5;

        public static void UpdateVisibleArea(Player player, TileMap map)
        {
            foreach (Tile t in visibleTiles)
                map[t.x, t.y].visible = false;

            visibleTiles = player.FieldOfVision();

            foreach (Tile tile in visibleTiles)
            {
                map[tile.x, tile.y].visible = true;
                map[tile.x, tile.y].wasVisible = true;
                if (map[tile.x, tile.y].creature != null)
                {
                    map[tile.x, tile.y].lastDisplayString = map[tile.x, tile.y].creature.displayString;
                    map[tile.x, tile.y].lastDisplayStringColor = map[tile.x, tile.y].creature.displayColor;
                }
                else
                {
                    map[tile.x, tile.y].lastDisplayString = null;
                    map[tile.x, tile.y].lastDisplayStringColor = null;
                }
            }

            onScreenArea = new List<Tile>();
            for (int x = player.x - (tilesVisibleX - 1) / 2; x < player.x + (tilesVisibleX - 1) / 2 + 1; x++)
            {
                if (0 <= x && x < map.mapX)
                {
                    for (int y = player.y - (tilesVisibleY - 1) / 2; y < player.y + (tilesVisibleY - 1) / 2 + 1; y++)
                    {
                        if (0 <= y && y < map.mapY)
                        {
                            onScreenArea.Add(map[x, y]);
                        }
                    }
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Tile playerTile = GameController.map[GameController.player.x, GameController.player.y];
            if(resized)
            {
                font.SetSize((uint)tileLength);
                smallFont.SetSize((uint)tileLength / 3);
                UpdateVisibleArea(GameController.player, GameController.map);
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
                    Vector2 v = TileToCoords(new Vector2((t.x - playerTile.x + (tilesVisibleX - 1) / 2), (t.y - playerTile.y + (tilesVisibleY - 1) / 2)));
                    dummyRect = new Rectangle((int)v.x, (int)v.y, tileLength, tileLength);
                    spriteBatch.Draw(t.paint.background, dummyRect, Color.White);
                } 
            }

            drawOder++;

            // Draw the text
            foreach (Tile tile in onScreenArea)
            {
                Vector2 vec = TileToCoords(new Vector2((tile.x - playerTile.x + (tilesVisibleX - 1) / 2), (tile.y - playerTile.y + (tilesVisibleY - 1) / 2)));
                Rectangle v = new Rectangle((int)vec.x, (int)vec.y, tileLength, tileLength);

                if (tile.visible)
                {
                    if (tile.creature != null)
                    {
                        font.DrawString(spriteBatch, tile.creature.displayString, v, tile.creature.displayColor);
                    }
                    else if (tile.items.Count > 0)
                    {
                        font.DrawString(spriteBatch, tile.items[0].displayString, v, tile.items[0].displayColor);

                        if (tile.items.Count > 1)
                        {
                            v.X += 2 * tileLength / 3;
                            v.Width = tileLength / 3;
                            v.Height = tileLength / 3;

                            smallFont.DrawString(spriteBatch, "+", v, Color.Red);
                        }

                    }
                    else if (tile.paint.displayString != "")
                    {
                        font.DrawString(spriteBatch, tile.paint.displayString, v, tile.paint.displayStringColor);
                    }
                }
                else if (tile.wasVisible)
                {
                    if (tile.lastDisplayStringColor != null)
                    {
                        font.DrawString(spriteBatch, tile.lastDisplayString, v, (Color)tile.lastDisplayStringColor);
                    }
                    else if (tile.items.Count > 0)
                    {
                        font.DrawString(spriteBatch, tile.items[0].displayString, v, tile.items[0].displayColor);

                        if (tile.items.Count > 1)
                        {
                            v.X += 2 * tileLength / 3;
                            v.Width = tileLength / 3;
                            v.Height = tileLength / 3;

                            smallFont.DrawString(spriteBatch, "+", v, Color.Red);
                        }
                    }
                    else if (tile.paint.displayString != "")
                    {
                        font.DrawString(spriteBatch, tile.paint.displayString, v, tile.paint.displayStringColor);
                    }
                }
            }

            drawOder++;

            foreach(Tile tile in onScreenArea)
            {
                if(tile.wasVisible && !tile.visible)
                {
                    Vector2 v = TileToCoords(new Vector2((tile.x - playerTile.x + (tilesVisibleX - 1) / 2), (tile.y - playerTile.y + (tilesVisibleY - 1) / 2)));
                    dummyRect = new Rectangle((int)v.x, (int)v.y, tileLength, tileLength);
                    spriteBatch.Draw(shadow, dummyRect, Color.White);
                }
            }            
        }

        static private Vector2 TileToCoords(Vector2 t)
        {
            return new Vector2((int)(t.x * tileLength), (int)(t.y * tileLength));
        }
    }
}
