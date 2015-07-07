using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProjectRogue
{
    public class GUITextbox :GUIModule
    {
        string text;
        Color color;
        ushort lineGap;

        public GUITextbox(string text, Func<Rectangle> textBox, ushort lineGap, Color? color = null)
            :base (textBox)
        {
            this.color = color ?? Color.GhostWhite;
            this.lineGap = lineGap;
            this.text = text;
        }

        public void changeColor(Color color)
        {
            this.color = color;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            GraphX.textFont.DrawString(spritebatch, text, drawingRectangle(), color, lineGap);
        }
    }

}
