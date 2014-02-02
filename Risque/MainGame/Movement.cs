using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameStateManagement
{
    class Movement
    {
        public enum Type {Attack, Transfer}
        public Type myType;
        public Country origin, dest;
        public Player owner;
        public double lastRan = 0;
        public static double frequency = 1.0; // run frequency in seconds

        public Movement(Player p, Country o, Country d, Type t)
        {
            owner = p;
            origin = o;
            dest = d;
            myType = t;
            o.isActive = true;
        }

        // return true if the movement can be repeated
        public bool perform(double time)
        {
            lastRan = time;
            if (myType == Type.Attack)
                return origin.attack(dest);
            if (myType == Type.Transfer)
                return origin.transfer(dest,1);

            return false;
        }
    }
}
