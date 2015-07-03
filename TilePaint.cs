using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectRogue
{
    [Serializable]
    public class TilePaint
    {
        public static List<TilePaint> tilePaints = new List<TilePaint>();

        Color color;
        public Texture2D background;
        public string displayString;
        public Color displayStringColor;

        void Register()
        {
            if (!tilePaints.Contains(this))
                tilePaints.Add(this);
        }

        TilePaint(Color color)
            : this(color, "", color) { }

        TilePaint(Color color, string displayString, Color displayStringColor)
        {
            this.color = color;
            this.displayString = displayString;
            this.displayStringColor = displayStringColor;
            Register();
        }

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            if(color != null)
            {
                background = new Texture2D(graphicsDevice, 1, 1);
                background.SetData(new Color[] { color });
            }
        }

        public static TilePaint wall = new TilePaint(Color.DimGray);
        public static TilePaint floor = new TilePaint(Color.LightGray);
        public static TilePaint upStair = new TilePaint(floor.color, "<", wall.color);
        public static TilePaint downStair = new TilePaint(floor.color, ">", wall.color);

        public static bool operator ==(TilePaint paint1, TilePaint paint2)
        {
            if (Object.ReferenceEquals(paint1, paint2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)paint1 == null) || ((object)paint2 == null))
            {
                return false;
            }
            return paint1.color.PackedValue == paint2.color.PackedValue && paint1.displayString == paint2.displayString && paint1.displayStringColor == paint2.displayStringColor;
        }

        public static bool operator !=(TilePaint paint1, TilePaint paint2)
        {
            return !(paint1 == paint2);
        }

        public override bool Equals(object obj)
        {
            TilePaint paint = obj as TilePaint;
            if ((object)paint == null)
                return false;
            return this == paint;
        }

        public override int GetHashCode()
        {
            return 1;
        }

    }      
}
