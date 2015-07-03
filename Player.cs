using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;



namespace ProjectRogue
{
    [Serializable]
    public class Player : Creature
    {
        public bool wizard = false;
        static bool delia = false;
        static int trogRage = 0;
        private List<List<Monster>> Monsters = new List<List<Monster>>();

        public Player()
            : base() { }

        public Player(int x, int y, int visionRadius, string name)
            : base(x, y, name, visionRadius, 10, 0.25, "@", Color.Red, Factions.player) { }

        public override void Load(Queue<string> saveStrings)
        {
            base.Load(saveStrings);
            wizard = Convert.ToBoolean(saveStrings.Dequeue());
            delia = Convert.ToBoolean(saveStrings.Dequeue());
            trogRage = Convert.ToInt32(saveStrings.Dequeue());
            GameController.player = this; 
        }

        public override List<string> saveString
        {
            get
            {
                List<string> s = new List<string>();
                s.AddRange(base.saveString);
                s.Add(wizard.ToString());
                s.Add(delia.ToString());
                s.Add(trogRage.ToString());
                return s;
            }
        }

        public override void die()
        {
            if (kill)
            {
                GameLog.newMessage("You die...");
                GameController.save = false;
                TurnHandler.waitForEnter(GameController.currentGUI.Close);
            }
            else
            {
                TurnHandler.yesNoQuestion("Do you really want to die?", yesDie, dontDie);
            }
        }

        public void yesDie()
        {  
            kill = true;
            die();
            return;
        }

        public void dontDie()
        {
            GameLog.newMessage("Thought so.");
            this.health = this.maxHealth;
            return;
        }

        public override bool AttackAction()
        {
            if (attacking)
            {
                Creature c = map[xTo, yTo].creature;
                if (c != null)
                {
                    if (c.hit(1 + toHit))
                    {
                        GameLog.newMessage("You hit the " + c.name + ".");
                        bool ded = c.takeDamage(this.dealMeleeDamage());
                        if (ded)
                        {
                            GameLog.newMessage("You kill the " + c.name + "!", Color.Red);
                        }
                        else
                        {
                            if(c.health == (double)c.maxHealth)
                            {
                                GameLog.newMessage("The " + c.name + " appears to be unharmed.");
                            }
                            else if(c.health >= 0.8f * (double) c.maxHealth)
                            {
                                GameLog.newMessage("The " + c.name + " is lightly wounded.", Color.LightCyan);
                            }
                            else if (c.health >= 0.6f * (double)c.maxHealth)
                            {
                                GameLog.newMessage("The " + c.name + " is moderately wounded.", Color.White);
                            }
                            else if (c.health >= 0.4f * (double)c.maxHealth)
                            {
                                GameLog.newMessage("The " + c.name + " is heavily wounded.", Color.Yellow);
                            }
                            else if (c.health >= 0.2f * (double)c.maxHealth)
                            {
                                GameLog.newMessage("The " + c.name + " is severely wounded.", Color.Red);
                            }
                            else
                            {
                                GameLog.newMessage("The " + c.name + " is almost dead.", Color.DarkRed);
                            }
                        }

                        return ded;
                    }
                    else
                    {
                        GameLog.newMessage("You miss the " + c.name + ".");
                    }
                }
            }

            return false;
        }

        protected override void EndTurn()
        {
            GraphX.UpdateVisibleArea(this, map);
            base.EndTurn();
        }

        protected override int dealMeleeDamage()
        {
            return Dice.d(2, 3);
        }

        public bool OnKeyPress(KeyMapper keyMapper, KeyboardState state)
        {
            int tempX = x;
            int tempY = y;
            bool walked = false;

            if(keyMapper.HasState("moveSW", state))
            {
                tempX--;
                tempY++;
                walked = true;
            }
            else if(keyMapper.HasState("moveS", state))
            {
                tempY++;
                walked = true;
            }
            else if(keyMapper.HasState("moveSE", state))
            {
                tempX++;
                tempY++;
                walked = true;
            }
            else if(keyMapper.HasState("moveW", state))
            {
                tempX--;
                walked = true;
            }
            else if(keyMapper.HasState("wait1", state))
            {
                walked = true;
            }
            else if(keyMapper.HasState("moveE", state))
            {
                tempX++;
                walked = true;
            }
            else if(keyMapper.HasState("moveNW", state))
            {
                tempX--;
                tempY--;
                walked = true;
            }
            else if(keyMapper.HasState("moveN", state))
            {
                tempY--;
                walked = true;
            }
            else if(keyMapper.HasState("moveNE", state))
            {
                tempX++;
                tempY--;
                walked = true;
            }
            else
            {
                walked = false;
            }

            if(walked)
            {
                if (map.inMap(tempX, tempY))
                {
                    if (map[tempX, tempY].walkable)
                    {
                        move(tempX, tempY);
                        GameObject.newTurn();
                        return true;
                    }
                    else
                    {
                        if (delia)
                        {
                            if (trogRage > 10)
                            {
                                GameLog.newMessage("You bump into the wall. Nothing happens.");
                                GameLog.newMessage("All nearby enemies in the dungeon spend a turn laughing at your stupidity.", Color.LightBlue);
                            }
                            if (trogRage == 10)
                            {
                                GameLog.newMessage("You bump into the wall. You explode and die.");
                                GameLog.newMessage("Trog intervenes to save you. He says:", Color.Gold);
                                GameLog.newMessage(" I'm tired of saving you....therefore im taking all the explosions from you.", Color.Goldenrod);
                                GameLog.newMessage(" MUHAHAHAHAHAHAHAHAH", Color.Red);
                                trogRage++;
                            }
                            if (trogRage < 10)
                            {
                                GameLog.newMessage("You bump into the wall. You explode and die.");
                                GameLog.newMessage("Trog intervenes to save you. His advice:", Color.Gold);
                                GameLog.newMessage(" Maybe next time you don't do that. Reverting time.....", Color.Goldenrod);
                                trogRage++;
                            }
                        }
                    }
                }

            }
            return false;
        }

        public override List<Tile> FieldOfVision()
        {
            List<Tile> tempFov = base.FieldOfVision();
            Monsters = tempFov.Where(t => t.creature as Monster != null).Select(t => (Monster)t.creature).GroupBy(m => m.name).Select(grp => grp.ToList()).ToList();
            return tempFov;
        }

        public List<List<Monster>> monsters
        {
            get
            {
                FieldOfVision();
                return Monsters;
            }

        }

        public void enterWizardMode()
        {
            kill = false;
            wizard = true;
        }

        public void suicide()
        {
            kill = true;
            die();
        }

        public void ReRegister()
        {
            this.Register();
        }

    }
}
