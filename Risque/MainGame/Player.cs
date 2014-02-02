using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManagement
{
    class Player
    {
        public PlayerIndex PI;
        public enum Tgt { Self, Enemy, Invalid }
        public Tgt target = Tgt.Invalid;
        public Country origin;
        public Country dest;
        public Country hover;
        public Vector2 pos = new Vector2(100, 100);
        public Vector2 display;
        public bool selected = false;
        public int captures = 0;
       
        private double invUntil = 0;
        private double invDuration = 0;

        private UI.ProgressBar progressBar;
        protected GameplayScreen gScreen;
        private Color myColor;
        private Color darkColor;

        public enum Power
        {
            Reinforce,
            Transfer,
            Invincible,
            Defect,
        }

        public static int MAX_POWER = 10;
        public static int[] POW_COST = new int[4] { 3, 4, 7, 10 };
        public Power curPower = Power.Reinforce;

        public Player(GameplayScreen g, PlayerIndex n, Color c, Vector2 d)
        {
            gScreen = g;
            PI = n;
            myColor = c;
            darkColor = Color.Lerp(myColor, Color.Black, 0.3f);
            display = d;
            progressBar = new UI.ProgressBar(g.ScreenManager.Game, new Rectangle((int)d.X, (int)d.Y, 300, 16));
            progressBar.maximum = MAX_POWER;
            progressBar.fillColor = Color.White;
            progressBar.borderColorInner = GameplayScreen.OCEAN;
            progressBar.backgroundColor = GameplayScreen.OCEAN;
            progressBar.target = POW_COST[(int)curPower];
        }

        public void incPower()
        {
            curPower += 1;
            if (curPower > Power.Defect)
                curPower = Power.Reinforce;
            progressBar.target = POW_COST[(int)curPower];
        }

        public void decPower()
        {
            curPower -= 1;
            if (curPower < Power.Reinforce)
                curPower = Power.Defect;
            progressBar.target = POW_COST[(int)curPower];
        }

        public void setPower(Power pow)
        {
            curPower = pow;
            progressBar.target = POW_COST[(int)curPower];
        }

        public bool canUse(Power pow)
        {
            if (captures >= POW_COST[(int)pow])
                return true;
            return false;
        }

        public bool usePower()
        {
            if (!canUse(curPower))
                return false;

            if (curPower == Power.Defect && hover.getOwner() == this && this.GetType() == typeof(Player))
                return false;

            captures -= POW_COST[(int)curPower];
            return true;
        }

        public Color getLightColor()
        {
            return myColor;
        }

        public Color getDarkColor()
        {
            return darkColor;
        }

        public bool isInvincible()
        {
            return invUntil > gScreen.totalGameTime;
        }

        public void becomeInvincible(double duration)
        {
            invDuration = duration;
            invUntil = gScreen.totalGameTime + duration;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int num = (int)PI + 1;
            if (isInvincible())
            {
                Vector2 fontDim = gScreen.gameFontSmall.MeasureString(" P " + num + "");
                Vector2 boxDim = new Vector2(progressBar.getWidth(), progressBar.getHeight() + fontDim.Y);
                boxDim.X *= (float)((invUntil - gScreen.totalGameTime)/invDuration);
                spriteBatch.Draw(gScreen.whitePixel, new Rectangle((int)display.X, (int)display.Y, (int)boxDim.X, (int)boxDim.Y), Color.Gold);
            }
            spriteBatch.DrawString(gScreen.gameFontSmall, " P" + num + "", display + new Vector2(0, progressBar.getHeight()), darkColor);

            if (captures > MAX_POWER)
                captures = MAX_POWER;
            progressBar.value = captures;
            progressBar.Draw(spriteBatch);

            Color powColor;
            if (canUse(curPower))
                powColor = Color.Black;
            else
                powColor = Color.Red;
            spriteBatch.DrawString(gScreen.gameFontSmall, curPower.ToString(), display + new Vector2(60, progressBar.getHeight()), powColor);
        }
    }
}
