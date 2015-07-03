using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace ProjectRogue
{
    public static class GameLog
    {
        public static List<Message> gameLog
        {
            get { return GameController.gameLog; }
            set { GameController.gameLog = value; }
        }

        static List<Message> newMessages = new List<Message>();

        public static void DrawLog(SpriteBatch spriteBatch, Rectangle drawArea)
        {
            Stack<Message> toDraw = new Stack<Message>();
            int height = (int)GraphX.textFontHeight + 1;
            int stringsToDraw = drawArea.Height / height;

            for(int i = 1; i <= gameLog.Count && i <= stringsToDraw; i++)
            {
                toDraw.Push(getStringToDraw(ref i, ref stringsToDraw));
            }

            // Draw the text
            int j = 0;
            while(toDraw.Count > 0)
            {
                Rectangle rect = new Rectangle(drawArea.X, drawArea.Y + height * j, drawArea.Width, height);
                Vector2 pos = new Vector2(rect.X, rect.Y);
                Message message = toDraw.Pop();
                string messageString = "";
                for (int i = 0; i < message.message.Count; i++)
                {
                    messageString += message.message[i];
                    if (i < message.filler.Count)
                        messageString += message.filler[i];
                }

                GraphX.textFont.DrawString(spriteBatch, messageString, pos, message.color);
                j++;
            }
        }

        static Message getStringToDraw(ref int i, ref int stringsToDraw)
        {
            int j = 0;
            do
            {
                j++;
            }while((gameLog.Count > i + j && gameLog[gameLog.Count - i] == gameLog[gameLog.Count - i - j]));

            Message message;
            if (j != 1)
            {
                List<string> help = new List<string>();
                help.AddRange(gameLog[gameLog.Count - i].message);
                help.Add(" x" + j.ToString());
                message = new Message(help, gameLog[gameLog.Count - i].filler, gameLog[gameLog.Count - i].color);
            }
            else
            {
                message = gameLog[gameLog.Count - i];
            }

            stringsToDraw += j - 1;
            i += j - 1;
            return message;
        }

        public static void newMessage(Message message)
        {
            newMessages.Add(message);
            gameLog.Add(message);

        }

        public static void newMessage(string message)
        {
            newMessages.Add(new Message(message));
            gameLog.Add(new Message(message));
        }

        public static void newMessage(string message, Color color)
        {
            newMessages.Add(new Message(message, color));
            gameLog.Add(new Message(message, color));
        }

        public static void newMessage(List<string> message, List<string> filler)
        {
            newMessages.Add(new Message(message, filler));
            gameLog.Add(new Message(message, filler));
        }

        public static void newMessage(List<string> message, List<string> filler, Color color)
        {
            newMessages.Add(new Message(message, filler, color));
            gameLog.Add(new Message(message, filler, color));
        }

        public static List<Message> getAndClearNewMessages()
        {
            List<Message> dummy = new List<Message>();
            dummy.AddRange(newMessages);
            newMessages = new List<Message>();
            return dummy;
        }
    }

    public class Message
    {
        public List<string> message;
        public List<string> filler;
        public Color color;

        public Message(List<string> message, List<string> filler, Color color)
        {
            this.message = message;
            this.filler = filler;
            this.color = color;
        }

        public Message(string message)
        {
            this.message = new List<string>{message};
            this.filler = new List<string>();
            this.color = Color.Gray;
        }

        public Message(string message, Color color)
        {
            this.message = new List<string> { message };
            this.filler = new List<string>();
            this.color = color;
        }

        public Message(List<string> message, List<string> filler)
            : this(message, filler, Color.Gray) { }

        public override bool Equals(Object obj)
        {
            Message vec = obj as Message;
            if ((object)vec == null)
                return false;

            return this == vec;
        }

        public override int GetHashCode()
        {
            return message.Count;
        }

        public static bool operator ==(Message m1, Message m2)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(m1, m2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)m1 == null) || ((object)m2 == null))
            {
                return false;
            }

            for (int i = 0; i < m1.message.Count; i++ )
            {
                if(m1.message[i] != m2.message[i])
                {
                    return false;
                }
            }

            for (int i = 0; i < m1.filler.Count; i++)
            {
                if (m1.filler[i] != m2.filler[i])
                {
                    return false;
                }
            }

            // Return true if the fields match:
            return m1.color == m2.color;
        }

        public static bool operator !=(Message m1, Message m2)
        {
            return !(m1 == m2);
        }

        public static bool sameBaseMessage(Message m1, Message m2)
        {
            if(m1.message.Count != m2.message.Count)
            {
                return false;
            }

            bool test = true;

            for (int i = 0; i < m1.message.Count; i ++)
            {
                if(m1.message[i] != m2.message[i])
                {
                    test = false;
                    break;
                }
            }

            return test && m1.color == m2.color;
        }
    }
}
