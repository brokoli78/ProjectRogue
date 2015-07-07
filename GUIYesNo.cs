using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectRogue
{
    class GUIYesNo :GUIPage
    {
        string question;
        Action yesAction;
        Action noAction;

        public GUIYesNo(string question, Action yesAction, Action noAction) //TODO: add yes/no keyboard legend (possibly using keybindings?, just grab the first option
        {
            this.question = question;
            this.yesAction = yesAction;
            this.noAction = noAction;

            content.Add(new GUITextbox(question, delegate() { return GameController.mainWindow.Window.ClientBounds; }, 0));
            content.Add(new GUITextbox("    y: yes    n: no",
                delegate()
                {
                    return new Rectangle(0, GameController.mainWindow.Window.ClientBounds.Height - 2 * (int)GraphX.textFontHeight, GameController.mainWindow.Window.ClientBounds.Width, (int)GraphX.textFontHeight); //TODO: beautify
                }, 0));
                
        }

        protected override KeyMapper mapper
        {
            get { return KeyMapper.getMapper("Main Game"); }
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

            base.KeyPress(state);
        }

    }
}
