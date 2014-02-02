using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManagement
{
    class Country : IEnumerable<Country>
    {
        private bool m_changedHands = false;
        public bool cancelOutgoing = false;
        public bool isActive = false;
        public bool contIsControlled = false;

        public const float MIN_ARMY = 2;

        private List<Country> neighbors = new List<Country>();
        private Player owner;
        private Continent myContinent;
        private Vector2 location;
        private Vector2 labelOffset;
        private Texture2D image;
        private int strength;
        private static Random random = new Random();

        private Country(Vector2 loc, Texture2D img)
        {
            image = img;
            location = loc;
            labelOffset = new Vector2(0, 0);
        }

        public Country(Vector2 loc, Texture2D img, Player o, int s) 
            : this(loc, img)
        {
            owner = o;
            strength = s;
        }

        public Country(Vector2 loc, Vector2 offset, Texture2D img, Player o, int s)
            : this(loc, img, o, s)
        {
            labelOffset = offset;
        }

        public bool changedHands
        {
            get
            {
                return m_changedHands;
            }
            set
            {
                m_changedHands = value;
                if(value == true)
                    GameplayScreen.transferCountry.Play();
            }
        }

        public Vector2 getLocation()
        {
            return location;
        }

        public Vector2 getScaledLoc(float scale, Vector2 offset)
        {
            return location * scale + offset;
        }

        public Vector2 getScaledLabelLoc(float scale, Vector2 offset)
        {
            return (location + labelOffset) * scale + offset;
        }

        public void setLocation(Vector2 loc)
        {
            location = loc;
        }

        public Vector2 getLabelOffset()
        {
            return labelOffset;
        }

        public void setLabelOffset(Vector2 offset)
        {
            labelOffset = offset;
        }

        public void setContinent(Continent cont)
        {
            myContinent = cont;
        }

        public Continent getContinent()
        {
            return myContinent;
        }

        public Texture2D getImage()
        {
            return image;
        }

        public void setStrength(int s)
        {
            strength = s;
        }

        public void addStrength(int s)
        {
            strength += s;
        }

        public int getStrength()
        {
            return strength;
        }

        public bool hasStrength()
        {
            return strength >= MIN_ARMY;
        }

        public bool canAddMove()
        {
            return hasStrength() && !isActive;
        }

        public void setOwner(Player o)
        {
            owner = o;
        }

        public Player getOwner()
        {
            return owner;
        }

        public Color getColor(bool isLight)
        {
            if (isLight)
                return owner.getLightColor();
            else
                return owner.getDarkColor();
        }

        public void addNeighbor(Country n)
        {
            neighbors.Add(n);
        }

        public IEnumerator<Country> GetEnumerator()
        {
            return neighbors.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // return true if the transfer can be repeated
        public bool transfer(Country c, int n)
        {
            strength -= n;
            c.strength += n;
            return hasStrength();
        }

        // return true if the attack can be repeated
        public bool attack(Country def)
        {
            int result = random.Next(2);
            if (result == 0)
            {
                if(!owner.isInvincible())
                    strength -= 1;
            }
            else
            {
                if(!def.owner.isInvincible())
                    def.strength -= 1;

                if (def.strength <= 0)
                {
                    def.owner = owner;
                    def.strength = 1;
                    strength -= 1;
                    def.changedHands = true;
                    owner.captures += 1;
                }
            }
            return hasStrength();

        }
    }
}
