using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectRogue
{
    public abstract class GUIModule
    {
        protected Func<Rectangle> drawingRectangle;

        protected GUIModule(Func<Rectangle> drawingRectangle)
        {
            this.drawingRectangle = drawingRectangle;
        }

        public abstract void Draw(SpriteBatch spritebatch);

        public virtual void KeyPress(KeyboardState state, KeyMapper mapper) { }
    }
}
