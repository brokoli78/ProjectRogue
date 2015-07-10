using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectRogue
{
    public class GUILookAround : GUI
    {

        int currentX, currentY;

        public static Texture2D rectangle;

        protected override KeyMapper mapper
        {
            get { return KeyMapper.getMapper("Main Game"); }
        }

        public GUILookAround()
        {
            currentX = GameController.player.x;
            currentY = GameController.player.y;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphX.Draw(spriteBatch);

            int posx = (GraphX.tilesVisibleX - 1) / 2;
            int posy = (GraphX.tilesVisibleY - 1) / 2;

            spriteBatch.Draw(rectangle, GraphX.TileToCoords(new Vector2(posx, posy)), Color.White);
        }

        List<List<Tile>> pathsUp;
        List<List<Tile>> pathsDown;

        public override void KeyPress(KeyboardState state)
        {
            int tempX = currentX;
            int tempY = currentY;
            if(mapper.HasState("escape", state) || mapper.HasState("lookAround", state))
            {
                GameLog.newMessage("Returning to the game...");
                this.Close();
                return;
            }
            else if (mapper.HasState("moveSW", state))
            {
                tempX--;
                tempY++;
            }
            else if (mapper.HasState("moveS", state))
            {
                tempY++;
            }
            else if (mapper.HasState("moveSE", state))
            {
                tempX++;
                tempY++;
            }
            else if (mapper.HasState("moveW", state))
            {
                tempX--;
            }
            else if (mapper.HasState("moveE", state))
            {
                tempX++;
            }
            else if (mapper.HasState("moveNW", state))
            {
                tempX--;
                tempY--;
            }
            else if (mapper.HasState("moveN", state))
            {
                tempY--;
            }
            else if (mapper.HasState("moveNE", state))
            {
                tempX++;
                tempY--;
            }
            else if (mapper.HasState("bigMoveSW", state))
            {
                tempX -= 7;
                tempY += 7;
            }
            else if (mapper.HasState("bigMoveS", state))
            {
                tempY += 7;
            }
            else if (mapper.HasState("bigMoveSE", state))
            {
                tempX += 7;
                tempY += 7;
            }
            else if (mapper.HasState("bigMoveW", state))
            {
                tempX -= 7;
            }
            else if (mapper.HasState("bigMoveE", state))
            {
                tempX++;
            }
            else if (mapper.HasState("bigMoveNW", state))
            {
                tempX -= 7;
                tempY -= 7;
            }
            else if (mapper.HasState("bigMoveN", state))
            {
                tempY -= 7;
            }
            else if (mapper.HasState("bigMoveNE", state))
            {
                tempX += 7;
                tempY -= 7;
            }
            else if(mapper.HasState("goDown", state))
            {
                if (pathsDown == null)
                {
                    pathsDown = new List<List<Tile>>();

                    for (int i = 0; i < GameController.map.downConnectionCount(); i++)
                    {
                        List<Tile> temp = new List<Tile>();

                        if (AStar.CalculatePath(GameController.map, GameController.map[GameController.player.x, GameController.player.y], GameController.map.downConnection(i), out temp, true))
                        {
                            if (temp.Count == 0)
                                temp = new List<Tile> { GameController.map[GameController.player.x, GameController.player.y] };
                            pathsDown.Add(temp);
                        }
                    }

                    pathsDown = pathsDown.OrderBy(x => x.Count).ToList();

                }

                if (pathsDown.Count > 0)
                {
                    int currentConnection = pathsDown.Count - 1;

                    for (int i = 0; i < pathsDown.Count; i++)
                    {
                        if (pathsDown[i].First() == GameController.map[currentX, currentY])
                        {
                            currentConnection = i;
                            break;
                        }
                    }

                    currentConnection++;

                    if (currentConnection >= pathsDown.Count)
                        currentConnection = 0;

                    tempX = pathsDown[currentConnection].First().x;
                    tempY = pathsDown[currentConnection].First().y;
                }
            }
            else if (mapper.HasState("goUp", state))
            {
                if (pathsUp == null)
                {
                    pathsUp = new List<List<Tile>>();

                    for (int i = 0; i < GameController.map.upConnectionCount(); i++)
                    {
                        List<Tile> temp = new List<Tile>();

                        if (AStar.CalculatePath(GameController.map, GameController.map[GameController.player.x, GameController.player.y], GameController.map.upConnection(i), out temp, true))
                        {
                            if (temp.Count == 0)
                                temp = new List<Tile> { GameController.map[GameController.player.x, GameController.player.y] };
                            pathsUp.Add(temp);
                        }
                    }

                    pathsUp = pathsUp.OrderBy(x => x.Count).ToList();

                }

                if (pathsUp.Count > 0)
                {
                    int currentConnection = pathsUp.Count - 1;

                    for (int i = 0; i < pathsUp.Count; i++)
                    {
                        if (pathsUp[i].First() == GameController.map[currentX, currentY])
                        {
                            currentConnection = i;
                            break;
                        }
                    }

                    currentConnection++;

                    if (currentConnection >= pathsUp.Count)
                        currentConnection = 0;

                    tempX = pathsUp[currentConnection].First().x;
                    tempY = pathsUp[currentConnection].First().y;
                }
            }
            else if(mapper.HasState("enter", state))
            {
                List<Tile> temp = new List<Tile>();

                if (AStar.CalculatePath(GameController.map, GameController.map[GameController.player.x, GameController.player.y], GameController.map[currentX, currentY], out temp, true) && temp.Count > 0)
                {
                    RunRestLoop.StartRunLoop(temp);
                }
                if(temp.Count == 0)
                {
                    GameLog.newMessage("I don't know how to get there!!", Color.DarkRed);
                }
                else
                {
                    this.Close();
                    return;
                }
            }

            currentX = MyMath.Clamp(tempX, 0, GameController.mapX);
            currentY = MyMath.Clamp(tempY, 0, GameController.mapY);
            GraphX.UpdateOnScreenArea(GameController.map[currentX, currentY]);
        }

        protected override void OnClose()
        {
            GraphX.UpdateOnScreenArea(GameController.map[GameController.player.x, GameController.player.y]);
            base.OnClose();
        }
    }
}
