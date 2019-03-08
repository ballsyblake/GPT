using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System;

using RC_Framework;

namespace Week_4_Tutorial
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        //Initialisation variables
        string dir = @"H:\GPT\Week 4 Tutorial\Week 4 Tutorial\Sprites\RCStock5\";
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D texBack = null;
        Texture2D texpaddle = null;
        Texture2D texBall = null;
        float xx = 350;
        float yy = 511;
        KeyboardState k;
        Sprite3 paddle = null;
        ImageBackground back1 = null;
        Rectangle playArea;
        bool showbb = false;
        KeyboardState prevK;

        //Boundaries
        int top = 56;
        int bot = 543;
        int paddleSpeed = 3; //this is not a boundary obviously... pretty self explanatory what this is 
        int lhs = 236;
        int rhs = 564;

        //Ball variables
        Sprite3 ball = null; //ball
        bool ballStuck = true;
        Vector2 ballOffset = new Vector2(32, -10);

        //Sprite List
        Texture2D texBlock1 = null;
        SpriteList sl = null;
        int blocksOffsetX = 40;
        int blocksOffsetY = 40;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //Setting the screen resoultion to 600x800
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;
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
            texBack = Util.texFromFile(GraphicsDevice, dir + "back3.png"); //***
            texpaddle = Util.texFromFile(GraphicsDevice, dir + "red64x32.png"); //***
            texBall = Util.texFromFile(GraphicsDevice, dir + "ball2.png"); //***
            texBlock1 = Util.texFromFile(GraphicsDevice, dir + "white64x32.png");
            paddle = new Sprite3(true, texpaddle, xx, yy);
            paddle.setBBToTexture();
            ball = new Sprite3(true, texBall, xx, yy);
            ball.setBBandHSFractionOfTexCentered(0.7f);
            sl = new SpriteList();
            
            back1 = new ImageBackground(texBack, Color.White, GraphicsDevice);
            playArea = new Rectangle(lhs, top, rhs - lhs, bot - top); // width and height
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Sprite3 s = new Sprite3(true, texBlock1, x * 68 + playArea.X + blocksOffsetX, y * 36 + playArea.Y + blocksOffsetY);
                    s.hitPoints = 1;
                    if (y == 0)
                    {
                        s.hitPoints = 2;
                        s.setColor(Color.LightBlue);
                    }

                    sl.addSpriteReuse(s);
                }
            }
            // TODO: use this.Content to load your game content here
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
            prevK = k;
            k = Keyboard.GetState();

            if (k.IsKeyDown(Keys.B) && prevK.IsKeyUp(Keys.B)) // ***
            {
                showbb = !showbb;
            }

            if (k.IsKeyDown(Keys.Right))
            {
                if (paddle.getPosX() < rhs - texpaddle.Width) paddle.setPosX(paddle.getPosX() + paddleSpeed);
            }

            if (k.IsKeyDown(Keys.Left))
            {
                if (paddle.getPosX() > lhs) paddle.setPosX(paddle.getPosX() - paddleSpeed);
            }
            if (ballStuck)
            {
                ball.setPos(paddle.getPos() + ballOffset);
                if (k.IsKeyDown(Keys.Space) && prevK.IsKeyUp(Keys.Space))
                {
                    ballStuck = false;
                    ball.setDeltaSpeed(new Vector2(2, -3));
                }
            }
            
            else
            {
                // move ball
                ball.savePosition();
                ball.moveByDeltaXY();
                Rectangle ballbb = ball.getBoundingBoxAA();

                if (ballbb.X + ballbb.Width > rhs)
                {
                    ball.setDeltaSpeed(ball.getDeltaSpeed() * new Vector2(-1, 1));
                }

                if (ballbb.X < lhs)
                {
                    ball.setDeltaSpeed(ball.getDeltaSpeed() * new Vector2(-1, 1));
                }

                if (ballbb.Y < top)
                {
                    ball.setDeltaSpeed(ball.getDeltaSpeed() * new Vector2(1, -1));
                }

                if (ballbb.Intersects(paddle.getBoundingBoxAA()))
                {
                    ball.setDeltaSpeed(ball.getDeltaSpeed() * new Vector2(1, -1));
                }

            }




            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            back1.Draw(spriteBatch); //***
            paddle.Draw(spriteBatch); //***
            ball.Draw(spriteBatch);
            sl.Draw(spriteBatch);
            
            if (showbb)
            {
                paddle.drawBB(spriteBatch, Color.Black);
                paddle.drawHS(spriteBatch, Color.Green);
                ball.drawInfo(spriteBatch, Color.Gray, Color.Green);
                sl.drawInfo(spriteBatch, Color.Brown, Color.Aqua);
                LineBatch.drawLineRectangle(spriteBatch, playArea, Color.Blue);
            }
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        // to be put into Game1 as a method to allow
        // loading textures from disk
        public static Texture2D texFromFile(GraphicsDevice gd, String fName)
        {
            // note needs :using System.IO;
            Stream fs = new FileStream(fName, FileMode.Open);
            Texture2D rc = Texture2D.FromStream(gd, fs);
            fs.Close();
            return rc;
        }

    }
}
