﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpFont.TrueType;
using SharpFont;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;


namespace ProjectRogue
{
    public class VectorFont : IDisposable
    {
        Library lib;
        Face face;
        FTSize size;

        Dictionary<char, VectorGlyph> database = new Dictionary<char, VectorGlyph>();

        /// <summary>
        /// Creates a new VectorFont with the File specified at path and the specified size in pixels.
        /// It is assumed that the font is either Monospaced or only single chars are used.
        /// </summary>
        /// <param name="path">path to the font-file</param>
        /// <param name="size">size in pixles</param>

        public VectorFont(string path, uint size)
        {
            lib = new Library();

            face = new Face(lib, path);

            face.SetPixelSizes(0, size);

            this.size = face.Size;
        }


        /// <summary>
        /// Sets a new font size in pixels
        /// </summary>
        /// <param name="pixels">size in pixels</param>

        public void SetSize(uint pixels)
        {
            face.SetPixelSizes(0, pixels);

            foreach (VectorGlyph glyph in database.Values)
            {
                glyph.Dispose();
            }

            database = new Dictionary<char, VectorGlyph>();

            this.size.Dispose();

            this.size = face.Size;
        }

        //returns the char texture from the database or generates it
        private VectorGlyph getCharTexture(char currentChar)
        {
            if (database.ContainsKey(currentChar))
            {
                return database[currentChar];
            }

            uint glyphIndex = face.GetCharIndex(currentChar);

            face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);

            face.Glyph.RenderGlyph(RenderMode.Mono);

            GlyphSlot glyph = face.Glyph;

            System.Drawing.Bitmap bmp = null;

            if (currentChar == ' ')
            {
                bmp = new System.Drawing.Bitmap((int)glyph.Advance.X, 1);
            }
            else
            {
                bmp = glyph.Bitmap.ToGdipBitmap(System.Drawing.Color.White);
            }

            Texture2D texture = null;

            using(MemoryStream s = new MemoryStream())
            {
                bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                s.Seek(0, SeekOrigin.Begin);
                texture = Texture2D.FromStream(GameController.mainWindow.GraphicsDevice, s);
            }

            bmp.Dispose();

            VectorGlyph vectorGlyph = new VectorGlyph(texture, glyph.BitmapLeft, glyph.BitmapTop, (int)glyph.Metrics.HorizontalAdvance);

            database.Add(currentChar, vectorGlyph);

            return vectorGlyph;
        }

        /// <summary>
        /// draws a string to the specified coordinates
        /// </summary>
        /// <param name="spriteBatch">spriteBatch to draw to</param>
        /// <param name="text">the text to draw</param>
        /// <param name="position">the origin of the draw</param>
        /// <param name="color">the color of the text</param>

        public void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color) //TODO: Add wrap text method / class for easy use in GUI (maybe even rewrite sidebar and gamelog)
        {
            Vector2 currentPosition = position;

            foreach (char currentChar in text)
            {
                VectorGlyph vectorGlyph = getCharTexture(currentChar);

                Vector2 glyphTopPosition = currentPosition + new Vector2(vectorGlyph.TextureLeft, (int)size.Metrics.Descender - vectorGlyph.TextureTop + (int)size.Metrics.NominalHeight);

                spriteBatch.Draw(vectorGlyph.Texture, glyphTopPosition, color);

                currentPosition.x += vectorGlyph.HorizontalAdvance;
            }
        }

        /// <summary>
        /// draws a string centered in the specified rectangle
        /// </summary>
        /// <param name="spriteBatch">spriteBatch to draw to</param>
        /// <param name="text">the text to draw</param>
        /// <param name="position">the origin of the draw</param>
        /// <param name="color">the color of the text</param>


        public void DrawString(SpriteBatch spriteBatch, string text, Rectangle position, Color color)
        {
            char c = text[0];

            VectorGlyph vectorGlyph = getCharTexture(c);

            int x = (position.Width - vectorGlyph.Texture.Width) / 2;
            int y = (position.Height - vectorGlyph.Texture.Height) / 2;

            Vector2 glyphTopPosition = new Vector2(position.X + x, position.Y + y);

            spriteBatch.Draw(vectorGlyph.Texture, glyphTopPosition, color);
        }

        public void Dispose()
        {
            foreach (VectorGlyph glyph in database.Values)
            {
                glyph.Dispose();
            }

            database = null;

            this.size.Dispose();
        }

        struct VectorGlyph : IDisposable
        {
            public Texture2D Texture;
            public int TextureLeft;
            public int TextureTop;
            public int HorizontalAdvance;

            public VectorGlyph(Texture2D Texture, int TextureLeft, int TextureTop, int HorizontalAdvance)
            {
                this.Texture = Texture;
                this.TextureLeft = TextureLeft;
                this.TextureTop = TextureTop;
                this.HorizontalAdvance = HorizontalAdvance;
            }

            public void Dispose()
            {
                Texture.Dispose();
            }
        }
    }
}
