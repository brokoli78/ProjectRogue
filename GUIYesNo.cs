using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectRogue
{
    class GUIYesNo :GUI
    {
        string question;
        Action yesAction;
        Action noAction;

        public GUIYesNo(string question, Action yesAction, Action noAction)
        {
            this.question = question;
            this.yesAction = yesAction;
            this.noAction = noAction;
        }

        protected override KeyMapper mapper
        {
            get { return KeyMapper.getMapper("Main Game"); }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphX.textFont.DrawString(spriteBatch, question, Vector2.zero, Color.GhostWhite); //TODO: add yes/no keyboard legend (possibly using keybindings?, just grab the first option
        }

        public override void KeyPress(KeyboardState state)
        {
            if(mapper.HasState("accept", state) || mapper.HasState("safeAccept", state))
            {
                Close();
                yesAction();
                return;
            }

            if(mapper.HasState("decline", state) || mapper.HasState("safeDecline", state))
            {
                Close();
                noAction();
                return;
            }
        }

    }
}
