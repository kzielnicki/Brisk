using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

/*
 * 2009 Luke Rymarz
 * 
 * Contact information at www.lukerymarz.com
 * 
  Basic Usage:
    1. Create an instance of a progress bar in your game1 class; 

    UI.ProgressBar progressBar; 

    2. In your LoadContent() function, set it to a new progressBar, and 
        provide the rectangle you want it to lie in. You can also set the 
        minimum, maximum, and value member variables here. The defaults 
        are 0, 100, and 0, respectively.

    progressBar = new UI.ProgressBar(this, new Rectangle(10, 10, 300, 16)); 
    progressBar.minimum = 0; 
    progressBar.maximum= 10000;

    3. In your Update() function, update the value of your progress bar if 
        necessary, and then call the progress bar's Update() function, 
        providing the gameTime (gameTime is not currently used in the 
        progress bar code, though).

    progressBar.value = character.health; 
    progressBar.Update(gameTime);

    4. In your Draw() function, call progressBar.Draw(), specifying a spriteBatch. 
        Make sure you're already called SpriteBatch.Begin() before calling the 
        progress bar Draw() function.

    topBar.Draw(spriteBatch);

 * 
 * */

namespace UI
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ProgressBar : Microsoft.Xna.Framework.GameComponent
    {
        #region public members
        /// <summary>
        /// Minimum value of the progress bar.  Default is 0.0f.
        /// </summary>
        public float minimum
        {
            get
            {
                return m_minimum;
            }
            set
            {
                m_minimum = value;
                // causes progress to update, and rectangles to update
                this.value = m_progress;
            }
        }
        
        /// <summary>
        /// Maximum value of the progress bar.  Default is 100.0f.
        /// </summary>
        public float maximum
        {
            get
            {
                return m_maximum;
            }
            set
            {
                m_maximum = value;
                // causes progress to update, and rectangles to update
                this.value = m_progress;
            }
        }

        /// <summary>
        /// Current progress value.
        /// </summary>
        public float value
        {
            get
            {
                return m_progress;
            }
            set
            {
                m_progress = value;
                if (m_progress < m_minimum)
                    m_progress = m_minimum;
                else if (m_progress > m_maximum)
                    m_progress = m_maximum;
                UpdateRectangles();
            }
        }

        /// <summary>
        /// Current target value.
        /// </summary>
        public float target
        {
            get
            {
                return m_target;
            }
            set
            {
                m_target = value;
                if (m_target < m_minimum)
                    m_target = m_minimum;
                else if (m_target > m_maximum)
                    m_target = m_maximum;
                UpdateRectangles();
            }
        }

        /// <summary>
        /// Outer border color.  Default is Gray.
        /// </summary>
        public Color borderColorOuter
        {
            get
            {
                return m_borderColorOuter;
            }
            set
            {
                if (m_borderColorOuter != value)
                {
                    m_borderColorOuter = value;
                    outerData[0] = m_borderColorOuter;
                    outerTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                                TextureUsage.Linear, SurfaceFormat.Color);
                    outerTexture.SetData(outerData);
                }
            }
        }

        public int getHeight()
        {
            return m_borderOuterRect.Height;
        }

        public int getWidth()
        {
            return m_borderOuterRect.Width;
        }

        public Vector2 getPos()
        {
            return new Vector2(m_borderOuterRect.X, m_borderOuterRect.Y);
        }

        /// <summary>
        /// Outer border thickness.  This is drawn within the bounds of the progress bar.  Default is 3.
        /// </summary>
        public Int32 borderThicknessOuter
        {
            get
            {
                return m_borderThicknessOuter;
            }
            set
            {
                m_borderThicknessOuter = value;
            }
        }
        
        /// <summary>
        /// Inner border color.  For situations where you will have multiple colors behind the progress bar.  
        /// Set this to something complementary to borderColorOuter.  Default is Black
        /// </summary>
        public Color borderColorInner
        {
            get
            {
                return m_borderColorInner;
            }
            set
            {
                if (m_borderColorInner != value)
                {
                    m_borderColorInner = value;
                    innerData[0] = m_borderColorInner;
                    innerTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                                TextureUsage.Linear, SurfaceFormat.Color);
                    innerTexture.SetData(innerData);
                }
            }
        }

        /// <summary>
        /// Inner border thickness.  This is drawn within the bounds of the progress bar.  Default is 2.
        /// </summary>
        public Int32 borderThicknessInner
        {
            get
            {
                return m_borderThicknessInner;
            }
            set
            {
                m_borderThicknessInner = value;
            }
        }

        /// <summary>
        /// Color of the progress section of the bar.  Default is Dark Blue.
        /// </summary>
        public Color fillColor
        {
            get
            {
                return m_fillColor;
            }
            set
            {
                if (m_fillColor != value)
                {
                    m_fillColor = value;
                    fillData[0] = m_fillColor;
                    fillTexture.Dispose();
                    fillTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                                TextureUsage.Linear, SurfaceFormat.Color);
                    fillTexture.SetData(fillData);
                }
            }
        }

        /// <summary>
        /// Color of the progress section of the bar.  Default is Dark Blue.
        /// </summary>
        public Color targetColor
        {
            get
            {
                return m_targetColor;
            }
            set
            {
                if (m_targetColor != value)
                {
                    m_targetColor = value;
                    targetData[0] = m_targetColor;
                    targetTexture.Dispose();
                    targetTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                                TextureUsage.Linear, SurfaceFormat.Color);
                    targetTexture.SetData(targetData);
                }
            }
        }

        /// <summary>
        /// Color of the progress section of the bar.  Default is Dark Blue.
        /// </summary>
        public Color targetTopColor
        {
            get
            {
                return m_targetTopColor;
            }
            set
            {
                if (m_targetTopColor != value)
                {
                    m_targetTopColor = value;
                    targetTopData[0] = m_targetTopColor;
                    targetTopTexture.Dispose();
                    targetTopTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                                TextureUsage.Linear, SurfaceFormat.Color);
                    targetTopTexture.SetData(targetTopData);
                }
            }
        }

        /// <summary>
        /// Color of the background (unfilled) section of the progress bar.  Default is White.
        /// </summary>
        public Color backgroundColor
        {
            get
            {
                return m_backgroundColor;
            }
            set
            {
                if (m_backgroundColor != value)
                {
                    m_backgroundColor = value;
                    backgroundData[0] = m_backgroundColor;
                    backgroundTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                            TextureUsage.Linear, SurfaceFormat.Color);
                    backgroundTexture.SetData(backgroundData);
                }
            }
        }

        public enum Orientation
        {
            HORIZONTAL_LR, // default, horizontal orientation, left to right fill
            HORIZONTAL_RL, // horizontal orientation, right to left fill
            VERTICAL_TB, // vertical orientation, top to bottom fill
            VERTICAL_BT, // vertical orientation, bottom to top fill
        }

        /// <summary>
        /// Gets the orientation of this progress bar.  Set at creation time.
        /// </summary>
        public Orientation orientation
        {
            get
            {
                return m_orientation;
            }
        }
        #endregion

        #region protected members
        protected float m_minimum = 0.0f;
        protected float m_maximum = 100.0f;
        protected float m_progress = 0;
        protected float m_target = 0;

        protected Rectangle m_borderOuterRect;
        protected Rectangle m_borderInnerRect;
        protected Rectangle m_backgroundRect;
        protected Rectangle m_fillRect;
        protected Rectangle m_targetRect;

        protected Color m_borderColorOuter;
        protected Int32 m_borderThicknessOuter;

        protected Color m_borderColorInner;
        protected Int32 m_borderThicknessInner;

        protected Color m_fillColor;
        protected Color m_targetColor;
        protected Color m_targetTopColor;
        protected Color m_backgroundColor;

        protected Color[] outerData;
        protected Color[] innerData;
        protected Color[] fillData;
        protected Color[] targetData;
        protected Color[] targetTopData;
        protected Color[] backgroundData;
        protected Texture2D outerTexture;
        protected Texture2D innerTexture;
        protected Texture2D backgroundTexture;
        protected Texture2D fillTexture;
        protected Texture2D targetTexture;
        protected Texture2D targetTopTexture;

        protected Orientation m_orientation;

        #endregion

        /// <summary>
        /// Construct a progress bar in the given rectangle with default HORIZONTAL_LR orientation.
        /// </summary>
        /// <param name="game">Your game.  Most likely, "this"</param>
        /// <param name="rect">The rectangle you wish the progress bar to occupy (includes borders).</param>
        public ProgressBar(Game game, Rectangle rect)
            : base(game)
        {
            m_borderOuterRect = rect;
            m_orientation = Orientation.HORIZONTAL_LR;

            Initialize();
        }

        /// <summary>
        /// Construct a progress bar in the given rectangle with the given orientation
        /// </summary>
        /// <param name="game">Your game.  Most likely, "this"</param>
        /// <param name="rect">The rectangle you wish the progress bar to occupy (includes borders).</param>
        /// <param name="orientation">The orientation of the progress bar.</param>
        public ProgressBar(Game game, Rectangle rect, Orientation orientation)
            : base(game)
        {
            m_borderOuterRect = rect;
            m_orientation = orientation;

            Initialize();
        }

        /// <summary>
        /// Construct a progress bar with x,y, width and height and with default HORIZONTAL_LR orientation.
        /// </summary>
        /// <param name="game">Your game.  Most likely, "this"</param>
        /// <param name="x">X-position of the top left corner of the progress bar.</param>
        /// <param name="y">Y-position of the top left corner of the progress bar.</param>
        /// <param name="width">Width of the progress bar.</param>
        /// <param name="height">Height of the progress bar.</param>
        public ProgressBar(Game game, Int32 x, Int32 y, Int32 width, Int32 height)
            : base(game)
        {
            m_borderOuterRect = new Rectangle(x, y, width, height);
            m_orientation = Orientation.HORIZONTAL_LR;

            Initialize();
        }

        /// <summary>
        /// Construct a progress bar with x,y, width and height and the given orientation.
        /// </summary>
        /// <param name="game">Your game.  Most likely, "this"</param>
        /// <param name="x">X-position of the top left corner of the progress bar.</param>
        /// <param name="y">Y-position of the top left corner of the progress bar.</param>
        /// <param name="width">Width of the progress bar.</param>
        /// <param name="height">Height of the progress bar.</param>
        /// /// <param name="orientation">The orientation of the progress bar.</param>
        public ProgressBar(Game game, Int32 x, Int32 y, Int32 width, Int32 height, Orientation orientation)
            : base(game)
        {
            m_borderOuterRect = new Rectangle(x, y, width, height);
            m_orientation = orientation;

            Initialize();
        }

        /// <summary>
        /// Initialize the Progress bar.  Called automatically from the constructor.
        /// </summary>
        public override void Initialize()
        {
            // create some textures.  These will actually be overwritten when colors are set below.
            outerTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                            TextureUsage.Linear, SurfaceFormat.Color);
            innerTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                            TextureUsage.Linear, SurfaceFormat.Color);
            backgroundTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                            TextureUsage.Linear, SurfaceFormat.Color);
            fillTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                            TextureUsage.Linear, SurfaceFormat.Color);
            targetTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                            TextureUsage.Linear, SurfaceFormat.Color);
            targetTopTexture = new Texture2D(Game.GraphicsDevice, 1, 1, 1,
                            TextureUsage.Linear, SurfaceFormat.Color);

            // initialize data arrays for building textures
            outerData = new Color[1];
            innerData = new Color[1];
            fillData = new Color[1];
            targetData = new Color[1];
            targetTopData = new Color[1];
            backgroundData = new Color[1];

            // initialize colors
            borderColorOuter = Color.Gray;
            borderColorInner = Color.Black;
            fillColor = Color.DarkBlue;
            targetColor = Color.Red;
            targetTopColor = Color.ForestGreen;
            backgroundColor = Color.White;

            // set border thickness
            m_borderThicknessInner = 2;
            m_borderThicknessOuter = 3;

            // calculate the rectangles for displaying the progress bar
            UpdateRectangles();

            base.Initialize();
        }

        /// <summary>
        /// Calculates the rectangles for displaying the progress bar.  
        /// Assumes m_borderOuterRect is already initialized.
        /// </summary>
        protected void UpdateRectangles()
        {
            // figure out inner border
            m_borderInnerRect = m_borderOuterRect;
            m_borderInnerRect.Inflate(m_borderThicknessOuter * -1, m_borderThicknessOuter * -1);

            // figure out background rectangle
            m_backgroundRect = m_borderInnerRect;
            m_backgroundRect.Inflate(m_borderThicknessInner * -1, m_borderThicknessInner * -1);

            // figure out fill rectangle based on progress.
            m_fillRect = m_backgroundRect;
            m_targetRect = m_backgroundRect;
            float percentProgress = (m_progress - m_minimum) / (m_maximum - m_minimum);
            float targetProgress = (m_target - m_minimum) / (m_maximum - m_minimum);
            // calculate fill properly according to orientation
            switch (m_orientation)
            {
                case Orientation.HORIZONTAL_LR:
                    m_fillRect.Width = (int)((float)m_fillRect.Width * percentProgress);
                    m_targetRect.Width = (int)((float)m_targetRect.Width * targetProgress); break;
                case Orientation.HORIZONTAL_RL:
                    // right to left means short the fill rect as usual, but it must justified to the right
                    m_fillRect.Width = (int)((float)m_fillRect.Width * percentProgress);
                    m_fillRect.X = m_backgroundRect.Right - m_fillRect.Width;
                    m_targetRect.Width = (int)((float)m_targetRect.Width * targetProgress);
                    m_targetRect.X = m_backgroundRect.Right - m_targetRect.Width;
                    break;
                case Orientation.VERTICAL_BT:
                    //justify the fill to the bottom
                    m_fillRect.Height = (int)((float)m_fillRect.Height * percentProgress);
                    m_fillRect.Y = m_backgroundRect.Bottom - m_fillRect.Height;
                    m_targetRect.Height = (int)((float)m_targetRect.Height * targetProgress);
                    m_targetRect.Y = m_backgroundRect.Bottom - m_targetRect.Height;
                    break;
                case Orientation.VERTICAL_TB:
                    m_fillRect.Height = (int)((float)m_fillRect.Height * percentProgress);
                    m_targetRect.Height = (int)((float)m_targetRect.Height * targetProgress); break;
                default:// default is HORIZONTAL_LR
                    m_fillRect.Width = (int)((float)m_fillRect.Width * percentProgress);
                    m_targetRect.Width = (int)((float)m_targetRect.Width * targetProgress); break;
            }
            
        }

        /// <summary>
        /// Draws the progress bar.  Call this in a spritebatch.Begin()/End() block.
        /// </summary>
        /// <param name="spriteBatch">Your spritebatch.  Make sure you have already called begin().</param>
        public void Draw(SpriteBatch spriteBatch)
        {         
            // draw the outer border
            spriteBatch.Draw(outerTexture, m_borderOuterRect, Color.White);
            // draw the inner border
            spriteBatch.Draw(innerTexture, m_borderInnerRect, Color.White);
            // draw the background color
            spriteBatch.Draw(backgroundTexture, m_backgroundRect, Color.White);
            // draw the progress
            if (m_progress < m_target)
            {
                spriteBatch.Draw(targetTexture, m_targetRect, Color.White);
                spriteBatch.Draw(fillTexture, m_fillRect, Color.White);
            }
            else
            {
                spriteBatch.Draw(fillTexture, m_fillRect, Color.White);
                spriteBatch.Draw(targetTopTexture, m_targetRect, Color.White);
            }
        }       
    }
}