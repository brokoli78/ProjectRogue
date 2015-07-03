using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectRogue
{
    abstract class GUIModule
    {
        public abstract void Draw(SpriteBatch spritebatch);
    }
}
