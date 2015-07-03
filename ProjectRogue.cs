using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System;
using System.Reflection;
using System.Globalization;

namespace ProjectRogue
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class ProjectRogue : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState previousState = new KeyboardState();

        public ProjectRogue()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //set the position
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 800;

            Point position = new Point();
            position.X = (GraphicsDevice.Adapter.CurrentDisplayMode.Width - 800) / 2;
            position.Y = (GraphicsDevice.Adapter.CurrentDisplayMode.Height - 800) / 2;

            this.Window.Position = position;

            graphics.ApplyChanges();
            
            GameController.mainWindow = this;
            GameController.currentGUI = new TurnHandler();

            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += Window_ClientSizeChanged;
            this.IsMouseVisible = true;

            KeyMapper.LoadKeyMappings(@".\settings\keybindings.txt");
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            string fontPath = Path.Combine(Path.GetFullPath("resources/"), "DejaVuSans.ttf");
            string textFontPath = Path.Combine(Path.GetFullPath("resources/"), "DejaVuSansMono.ttf");
            GraphX.font = new VectorFont(fontPath, 40);
            GraphX.textFont = new VectorFont(textFontPath, GraphX.textFontHeight);
            GraphX.smallFont = new VectorFont(fontPath, 10);

            foreach(TilePaint t in TilePaint.tilePaints)
            {
                t.LoadContent(GraphicsDevice);
            }

            SideBar.green = new Texture2D(GraphicsDevice, 1, 1);
            SideBar.green.SetData( new Color[] {Color.Green} );

            SideBar.grey = new Texture2D(GraphicsDevice, 1, 1);
            SideBar.grey.SetData( new Color[] {Color.Gray} );

            GraphX.shadow = new Texture2D(GraphicsDevice, 1, 1);
            GraphX.shadow.SetData(new Color[] { Color.FromNonPremultiplied(0, 0, 0, 100) });
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            SideBar.green.Dispose();
            SideBar.grey.Dispose();
            GraphX.shadow.Dispose();

            GraphX.font.Dispose();
            GraphX.textFont.Dispose();
            GraphX.smallFont.Dispose();
        }

        TimeSpan accumulatedDeltaTime = TimeSpan.Zero;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();

            if(state != previousState)
            {
                previousState = state;

                int keys = state.GetPressedKeys().Length;

                if (state.IsKeyDown(Keys.LeftShift))
                    keys--;
                if (state.IsKeyDown(Keys.RightShift))
                    keys--;
                if (state.IsKeyDown(Keys.LeftControl))
                    keys--;
                if (state.IsKeyDown(Keys.RightControl))
                    keys--;
                if (state.IsKeyDown(Keys.LeftAlt))
                    keys--;
                if (state.IsKeyDown(Keys.RightAlt))
                    keys--;
                if (state.IsKeyDown(Keys.LeftWindows))
                    keys--;
                if (state.IsKeyDown(Keys.RightWindows))
                    keys--;

                if(keys > 0 && GameController.currentGUI != null)
                {
                    GameController.currentGUI.KeyPress(state);
                }
            }


            accumulatedDeltaTime += gameTime.ElapsedGameTime;

            if(accumulatedDeltaTime.TotalMilliseconds > 1f / 60f * 1000f)
            {
                accumulatedDeltaTime = TimeSpan.Zero;
                if(GameController.currentGUI != null)
                    GameController.currentGUI.Tick();
            }

            base.Update(gameTime);
        }

        void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            if(Window.ClientBounds.Width < 800)
            {
                graphics.PreferredBackBufferWidth = 800;
                graphics.ApplyChanges();
            }

            if (Window.ClientBounds.Height < 800)
            {
                graphics.PreferredBackBufferHeight = 800;
                graphics.ApplyChanges();
            }

            Rectangle gameWindow = this.Window.ClientBounds;
            GraphX.messageBoxHeight = MyMath.Clamp((int)(gameWindow.Height * (1f / 3f)), 0, GraphX.maxMessageBoxHeight) + (gameWindow.Height - MyMath.Clamp((int)(gameWindow.Height * (1f / 3f)), 0, GraphX.maxMessageBoxHeight)) % GraphX.tilesVisibleY;
            GraphX.tileLength = (gameWindow.Height - GraphX.messageBoxHeight) / GraphX.tilesVisibleY;
            GraphX.sideBarWidth = MyMath.Clamp((int)(gameWindow.Width * (1f / 3f)), 0, GraphX.maxSideBarWidth) + (gameWindow.Width - MyMath.Clamp((int)(gameWindow.Width * (1f / 3f)), 0, GraphX.maxSideBarWidth) - 2 * GraphX.sideBarOffset) %  GraphX.tileLength;
            GraphX.tilesVisibleX = (gameWindow.Width - GraphX.sideBarWidth - 2 * GraphX.sideBarOffset) / GraphX.tileLength;

            if(GraphX.tilesVisibleX % 2 == 0)
            {
                GraphX.tilesVisibleX--;
                GraphX.sideBarWidth += GraphX.tileLength;
            }

            GraphX.resized = true;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            if(GameController.currentGUI != null)
                GameController.currentGUI.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        bool fullscreen = false;
        Rectangle prevWindowPos;

        public void ToggleFullscreen()//TODO: remember settings, possibly options file with option for permanent an on the fly changes
        {
            if(fullscreen)
            {
                Window.Position = prevWindowPos.Location;

                graphics.PreferredBackBufferWidth = prevWindowPos.Width;
                graphics.PreferredBackBufferHeight = prevWindowPos.Height;

                Window.IsBorderless = false;
            }
            else
            {
                prevWindowPos = Window.ClientBounds;
                prevWindowPos.Location = Window.Position;

                Window.IsBorderless = true;

                Window.Position = Point.Zero;

                graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            }

            fullscreen = !fullscreen;

            graphics.ApplyChanges();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            while(GameController.currentGUI != null)
            {
                GameController.currentGUI.Close();
            }

            base.OnExiting(sender, args);
        }
      
    }
}
