using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectRogue
{
    public class GUIMainMenu : GUIPage
    {
        protected override KeyMapper mapper
        {
            get { return KeyMapper.getMapper("Main Game"); }
        }

        List<string> logoString = new List<string>();

        List<string> menuItems = new List<string> {
                "New Game",
                "Load Game",
                "Quit"
            };


        public GUIMainMenu()
        {
            using (FileStream stream = File.OpenRead(@".\resources\logo.logo"))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if(line.Length > logoLength)
                        logoLength = line.Length;
                    logoHeigth++;
                    logoString.Add(line);
                }
            }

            content.Add(new GUIList(menuItems, menuListBounds, 0, ListStlyes.SingleCentered, ItemSelected, true));
        }


        int logoLength = 0;
        int logoHeigth = 0;

        Rectangle logoRectangle
        {
            get { return new Rectangle((GameController.mainWindow.Window.ClientBounds.Width - logoLength * GraphX.textFontAdvance) / 2, 0, logoLength * GraphX.textFontAdvance, logoHeigth * GraphX.textFontHeight); }
        }

        public Rectangle menuListBounds()
        {
            return new Rectangle(0, logoRectangle.Bottom + (GameController.mainWindow.Window.ClientBounds.Height - logoRectangle.Bottom - menuItems.Count * GraphX.textFontHeight) / 2, GameController.mainWindow.Window.ClientBounds.Width, menuItems.Count * GraphX.textFontHeight);
        }

        public void ItemSelected(int item)
        {
            switch(item)
            {
                case 0:
                    GameController.currentGUI = new GUICreateCharacter();
                    break;

                case 1:
                    GameController.currentGUI = new GUILoadGame();
                    break;

                case 2:
                    GameController.currentGUI.Close();
                    break;

                default:
                    throw new ArgumentOutOfRangeException("item", "this item id is not good");
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < logoString.Count; i++)
            {
                int x = logoRectangle.X;
                int y = logoRectangle.Y + i * GraphX.textFontHeight;
                GraphX.textFont.DrawString(spriteBatch, logoString[i], new Vector2(x, y), Color.Gray);
            }

            base.Draw(spriteBatch);
        }
    }
}
