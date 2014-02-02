using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace GameStateManagement
{
    class Doodad
    {
        public Vector2 location;
        public Texture2D image;
        public Color color;

        public Doodad(Vector2 loc, Texture2D img, string imgName, Color col)
        {
            location = loc;
            image = img;
            image.Name = imgName;
            color = col;
        }

        public Vector2 getScaledLoc(float scale, Vector2 offset)
        {
            return location * scale + offset;
        }
    }
}
