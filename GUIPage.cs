using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectRogue
{
    public abstract class GUIPage : GUI
    {
        protected List<GUIModule> content = new List<GUIModule>();

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach(GUIModule module in content)
            {
                module.Draw(spriteBatch);
            }
        }

        public override void KeyPress(KeyboardState state)
        {
            foreach(GUIModule module in content)
            {
                module.KeyPress(state, mapper);
            }
        }
    }
}
