using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProjectRogue
{
    public class GUICreateCharacter :GUIPage
    {
        protected override KeyMapper mapper
        {
            get { return KeyMapper.getMapper("Main Game"); }
        }

        NameGenerator ng;

        public GUICreateCharacter()
        {
            content.Add(new GUITextbox("    enter: accept    escape: cancle    generate new name: g",
                delegate()
                {
                    return new Rectangle(0, GameController.mainWindow.Window.ClientBounds.Height - 2 * (int)GraphX.textFontHeight, GameController.mainWindow.Window.ClientBounds.Width, (int)GraphX.textFontHeight); //TODO: beautify
                }, 0));

            ng =  new NameGenerator(NameGenerator.LoadDictionary(GameController.dictionaryPath), 3, .001, true);
            if(GameController.playerName == null)
                GameController.playerName = ng.GenerateName();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            GraphX.textFont.DrawString(spriteBatch, "Your Name: " + GameController.playerName, new Vector2(0, 0), Color.GhostWhite);
            base.Draw(spriteBatch);
        }

        public override void KeyPress(KeyboardState state) //TODO: text input, remember name
        {
            if(mapper.HasState("pickUp", state)) //TODO: menu keyboard layout
            {
                GameController.playerName = ng.GenerateName();
            }
            else if(mapper.HasState("enter", state))
            {
                string origFileName = GameController.playerName;
                string fileName = origFileName;
                int i = 1;

                while(File.Exists(GameController.saveDirectory + fileName + ".rogue"))
                {
                    fileName = origFileName + " (" + i + ")";
                }

                GameController.FileName = GameController.saveDirectory + fileName + ".rogue";

                GameController.currentGUI.Close();
                GameController.currentGUI = new TurnHandler();
            }
            else if(mapper.HasState("escape", state))
            {
                GameController.currentGUI.Close();
            }

            base.KeyPress(state);
        }
    }
}
