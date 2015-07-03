using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ProjectRogue
{
    public abstract class GUI
    {
        GUI lastGUI;
        bool loaded = false;

        public bool Loaded
        {
            get { return loaded; }
        }

        protected abstract KeyMapper mapper
        {
            get;
        }

        public void Load(GUI lastGUI)
        {
            this.lastGUI = lastGUI;
            this.loaded = true;
            GameController.currentGUI = this;
            this.OnLoad();
        }

        protected virtual void OnLoad() { }

        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void KeyPress(KeyboardState state);
        public virtual void Tick() { }

        public void Close()
        {
            this.OnClose();

            GameController.currentGUI = lastGUI;
            if(lastGUI == null)
                GameController.mainWindow.Exit();
        }

        protected virtual void OnClose() { }
    }
}
