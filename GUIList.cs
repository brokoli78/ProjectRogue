using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectRogue
{
    public enum ListStlyes { Single, Multiple, SingleCentered };

    class GUIList : GUIModule
    {
        Action<int> selectedAction;

        List<string> listContent;
        int bufferZone;
        ListStlyes style;

        int longestStringLength = 0;
        int selectedItem = 0;
        int firstDisplayedItem = 0;
        int numItemsHorizontal;
        int numItemsVertical;
        bool displayScrollArrows;

        int numDisplayableItems
        {
            get { return numItemsHorizontal * numItemsVertical; }
        }
                      
        public GUIList(List<string> listStrings, Func<Rectangle> bounds, int bufferZone, ListStlyes style, Action<int> selectedAction, bool displayScrollArrows)
            :base(bounds)
        {
            this.selectedAction = selectedAction;
            this.bufferZone = bufferZone;
            this.displayScrollArrows = displayScrollArrows;


            listContent = listStrings;

            foreach(string s in listStrings)
            {
                if (longestStringLength < s.Length)
                    longestStringLength = s.Length;
            }

            longestStringLength *= (int) GraphX.textFont.MeasureString(" ").x;

            this.style = style;

            numItemsHorizontal = 1;
            numItemsVertical = (int)((drawingRectangle().Height - bufferZone) / (GraphX.textFontHeight + bufferZone));
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            switch (style)
            {
                case ListStlyes.Single:
                    throw new NotImplementedException();

                case ListStlyes.Multiple:
                    throw new NotImplementedException();

                case ListStlyes.SingleCentered:

                    numItemsHorizontal = 1;
                    numItemsVertical = (int) ((drawingRectangle().Height - bufferZone) / (GraphX.textFontHeight + bufferZone));

                    for (int i = 0; i < numItemsVertical && i < listContent.Count; i++)
                    {
                        int x = drawingRectangle().X + (int) (drawingRectangle().Width - GraphX.textFont.MeasureString(listContent[firstDisplayedItem + i]).x) / 2 ;

                        if (firstDisplayedItem + i != selectedItem)
                            GraphX.textFont.DrawString(spritebatch, listContent[firstDisplayedItem + i], new Vector2(x, drawingRectangle().Y + bufferZone + i * (GraphX.textFontHeight + bufferZone)), Color.Gray);
                        else
                            GraphX.textFont.DrawString(spritebatch, listContent[firstDisplayedItem + i], new Vector2(x, drawingRectangle().Y + bufferZone + i * (GraphX.textFontHeight + bufferZone)), Color.GhostWhite);
                    }

                    break;
            }

            if (firstDisplayedItem != 0 && displayScrollArrows)
                GraphX.textFont.DrawString(spritebatch, "^", new Vector2(drawingRectangle().X, drawingRectangle().Y), Color.GhostWhite);

            if (firstDisplayedItem + numDisplayableItems < listContent.Count && displayScrollArrows)
                GraphX.textFont.DrawString(spritebatch, "v", new Vector2(drawingRectangle().X, drawingRectangle().Y + drawingRectangle().Height - GraphX.textFontHeight), Color.GhostWhite);

        }

        public override void KeyPress(KeyboardState state, KeyMapper mapper)
        {
            if(mapper.HasState("arrowDown", state) && selectedItem != listContent.Count - 1)
            {
                selectedItem++;

                if (firstDisplayedItem + numDisplayableItems - 1 < selectedItem)
                    firstDisplayedItem++;
            }

            if (mapper.HasState("arrowUp", state) && selectedItem != 0)
            {
                selectedItem--;

                if (firstDisplayedItem > selectedItem)
                    firstDisplayedItem--;
            }


            if(mapper.HasState("enter", state))
                selectedAction(selectedItem);

            base.KeyPress(state, mapper);
        }
    }
}
