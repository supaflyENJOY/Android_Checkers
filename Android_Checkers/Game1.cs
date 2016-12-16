using Android_Checkers.Assets;
using Java.Lang;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

namespace Android_Checkers {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {
        GraphicsDeviceManager graphics;
        public Texture2D TextureBoard, TextureChecker;
        GameBoard gb;

        public static float Dx = 1f;
        public static float Dy = 1f;
        private static int NominalWidth = 504;
        private static int NominalHeight = 504;
        private static float NominalWidthCounted;
        private static float NominalHeightCounted;
        private static int CurrentWidth;
        private static int CurrentHeigth;
        private static float deltaY = 0;
        private static float deltaY_1 = 0;
        private static float posX = 0, posY = 0;
        public static float YTopBorder;
        public static float YBottomBorder;
        private SpriteBatch SpriteBatch;

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            var metric = new Android.Util.DisplayMetrics();
            Activity.WindowManager.DefaultDisplay.GetMetrics(metric);
            // установка параметров экрана

            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = metric.WidthPixels;
            graphics.PreferredBackBufferHeight = metric.HeightPixels;
            CurrentWidth = graphics.PreferredBackBufferWidth;
            CurrentHeigth = graphics.PreferredBackBufferHeight;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            UpdateScreenAttributies();
            Debug.WriteLine("CurrentWidth: " + CurrentWidth);
            Debug.WriteLine("CurrentHeigth: " + CurrentHeigth);
            Debug.WriteLine("NominalWidth: " + NominalWidth);
            Debug.WriteLine("NominalHeight: " + NominalHeight);
            Debug.WriteLine("Dx: " + Dx);
            Debug.WriteLine("Dy: " + Dy);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here
            var online = true;
            gb = new GameBoard(online);
            var sockets = new Sockets("149.154.66.55", 6969);
            gb.SetSockets(sockets);
            if(online == false) {
                gb.StartGame();
            } else {
                sockets.SetGameBoard(gb);
                sockets.Connect();
            }
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Content.RootDirectory = "Content/";
            TextureBoard = Content.Load<Texture2D>("board");
            TextureChecker = Content.Load<Texture2D>("checker");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation tl in touchCollection) {
                if ((tl.State == TouchLocationState.Released)) {
                    gb.ProceedLeftClick(tl.Position.X, tl.Position.Y);
                    break;
                }
            }

            base.Update(gameTime);
        }

        public static float AbsoluteX(float x) {
            return x * Dx + posX;
        }
        
        public static float AbsoluteY(float y) {
            return y * Dy + posY;
        }

        public static float DeabsoluteX(float x) {
            return (x - posX) / Dx;
        }
        
        public static float DeabsoluteY(float y) {
            return (y - posY) / Dy;
        }

        public void UpdateScreenAttributies() {
            Dx = (float)CurrentWidth / NominalWidth;
            Dy = (float)CurrentHeigth / NominalHeight;
            Dy = Dx = Math.Min(Dx, Dy);

            NominalHeightCounted = CurrentHeigth / Dx;
            NominalWidthCounted = CurrentWidth / Dx;

            int check = Math.Abs(CurrentHeigth - CurrentWidth / 16 * 9);
            if (check > 10)
                deltaY = (float)check / 2; // недостающее расстояние до 16:9 по п оси Y (в абсолютных координатах)
            deltaY_1 = -(CurrentWidth / 16 * 10 - CurrentWidth / 16 * 9) / 2f;

            YTopBorder = -deltaY / Dx; // координата точки в левом верхнем углу (в вируальных координатах)
            YBottomBorder = NominalHeight + (180); // координата точки в нижнем верхнем углу (в виртуальных координатах)
            posX = (CurrentWidth - Math.Min(CurrentHeigth, CurrentWidth)) / 2;
            posY = (CurrentHeigth - Math.Min(CurrentWidth, CurrentHeigth)) / 2;
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            SpriteBatch.Draw(TextureBoard, new Vector2(posX, posY),
                             new Rectangle(0, 0, TextureBoard.Width, TextureBoard.Height),
                             Color.White,
                             0, new Vector2(0, 0), 1 * Dx, SpriteEffects.None, 0);
            // TODO: Add your drawing code here
           foreach(var s in gb.entity) {
                if(s.marked)
                    SpriteBatch.Draw(TextureChecker, new Vector2(AbsoluteX(30 + 56 * s.x), AbsoluteY(30 + 56 * s.y)),
                                 new Rectangle(0, 0, TextureChecker.Width, TextureChecker.Height),
                                 s.getTeam() == CheckerTeam.White ? new Color(210, 210, 210) : new Color(70, 70, 70),
                                 0, new Vector2(0, 0), 0.52f * Dx, SpriteEffects.None, 0);
                else
                    SpriteBatch.Draw(TextureChecker, new Vector2(AbsoluteX(30 + 56 * s.x), AbsoluteY(30 + 56 * s.y)),
                             new Rectangle(0, 0, TextureChecker.Width, TextureChecker.Height),
                             s.getTeam() == CheckerTeam.White ? Color.White : new Color(40, 40, 40),
                             0, new Vector2(0, 0), 0.52f * Dx, SpriteEffects.None, 0);
            }

            SpriteBatch.End(); // прервать отрисовку на данном этапе
            base.Draw(gameTime);
        }
    }
}
