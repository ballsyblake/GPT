using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Xna.Framework.Storage;

#pragma warning disable 1591

using RC_Framework;

namespace RC_Framework
{
    // ------------------------------------- square mask ---------------------------------------------

    public class MicroGame1 : RC_Renderable
    {
        //public Vector2 pos;
        public float baseHeight = 300; // all sizes in pixels should relate to this size screen 
        public float baseWidth = 200;

        int mgState = 0; // game state
        // 0=waiting startup
        // 1=playing
        // 2=waiting for respawn
        // 3=Ending count You Win           
        // 4=Ending count You Looser
        // 5=Game over You Winner
        // 6=Game over You Looser

        public int playStyle = 2; // 0=playing-has focus 1=frozen 2=autoplay
        
        /// <summary>
        /// True makes a mouse click give it focus
        /// </summary>
        public bool mouseFocus = false; // True makes a mouse click give it focus
        int timeIn2 = 0;
        public int timeIn2Max = 60;
        int timeIn5or6 = 0;
        public int timeIn5or6Max = 300;
                
        public bool mgWin; // true if won the game
        public int mgScore; // the score

        int endingCount = 0;
        int endingCountMax = 60;

        int lives;
        public int maxLives=3;

        public Rectangle outerRectangle; // what the game is played in
        int widthOfMask=50;
        Rectangle innerRectangle; // what the game is played in 
        
        public Color maskColor;

        public int scoreIfTargetHit = 1; //mgScore
        public int scoreIfPlayerHit = -2;
        public int scoreIfTargetEscapes = -1;

        //float sizeRatioX = 1; 
        //float sizeRatioY = 1;

        SpriteList targets;
        public Texture2D targetTex = null;
        Vector2 targetSize;
        Vector2 actualTargetSize; 
        int numOfTargets=0;
        int numOfTargetsMax=30;

        public Texture2D shooterTex=null;
        Vector2 shooterSize; 
        Vector2 actualShooterSize;
        public Sprite3 shooter;
        float shooterSpeed = 1;

        Vector2 shooterShellOffset = new Vector2(1, 1);
        //int shots; 
        int shotsMax;
        public Color shotColor = Color.CadetBlue;
        public int shotWidth=3;
        public int shotLength=9;
        public bool shootThrough = false; // false if explosions kill bullets

        Texture2D explodeTex;
        Vector2 explodeSize;
        Vector2 actualExplodeSize;
        RC_RenderableList explosions;
        public Color explodeColourStart = Color.White;
        public Color explodeColourEnd = new Color(255,25,255,0);

        Vector2[] bullets;
        int maxMaxBullets=10;
        public float bulletSpeed = 3;

        private KeyboardState prevKeyState;

        public float genAliensPercent = 0.5f; // ????
        Random rnd;
        SpriteFont font;

        public String msg0 = "Press Space To Start";
        public String msg3 = "Winning";
        public String msg5 = "Game over You Win";
        public String msg6 = "Game over You Loose";
        public String msg56 = "press p to play";

        RC_RenderableList eyeCandy;

        RC_RenderableBounded backGround=null;
        int ticks = 0; // timer

        int AIMoveLevel = 10; // 1 = best 120 = really bad
        int AIShootLevel = 20; // 1 = silly   10-30 ok   120 = really bad
        float AI_x;
        float AI_xxx; // set when alien generated
        float prevX;

        public bool showbb = false;

        public MicroGame1(Rectangle outerRect, int widthOfMaskZ, Color maskColorZ, SpriteFont fontZ)
        {
            //for standard size use the line below
            //new MicroGame1(new Rectangle(100,100,300,400),50,Color.Magenta);
            
            maskColor = maskColorZ;
            resize(outerRect, widthOfMaskZ);
            bullets = new Vector2[maxMaxBullets];
            rnd = new Random();
            font = fontZ;
        }

        public override void reset()
        {
            mgScore = 0;
            numOfTargets = 0;
            targets = new SpriteList();
            explosions = new RC_RenderableList();
            eyeCandy = new RC_RenderableList();
            //numOfTargetsMax = 30;
            for (int i = 0; i < innerRectangle.X; i++) rnd.Next(0, 100); // alter random number
            lives = maxLives;
            endingCount = 0;
            timeIn5or6 = 0;
            mgState = 0;
        }

        public void setShooter(Texture2D tex, Vector2 size, int shotsZ)
        {
        shooterTex=tex;
        shooterSize.X = size.X; 
        shooterSize.Y = size.Y;
        shotsMax = shotsZ;
        sizeTheShooter();
        }

        void sizeTheShooter()
        {
        actualShooterSize = new Vector2(shooterSize.X * innerRectangle.Width / baseWidth, shooterSize.Y * innerRectangle.Height / baseHeight);

        shotWidth = (int)(actualShooterSize.X * 0.1f);
        shotLength = (int)(actualShooterSize.Y * 0.6f);
        shooterShellOffset = new Vector2(actualShooterSize.X / 2 - shotWidth / 2,0);
        if (shotWidth < 1) shotWidth = 1;
        if (shotLength < 1) shotLength = 1;

        float xx = innerRectangle.X + innerRectangle.Width / 2;
        float yy = innerRectangle.Y + innerRectangle.Height - actualShooterSize.Y;
        shooter = new Sprite3(true, shooterTex, xx, yy);
        shooter.setWidthHeight(actualShooterSize.X,actualShooterSize.Y);
        shooter.setBBToTexture();
        }

        public void setTarget(Texture2D tex, Vector2 size, int maxNumOfTargets)
        {
            numOfTargetsMax=maxNumOfTargets;
            targetTex = tex;
            targetSize.X = size.X;
            targetSize.Y = size.Y;
            actualTargetSize = new Vector2(targetSize.X * innerRectangle.Width / baseWidth, targetSize.Y * innerRectangle.Height / baseHeight);
        }

        public void setExplode(Texture2D tex, Vector2 size)
        {
            explodeTex = tex;
            explodeSize.X = size.X;
            explodeSize.Y = size.Y;
            actualExplodeSize = new Vector2(explodeSize.X * innerRectangle.Width / baseWidth, explodeSize.Y * innerRectangle.Height / baseHeight);
        }

        /// <summary>
        /// Importantly this routine sets the bounds of the Renderable to inner rectangle
        /// </summary>
        /// <param name="b"></param>
        public void setBackground(RC_RenderableBounded b) // set null if none
        {
            backGround = b;
            b.bounds = new Rectangle(innerRectangle.X,innerRectangle.Y,innerRectangle.Width,innerRectangle.Height);
        }

        public void setPlayType(int playStyleZ, bool mouseFocusZ)
        {
        playStyle = playStyleZ; // 0=playing-has focus 1=frozen 2=autoplay
        mouseFocus = mouseFocusZ; // True makes a mouse click give it focus
        }

        void makeExplosion(Vector2 pos)
        {
            TextureFade t = new TextureFade(explodeTex,
                new Rectangle((int)(pos.X),(int)(pos.Y),(int)(actualExplodeSize.X),(int)(actualExplodeSize.Y)), 
                new Rectangle((int)(pos.X-5),(int)(pos.Y-5),(int)(actualExplodeSize.X+5),(int)(actualExplodeSize.Y+5)),
                explodeColourStart, explodeColourEnd, 20);
            t.loop=0;
            explosions.addReuse(t);
        }


        public void resize(Rectangle outerRect, int widthOfMaskZ)
        {

            outerRectangle.X = outerRect.X;
            outerRectangle.Y = outerRect.Y;
            outerRectangle.Width = outerRect.Width;
            outerRectangle.Height = outerRect.Height;

            widthOfMask = widthOfMaskZ;

            innerRectangle.X = outerRectangle.X+widthOfMask;
            innerRectangle.Y = outerRectangle.Y+widthOfMask;
            innerRectangle.Width = outerRectangle.Width-widthOfMask*2;
            innerRectangle.Height = outerRectangle.Height-widthOfMask*2;
            AI_xxx =innerRectangle.X+innerRectangle.Width/2;
            AI_x = AI_xxx;

        }

        void drawBack(SpriteBatch sb)
        {
            //sb.Draw(LineBatch._empty_texture, innerRectangle, backColor);
            if (backGround!=null) backGround.Draw(sb);
        }

        void drawMask(SpriteBatch sb)
        {
            int xx = outerRectangle.X + outerRectangle.Width - widthOfMask; // right x pos
            int yy = outerRectangle.Y + outerRectangle.Height - widthOfMask; // bottom y pos
            int hh = outerRectangle.Height - widthOfMask*2; // height of middle
            
            Rectangle r = new Rectangle(outerRectangle.X, outerRectangle.Y, outerRectangle.Width, widthOfMask);
            sb.Draw(LineBatch._empty_texture, r, maskColor);
            
            r = new Rectangle(outerRectangle.X, outerRectangle.Y + widthOfMask, widthOfMask, hh);
            sb.Draw(LineBatch._empty_texture, r, maskColor);
            
            r = new Rectangle(xx, outerRectangle.Y + widthOfMask, widthOfMask, hh);
            sb.Draw(LineBatch._empty_texture, r, maskColor);
            
            r = new Rectangle(outerRectangle.X, yy, outerRectangle.Width, widthOfMask);
            sb.Draw(LineBatch._empty_texture, r, maskColor);
        }

        void fire()
        {
            if (mgState != 1 && mgState != 3) return; 

            for (int i = 0; i < shotsMax; i++)
            {
                if (bullets[i].X == 0)
                {
                    bullets[i].X = shooter.getPosX() + shooterShellOffset.X;
                    bullets[i].Y = shooter.getPosY() + shooterShellOffset.Y + 1 - shotLength;
                    break;
                }
            }
        }

        void drawBullets(SpriteBatch sb)
        {
            for (int i = 0; i < shotsMax; i++)
            {
                if (bullets[i].X != 0)
                {
                    for (int j = 0; j < shotWidth; j++)
                    {
                        LineBatch.drawLine(sb, shotColor, bullets[i]+new Vector2(j,0), bullets[i] + new Vector2(j, shotLength));
                    }
                }
            }
        }

        void moveBullets()
        {
            for (int i = 0; i < shotsMax; i++)
            {
                if (bullets[i].X != 0)
                {
                    //bullets[i].X = shooter.getPosX() + shooterShellOffset.X;
                    bullets[i].Y = bullets[i].Y - bulletSpeed;
                    if (bullets[i].Y < innerRectangle.Y - shotLength) bullets[i].X=0;
                }
                if (bullets[i].X != 0)
                {
                    // check for collision
                    int tt = targets.pointInList(bullets[i]);
                    if (tt == -1) tt = targets.pointInList(bullets[i] + new Vector2(0, shotLength));
                    if ( tt !=-1 )
                    {
                        Sprite3 s = targets.getSprite(tt);
                        makeExplosion(s.getPos());
                        s.visible = false;
                        s.active = false;
                        adjustScore(scoreIfTargetHit, s.getPosX(), s.getPosY());
                        if (!shootThrough) bullets[i].X=0; // remove bullet
                    }
                }
            }
        }

        void adjustScore(int amount, float x, float y)
        {            
            // to not display the fade text make x < 0
            mgScore = mgScore + amount;
            if (x > 0)
            {
                TextRenderableFade r = new TextRenderableFade(amount.ToString(), new Vector2(x, y), font,
                                       Color.Black, new Color(0, 255, 0, 9), 30);
                eyeCandy.addReuse(r);
            }
        }

        void checkIfIAmShot()
        {
            Rectangle r = shooter.getBoundingBoxAA();
            int temp = targets.collisionWithRect(r);
            if (temp == -1) return;
            Sprite3 s = targets.getSprite(temp);
            makeExplosion(shooter.getPos());
            makeExplosion(s.getPos());
            adjustScore(scoreIfPlayerHit, s.getPosX(), s.getPosY());
            s.active = false;
            s.visible = false;
            //shooter.visible = false;
            lives = lives -1;
            mgState = 2;
            timeIn2 = 0;
            if (lives <= 0) mgState = 4;
        }

        void generateAliens()
        {
            if (numOfTargets >= numOfTargetsMax) { mgState = 3; return; };

            if (rnd.Next(0,1000) < genAliensPercent*10 && numOfTargets < numOfTargetsMax)
            {
                numOfTargets++;
                float xx =rnd.Next(innerRectangle.X, (int)(innerRectangle.X + innerRectangle.Width - actualTargetSize.X));
                float yy = innerRectangle.Y - actualTargetSize.Y / 2 ; 
                Sprite3 s = new Sprite3(true, targetTex,xx, yy);
                s.setWidthHeight(actualTargetSize.X, actualTargetSize.Y);
                s.setBBToTexture();

                //s.setWidthHeightOfTex(1536, 384);
                //s.setXframes(16);
                //s.setYframes(4);
                s.setDeltaSpeed(new Vector2(0, 1));
                //s.setBB(10, 10, 48 - 20, 48 - 20);

                //int frames = 12;
                //Vector2[] anim = new Vector2[frames];
                //for (int i = 0; i < frames; i++)
                //{
                //    anim[i].X = i; anim[i].Y = 0;
                //}
                //s.setAnimationSequence(anim, 0, frames - 1, 3);
                //s.animationStart();
                targets.addSpriteReuse(s);
                AI_xxx = s.getPosX() + s.getWidth() / 2;

            }
        }

        public override void Draw(SpriteBatch sb)
        {
            //int mgStates = 0; // game state
            // 0=waiting startup
            // 1=playing
            // 2=waiting for respawn
            // 3=Ending count You Win           
            // 4=Ending count You Looser
            // 5=Game over You Winner
            // 6=Game over You Looser

            sb.Begin(SpriteSortMode.Deferred,BlendState.NonPremultiplied);
            drawBack(sb);
            if (mgState == 0 || mgState == 1 || mgState == 3) shooter.Draw(sb);
            drawBullets(sb);
            targets.Draw(sb);
            if (showbb)
            {
                targets.drawInfo(sb, Color.Beige, Color.Gray);
                shooter.drawInfo(sb, Color.Beige, Color.Gray);
            }
            explosions.Draw(sb);
            eyeCandy.Draw(sb);
            drawMask(sb);
            
            // draw lives 
            for (int i = 0; i < lives; i++)
            {
                //lives
                Rectangle r = new Rectangle((int)(outerRectangle.X + 2 + i * (actualShooterSize.X+4)), (int)(innerRectangle.Y+innerRectangle.Height + 2), (int)actualShooterSize.X - 2, (int)actualShooterSize.Y - 2);
                sb.Draw(shooterTex,r, Color.White);
            }
            
            // draw score
            sb.DrawString(font,mgScore.ToString(), new Vector2(outerRectangle.X + 2, outerRectangle.Y + 1), Color.Black);

            drawTexts(sb);

            sb.End();
        }

        void drawTexts(SpriteBatch sb)
        {
            // drawText
            if (mgState == 0) //0=waiting startup
            {
                sb.DrawString(font, msg0, new Vector2(innerRectangle.X + 1, innerRectangle.Y + 4), Color.Black);
            }
            if (mgState == 3)// 3=Ending count You Win
            {
                sb.DrawString(font, msg3, new Vector2(innerRectangle.X + 1, innerRectangle.Y + 4), Color.Black);
            }
            if (mgState == 5) // 5=Game over You Winner
            {
                sb.DrawString(font, msg5, new Vector2(innerRectangle.X + 1, innerRectangle.Y + 4), Color.Black);
            }
            if (mgState == 6) // 6=Game over You Looser
            {
                sb.DrawString(font, msg6, new Vector2(innerRectangle.X + 1, innerRectangle.Y + 4), Color.Black);
            }
            if ((mgState == 6 || mgState == 5) && (playStyle != 2)) // 6=Game over You Looser
            {
                sb.DrawString(font, msg56, new Vector2(innerRectangle.X + 1, innerRectangle.Y + 25), Color.Black);
            }

        }

        void UpdateMG6()
        {
            //the game is waiting to start
            if (playStyle == 2) // 0=playing-has focus 1=frozen 2=autoplay)
            {
                if (rnd.Next(0, 100) < 2 && timeIn5or6 > timeIn5or6Max) reset();
            }

            KeyboardState keyState = Keyboard.GetState();
            if (playStyle == 0 && keyState.IsKeyDown(Keys.P))
            {
                reset();
            }
        }

        void UpdateMG5()
        {
            //the game is waiting to start
            if (playStyle == 2) // 0=playing-has focus 1=frozen 2=autoplay)
            {
                if (rnd.Next(0, 100) < 2 && timeIn5or6 > timeIn5or6Max) reset();
            }

            KeyboardState keyState = Keyboard.GetState();
            if (playStyle == 0 && keyState.IsKeyDown(Keys.P))
            {
                reset();
            }
        }

        void UpdateMG0()
        {
            //the game is waiting to start
            if (playStyle == 2) // 0=playing-has focus 1=frozen 2=autoplay)
            {
                if (rnd.Next(0, 100) < 2) mgState = 1;
            }

            KeyboardState keyState = Keyboard.GetState();
            if (playStyle == 0 && keyState.IsKeyDown(Keys.Space))
            {
                mgState = 1;
            }

        }

        public override void Update(GameTime gameTime)
        {
            //int mgStates = 0; // game state
            // 0=waiting startup
            // 1=playing
            // 2=waiting for respawn
            // 3=Ending count You Win           
            // 4=Ending count You Looser
            // 5=Game over You Winner
            // 6=Game over You Looser

            ticks++;
            if (mgState == 2) timeIn2++;
            if (mgState == 5 || mgState == 6) timeIn5or6++;

            if (playStyle == 1) return; // frozen

            KeyboardState keyState = Keyboard.GetState();

            if (mgState == 3 || mgState == 4) endingCount++;
            if (mgState == 3 && targets.countActive() == 0 && endingCount > endingCountMax) mgState = 5;
            if (mgState == 4 && endingCount > endingCountMax) mgState = 6;

            if (mgState == 1 || mgState == 3)
            {
                checkIfIAmShot();
            }

            if (mgState == 2)
            {
                if (targets.countActive() == 0 && timeIn2 > timeIn2Max)
                {
                    mgState = 1;
                }
            }
            prevX = shooter.getPosX();
            if (playStyle == 2) automove();// auto playing fire();

            if (playStyle == 0) // playing using keyboard
            {

               
                if (keyState.IsKeyDown(Keys.Space) && prevKeyState.IsKeyUp(Keys.Space))
                {
                    fire();
                }
                if (keyState.IsKeyDown(Keys.Left))
                {
                    shooter.setPosX(shooter.getPosX() - shooterSpeed);
                    if (shooter.getPosX() < innerRectangle.X) shooter.setPosX(prevX);
                }
                if (keyState.IsKeyDown(Keys.Right))
                {
                    shooter.setPosX(shooter.getPosX() + shooterSpeed);
                    if (shooter.getPosX() + actualShooterSize.X > innerRectangle.X + innerRectangle.Width) shooter.setPosX(prevX);

                }

                if (keyState.IsKeyDown(Keys.B) && !prevKeyState.IsKeyDown(Keys.B))
                {
                    showbb = !showbb;
                }

                //moveBullets();
                //if (mgState == 1) generateAliens();
                //targets.moveDeltaXY_A(true);
                //int i = targets.removeIfOutside(innerRectangle);
                //adjustScore(i * scoreIfTargetEscapes, -900, -900);
                //targets.animationTick();
            }

            if (playStyle == 2 || playStyle == 0)
             {                
                moveBullets();
                if (mgState == 1) generateAliens();
                targets.moveDeltaXY();
                int i = targets.removeIfOutside(innerRectangle);
                adjustScore(i * scoreIfTargetEscapes, -900, -900);
                targets.animationTick(gameTime);
                explosions.Update(gameTime);
                eyeCandy.Update(gameTime);
            }

            if (backGround != null) backGround.Update(gameTime);
            if (mgState == 0) UpdateMG0();
            if (mgState == 5) UpdateMG5();
            if (mgState == 6) UpdateMG6();
            prevKeyState = keyState;
        }

        public override bool MouseDownEventLeft(float mouse_x, float mouse_y)
        {
            //   public int playStyle = 0; // 0=playing-has focus 1=frozen 2=autoplay
            //public bool mouseFocus = false; // True makes a mouse click give it focus
            if (outerRectangle.Contains(new Point((int)(mouse_x), (int)mouse_y)))
            {
            if (mouseFocus)
                {
                if (playStyle == 0)playStyle =1;else playStyle=0; 
                return true;
                }
            }
            return false;
        }

        void automove() // auto playing fire(); AI_x
        {
            if (shooter.getPosX()+shooter.getWidth()/2 > AI_x )
            {
                shooter.setPosX(shooter.getPosX() - shooterSpeed);
                if (shooter.getPosX() < innerRectangle.X) shooter.setPosX(prevX);
            }
            if (shooter.getPosX() + shooter.getWidth() / 2 < AI_x)
            {
                shooter.setPosX(shooter.getPosX() + shooterSpeed);
                if (shooter.getPosX() + actualShooterSize.X > innerRectangle.X + innerRectangle.Width) shooter.setPosX(prevX);

            }
            if (ticks % AIMoveLevel == 0)
            {
                AI_x = AI_xxx;
            }
            if (ticks % AIShootLevel == 0 && targets.countActive() > 0)
            {
                fire();
            }

        }
    }
}
