using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectRogue
{
    public class TurnHandler : GUI
    {
        public static int turnCounter
        {
            get { return GameController.turnCounter; }
            set { GameController.turnCounter = value; }
        }

        static bool lockYesNo = false;
        static Action yesAction;
        static Action noAction;

        static bool lockEnter = false;
        static Action continueAction;

        static bool runRestLoop
        {
            get { return RunRestLoop.runLoop || RunRestLoop.restLoop; }
            set 
            {
                RunRestLoop.runLoop = value;
                RunRestLoop.restLoop = value;
            }
        }

        protected override KeyMapper mapper
        {
            get { return KeyMapper.getMapper("Main Game"); }
        } 

        public override void KeyPress(KeyboardState state)
        {
            KeyMapper keyMapper = mapper;

            if (keyMapper.HasState("toggleFullscreen", state))
                GameController.mainWindow.ToggleFullscreen();

            if(lockEnter)
            {
                if(keyMapper.HasState("enter", state))
                {
                    lockEnter = false;
                    continueAction();
                }
                return;
            }

            if(lockYesNo)
            {
                runRestLoop = false;

                if(keyMapper.HasState("accept", state) || keyMapper.HasState("safeAccept", state))
                {
                    lockYesNo = false;
                    yesAction();
                }
                else if(keyMapper.HasState("decline", state) || keyMapper.HasState("safeDecline", state))
                {
                        lockYesNo = false;
                        noAction();
                }
                else
                {
                    GameLog.newMessage("[Y]es or [N]o only, please!");
                }

                return;
            }

            if (runRestLoop)
            {
                runRestLoop = false;
                return;
            }

            if(keyMapper.HasState("wizard", state) && !GameController.player.wizard)
            {
                yesNoQuestion("ENTER WIZARD-MODE?", GameController.player.enterWizardMode);
            }

            if (keyMapper.HasState("run", state))
            {
                if (!RunRestLoop.start)
                    RunRestLoop.cont = true; 

                RunRestLoop.StartExploreLoop(GameController.player, GameController.map);
                return;
            }

            if(keyMapper.HasState("rest", state))
            {
                RunRestLoop.StartRestLoop(GameController.player, GameController.map);
                return;
            }

            if (keyMapper.HasState("saveAndQuit", state))
                yesNoQuestion("Save and quit game?", this.Close);

            if(mapper.HasState("commitSuicide", state))
                yesNoQuestion("Commit suicide?", GameController.player.suicide);

            if (keyMapper.HasState("goUp", state))
            {
                UpStairTile s = GameController.map[GameController.player.x, GameController.player.y] as UpStairTile;
                if (s != null)
                {
                    GameObject.newTurn();
                    GameController.map[GameController.player.x, GameController.player.y].creature = null;
                    Player p = GameController.player;
                    GameController.player.DestroyNow();
                    GraphX.URBLINDNOW();
                    GameController.currentFloor--;
                    p.position = GameController.map.downConnection(s.connection).position;
                    GameController.player = p;
                    p.ReRegister();
                    GameController.map[GameController.player.x, GameController.player.y].creature = GameController.player;
                    GameController.player.UpdateFOV();
                    GameObject.newTurn();
                }
                else
                {
                    GameLog.newMessage("You can't go up here!");
                }
                return;
            }


            if (keyMapper.HasState("goDown", state)) //todo: set everything to wasvisible
            {
                DownStairTile s = GameController.map[GameController.player.x, GameController.player.y] as DownStairTile;
                if (s != null)
                {
                    GameObject.newTurn();
                    GameController.map[GameController.player.x, GameController.player.y].creature = null;
                    Player p = GameController.player;
                    GameController.player.DestroyNow();
                    GraphX.URBLINDNOW();
                    GameController.currentFloor++;
                    p.position = GameController.map.upConnection(s.connection).position;
                    GameController.player = p;
                    p.ReRegister();
                    GameController.map[GameController.player.x, GameController.player.y].creature = GameController.player;
                    GameController.player.UpdateFOV();
                    GameObject.newTurn();
                }
                else
                {
                    GameLog.newMessage("You can't go down here!");
                }
                return;
            }

            if(keyMapper.HasState("pickUp", state))
            {
                if(GameController.map[GameController.player.x, GameController.player.y].items.Count > 0)
                {
                    GameController.map[GameController.player.x, GameController.player.y].items[0].PickUp(GameController.player);
                    GameObject.newTurn();
                }
                else
                {
                    GameLog.newMessage("There are no items here.");
                }
            }

            if(keyMapper.HasState("lookAround", state))
            {
                GameController.currentGUI = new GUILookAround();
            }

            GameController.player.OnKeyPress(keyMapper, state);
        }

        public override void Tick()
        {
            RunRestLoop.RunLoop();
            RunRestLoop.RestLoop();
        }

        public static void yesNoQuestion(string question, Action onYes, Action onNo)
        {
            GameLog.newMessage(question + " [Y/N]", Color.Cyan);
            lockYesNo = true;
            yesAction = onYes;
            noAction = onNo;
        }

        public static void yesNoQuestion(string question, Action onYes)
        {
            GameLog.newMessage(question + " [Y/N]", Color.Cyan);
            lockYesNo = true;
            yesAction = onYes;
            noAction = okThen;
        }

        public static void waitForEnter(Action continueAction)
        {
            lockEnter = true;
            TurnHandler.continueAction = continueAction;
        }

        public static void okThen()
        {
            GameLog.newMessage("Ok, then.", Color.Cyan);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphX.Draw(spriteBatch);
        }

        protected override void OnClose()
        {
            if (GameController.save)
                GameController.SaveGame();

            GameController.save = false;
        }

        protected override void OnLoad()
        {
            if (!System.IO.File.Exists(GameController.FileName))
            {
                GameController.NewGame();
            }
            else
            {
                GameController.LoadGame();
            }
        }
    }
}

