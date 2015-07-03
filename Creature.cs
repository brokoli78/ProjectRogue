using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace ProjectRogue
{
    [Serializable]
    public abstract class Creature : GameObject
    {
        public enum Factions { player, monster };
        public double hpRegenRate;

        public int gold = 0;

        public Factions faction;
        public int dodge = 1;
        public int toHit = 1;

        public string name;
        public int maxHealth;

        public string displayString;
        public Color displayColor;

        private double hp;

        private int xFrom;
        private int yFrom;

        protected int xTo;
        protected int yTo;

        bool walking = false;
        protected bool attacking = false;

        public int visionRadius;

        protected bool kill = true;

        public double health
        {
            get { return hp; }
            protected set { hp = value; }
        }

        public Creature(int x, int y, string name, int visionRadius, int maxHealth, double hpRegenRate, string displayString, Color displayColor, Factions faction)
            :base(x, y)
        {
            this.name = name;
            this.visionRadius = visionRadius;
            this.maxHealth = maxHealth;
            this.health = maxHealth;
            this.hpRegenRate = hpRegenRate;
            this.displayString = displayString;
            this.displayColor = displayColor;
            map[x, y].creature = this;
            this.faction = faction;
        }

        public Creature()
            : base() { }

        public override void Load(Queue<string> saveStrings)
        {
            base.Load(saveStrings);
            this.name = saveStrings.Dequeue();
            this.visionRadius = Convert.ToInt32(saveStrings.Dequeue());
            this.maxHealth = Convert.ToInt32(saveStrings.Dequeue());
            this.health = Convert.ToDouble(saveStrings.Dequeue());
            this.hpRegenRate = Convert.ToDouble(saveStrings.Dequeue());
            this.displayString = saveStrings.Dequeue();
            this.displayColor = new Color() { PackedValue = Convert.ToUInt32(saveStrings.Dequeue()) };
            map[x, y].creature = this;
            this.faction = (Factions)Enum.Parse(typeof(Factions),saveStrings.Dequeue());
            this.gold = Convert.ToInt32(saveStrings.Dequeue());
        }

        public override List<string> saveString
        {
            get
            {
                List<string> s = new List<string>();
                s.AddRange(base.saveString);
                s.Add(name);
                s.Add(visionRadius.ToString());
                s.Add(maxHealth.ToString());
                s.Add(health.ToString());
                s.Add(hpRegenRate.ToString());
                s.Add(displayString);
                s.Add(displayColor.PackedValue.ToString());
                s.Add(faction.ToString());
                s.Add(gold.ToString());
                return s;
            }
        }

        public bool hit(int accuracy)
        {
            if (Dice.d(1, accuracy) > Dice.d(1, this.dodge))
                return true;
            return false;
        }

        protected virtual void attack(Creature creature)
        {
            xTo = creature.x;
            yTo = creature.y;
            attacking = true;
        }
        
        public virtual void move(int x, int y)
        {
            attacking = true;
            xFrom = this.x;
            yFrom = this.y;
            xTo = x;
            yTo = y;
            walking = true;
        }

        public virtual void die()
        {
            if (kill)
            {
                if(gold > 0)
                {
                    Queue<string> s = new Queue<string>();
                    s.Enqueue(x.ToString());
                    s.Enqueue(y.ToString());
                    s.Enqueue(((int)Item.ItemCreationMode.Map).ToString());
                    s.Enqueue(gold.ToString());
                    Create(typeof(Gold), s);
                }
                Destroy();
            }
        }

        public virtual bool takeDamage(double damage)
        {
            if (damage < 0)
                throw new ArgumentOutOfRangeException("damage");

            health -= damage;

            if (kill && health <= 0)
                return true;
            return false;
        }

        public virtual void heal(double health)
        {
            if (health < 0)
                throw new ArgumentOutOfRangeException("health");

            this.health += health;

            if(this.health > maxHealth)
            {
                this.health = maxHealth;
            }
            //status effects
        }

        protected override void Turn()
        {
            heal(hpRegenRate);
        }

        protected override void LateTurn()
        {
            if (health <= 0)
                die();
        }

        private List<Tile> fov = null;

        public virtual List<Tile> FieldOfVision()
        {
            if (fov == null)
                UpdateFOV();
            return fov;
        }

        public void UpdateFOV()
        {
            fov = FOV.FieldOfVision(map[x, y], visionRadius, map);
        }

        public virtual bool AttackAction()
        {
            if(attacking)
            {
                Creature c = map[xTo, yTo].creature;
                if(c != null && c.faction != this.faction)
                {
                    if (c.hit(1 + toHit))
                    {
                        if (c == GameController.player)
                            GameLog.newMessage("The " + this.name + " hits you.", Color.White);
                        return c.takeDamage(this.dealMeleeDamage());
                    }
                    else
                    {
                        if (c == GameController.player)
                            GameLog.newMessage("The " + this.name + " misses you.");
                    }
                }
            }

            return false;
        }

        protected abstract int dealMeleeDamage();

        public virtual bool MoveAction()
        {
            if(walking)
            {
                walking = false;

                if (map[xTo, yTo].creature != null && map[xTo, yTo].creature != this)
                {
                    if (this.AttackAction())
                    {
                        return false;
                    }
                    if(map[xTo, yTo].creature.MoveAction())
                    {
                        map[xFrom, yFrom].creature = null;
                        map[xTo, yTo].creature = this;
                        x = xTo;
                        y = yTo;
                        UpdateFOV();
                        return true;
                    }
                }
                else
                {
                        map[xFrom, yFrom].creature = null;
                        map[xTo, yTo].creature = this;
                        x = xTo;
                        y = yTo;
                        UpdateFOV();
                        return true;
                }
            }

            return false;
        }
    }
}
