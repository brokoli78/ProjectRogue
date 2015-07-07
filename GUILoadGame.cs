using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace ProjectRogue
{
    public class GUILoadGame : GUIPage
    {
        protected override KeyMapper mapper
        {
            get { return KeyMapper.getMapper("Main Game"); }
        }

        List<string> files = new List<string>();

        public GUILoadGame()
        {
            content.Add(new GUITextbox("    enter: load    escape: cancle",
                delegate()
                {
                    return new Rectangle(0, GameController.mainWindow.Window.ClientBounds.Height - 2 * (int)GraphX.textFontHeight, GameController.mainWindow.Window.ClientBounds.Width, (int)GraphX.textFontHeight); //TODO: beautify
                }, 0));

            files = Directory.EnumerateFiles(GameController.saveDirectory, "*.rogue").ToList(); //TODO: sanity check header

            files = files.Select(s => s.Remove(0, GameController.saveDirectory.Length)).ToList();
            files = files.Select(s => s.Remove(s.Length - ".rogue".Length)).ToList();

            content.Add(new GUIList(files, delegate() { return GameController.mainWindow.Window.ClientBounds; }, 0, ListStlyes.SingleCentered, ItemSelected, true));

        }

        public void ItemSelected(int item)
        {
            if(files.Count > 0)
            {
                GameController.FileName = GameController.saveDirectory + files[item] + ".rogue";

                GameController.currentGUI.Close();
                GameController.currentGUI = new TurnHandler();
            }
        }

        public override void KeyPress(KeyboardState state) //TODO: text input, remember name
        {
            if (mapper.HasState("escape", state))
            {
                GameController.currentGUI.Close();
            }

            base.KeyPress(state);
        }

    }
}
