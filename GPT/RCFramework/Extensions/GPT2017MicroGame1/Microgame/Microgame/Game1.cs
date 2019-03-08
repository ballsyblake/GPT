using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using RC_Framework;

namespace Microgame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static bool showbb = false;
        SpriteFont fonty;

        public MicroGame1 micro0 = null;
        public MicroGame1 micro1 = null;
        public MicroGame1 micro2 = null;

        MouseState mState = Mouse.GetState();
        MouseState prevmState;
        int mousex, mousey;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 1000;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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

            LineBatch.init(GraphicsDevice);

            UtilTexSI.initTextures(GraphicsDevice);
            UtilTexSI.init8Bit(GraphicsDevice);

            Texture2D texQ = UtilTexSI.eightBit32[2];

            string[] str13 = {"   7    ",
                              "  777   ",
                              "77777777",
                              "77777777"};

            RC_Surface surBase = new RC_Surface(str13, UtilTexSI.Pallete11, UtilTexSI.Pal11);
            RC_Surface sur = new RC_Surface(surBase, 4, 4, 0, 0, Color.TransparentBlack, 0, 0);
            Texture2D shooterTexQ = sur.createTex(GraphicsDevice);


            fonty = Content.Load<SpriteFont>("Fonty");

            micro0 = new MicroGame1(new Rectangle(100,100,300,450),30,Color.Green,fonty);
            micro0.setShooter(shooterTexQ, new Vector2(30, 15), 5);
            micro0.setTarget(texQ, new Vector2(20, 20), 20);
            micro0.setExplode(UtilTexSI.texRainbow, new Vector2(25, 25));

           //micro0.setBackground(aa);
            micro0.playStyle = 1;
            micro0.mouseFocus = true;
            micro0.reset();

            micro1 = new MicroGame1(new Rectangle(450, 110, 200, 400), 35, Color.Purple, fonty);
            micro1.setShooter(shooterTexQ, new Vector2(30, 15), 2);
            micro1.setTarget(texQ, new Vector2(40, 40), 20);
            micro1.setExplode(UtilTexSI.texRainbow, new Vector2(25, 25));

            BlinkingBoxes aa = new BlinkingBoxes(new Rectangle(0, 0, 0, 0), new Vector2(38, 29), new Vector2(2, 5), 5, 9, 6, 101);

            micro1.setBackground(aa);
            micro1.playStyle = 1;
            micro1.mouseFocus = true;
            micro1.reset();

            micro2 = new MicroGame1(new Rectangle(660, 100, 300, 200), 40, Color.DarkCyan, fonty);
            micro2.setShooter(shooterTexQ, new Vector2(30, 15), 5);
            micro2.setTarget(texQ, new Vector2(20, 20), 20);
            micro2.setExplode(UtilTexSI.texRainbow, new Vector2(25, 25));

            //micro0.setBackground(aa);
            micro2.playStyle = 1;
            micro2.mouseFocus = true;
            micro2.reset();

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
           
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            prevmState = mState;
            mState = Mouse.GetState();

            KeyboardState k = Keyboard.GetState();

            mousex = mState.X;
            mousey = mState.Y;
            if (mState.LeftButton == ButtonState.Pressed && prevmState.LeftButton == ButtonState.Released)
            {
                micro0.MouseDownEventLeft((float)mousex, (float)mousey);
                micro1.MouseDownEventLeft((float)mousex, (float)mousey);
                micro2.MouseDownEventLeft((float)mousex, (float)mousey);
            }

            micro0.Update(gameTime);
            micro1.Update(gameTime);
            micro2.Update(gameTime);


            if (k.IsKeyDown(Keys.D0)) { micro0.playStyle = 2; }
            if (k.IsKeyDown(Keys.D1)) { micro1.playStyle = 2; }
            if (k.IsKeyDown(Keys.D2)) { micro2.playStyle = 2; }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Silver);

            micro0.Draw(spriteBatch); // does its own begin and end in spritebatch
            micro1.Draw(spriteBatch); // does its own begin and end in spritebatch
            micro2.Draw(spriteBatch); // does its own begin and end in spritebatch

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            LineBatch.drawCross(spriteBatch,mousex,mousey,4,Color.Black,Color.Black);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
