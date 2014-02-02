using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameStateManagement
{
    class Continent : List<Country>
    {
        public string name;
        public Player owner;
        public int value;

        public Continent(string n, int v)
            : base()
        {
            name = n;
            value = v;
            owner = GameplayScreen.noone;
        }

        new public void Add(Country c)
        {
            base.Add(c);
            c.setContinent(this);
        }
    }
}
