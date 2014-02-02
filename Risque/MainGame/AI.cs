using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManagement
{
    class AI : Player
    {
        public AI(GameplayScreen g, PlayerIndex n, Color c, Vector2 d)
            :base(g,n,c,d)
        {
        }

        public void place()
        {
            // we want to incentivize staying in continent, but disincinetivizing placing
            // next to multiple terrs we already control in said continent
            foreach (KeyValuePair<string, Country> cKV in gScreen.territories)
            {
                Country c = cKV.Value;
                if (c.getOwner() == GameplayScreen.noone)
                {
                    c.setOwner(this);
                    break;
                }
            }
        }

        public void update()
        {
            Dictionary<Continent, float> myContinents = priceContinents();
            // randomly choose a move type to do
            
            // first prune the bad movements, otherwise AI will be confused
            pruneBadMoves(myContinents);

            // 50% chance of using a power if it can be afforded
            if (randomDecision(0.5) && canUse(curPower))
                doBestPower(myContinents);
            // 50% chance of transfer if we get here
            else if (randomDecision(0.5))//0.9 * Math.Pow(1.5, -countMoves(Movement.Type.Transfer))))
                doBestTransfer(myContinents);
            // otherwise, we attack
            else
                doBestAttack(myContinents);
        }

        private bool randomDecision(double threshold)
        {
            double rand = gScreen.random.NextDouble();
            if (rand < threshold)
                return true;
            else
                return false;
        }

        private int countMoves(Movement.Type type)
        {
            int count = 0;
            foreach (Movement m in gScreen.movements)
            {
                if (m.owner == this && m.myType == type)
                    count++;
            }
            return count;
        }

        private void pruneBadMoves(Dictionary<Continent, float> myContinents)
        {
            for (int i = 0; i < gScreen.movements.Count; ++i)
            {
                Movement m = gScreen.movements[i];
                if (m.owner == this)
                {
                    if (m.myType == Movement.Type.Attack)
                    {
                        if (attackValue(m.dest, myContinents) < 0)
                        {
                            m.origin.isActive = false;
                            gScreen.movements.RemoveAt(i--);
                        }
                    }
                    else
                    {
                        float moveValue = defenseValue(m.dest, myContinents);
                        float originValue = defenseValue(m.origin, myContinents);
                        if (originValue > 0)
                            moveValue -= originValue;
                        if (moveValue < 0)
                        {
                            m.origin.isActive = false;
                            gScreen.movements.RemoveAt(i--);
                        }
                    }
                }
            }
        }

        private void doBestPower(Dictionary<Continent, float> myContinents)
        {
            // find my avg # of guys per frontlines territory
            // if above 10, will try to use defect rather than reinforce
            int terrs = 0;
            float guys = 0;
            foreach (KeyValuePair<string, Country> cKV in gScreen.territories)
            {
                Country c = cKV.Value;
                if (c.getOwner() == this)
                {
                    if(gScreen.isFrontLines(c))
                        terrs++;
                    guys += c.getStrength();
                }
            }

            if (guys / terrs >= 10)
            {
                setPower(Power.Defect);
                if (usePower())
                {
                    Country bestTarget = null;
                    float value = -1;
                    foreach (KeyValuePair<string, Country> cKV in gScreen.territories)
                    {
                        Country c = cKV.Value;
                        if (c.getOwner() != this)
                        {
                            float contVal;
                            myContinents.TryGetValue(c.getContinent(), out contVal);
                            float v = c.getStrength() * contVal;
                            if (v > value)
                            {
                                value = v;
                                bestTarget = c;
                            }
                        }
                    }
                    if(bestTarget != null)
                        gScreen.defect(this, bestTarget);
                }
            }
            else
            {
                setPower(Power.Reinforce);
                while (usePower())
                    gScreen.reinforce(this);
            }
        }

        private void doBestAttack(Dictionary<Continent, float> myContinents)
        {
            Country target = null;
            float bestVal = 1;
            foreach (KeyValuePair<string, Country> cKV in gScreen.territories)
            {
                Country c = cKV.Value;
                if (c.getOwner() != this)
                {
                    float value = attackValue(c, myContinents);
                    if (value > bestVal)
                    {
                        int inActives = 0;
                        foreach (Country n in c)
                        {
                            if (n.getOwner() == this && !n.isActive && n.hasStrength())
                            {
                                inActives++;
                            }
                        }
                        if (inActives > 0)
                        {
                            bestVal = value;
                            target = c;
                        }
                    }
                }
            }

            
            if (target != null)
            {
                foreach (Country n in target)
                {
                    if (n.getOwner() == this && !n.isActive && n.hasStrength())
                    {
                        gScreen.movements.Add(new Movement(this, n, target, Movement.Type.Attack));
                    }
                    //Console.Out.WriteLine("Attacking " + target.Key);
                }
            }
        }

        private void doBestTransfer(Dictionary<Continent, float> myContinents)
        {
            // find the best target
            Country target = null;
            Country origin = null;
            float bestVal = 0;
            foreach (KeyValuePair<string, Country> cKV in gScreen.territories)
            {
                Country c = cKV.Value;
                if (c.getOwner() == this)
                {
                    float value = defenseValue(c, myContinents);
                    // the value of a movement is the desirability of the target, minus desirability
                    // of the best origin (if the origin has positive desirability)
                    Country tmpOrigin = null;
                    float tmpBestVal = float.PositiveInfinity;
                    foreach (KeyValuePair<string, Country> nKV in gScreen.territories)
                    {
                        Country n = nKV.Value;
                        if (!n.isActive && gScreen.isValidPath(n, c))
                        {
                            float v = defenseValue(n, myContinents);
                            if (v < tmpBestVal)
                            {
                                tmpBestVal = v;
                                tmpOrigin = n;
                            }
                        }
                    }

                    if (tmpBestVal > 0)
                        value -= tmpBestVal;

                    if (value > bestVal)
                    {
                        bestVal = value;
                        target = c;
                        origin = tmpOrigin;
                    }
                }
            }

            // if a good move was found, do it
            if (target != null)
            {
                gScreen.movements.Add(new Movement(this, origin, target, Movement.Type.Transfer));
                //Console.Out.WriteLine("Attacking " + target.Key);
            }
        }

        // find out how much of each continent we control
        private Dictionary<Continent,float> priceContinents()
        {
            Dictionary<Continent, float> myContinents = new Dictionary<Continent, float>();
            for (int i = 0; i < gScreen.continents.Count; ++i)
            {
                Continent cont = gScreen.continents[i];

                // continents owned by another player are valuable
                if (cont.owner != GameplayScreen.noone && cont.owner != this)
                {
                    myContinents.Add(cont, 0.99f);
                }
                else
                {
                    // otherwise, base value on how many terrs we already control within cont
                    // unless another player controls almost the whole thing
                    Player otherOwner = GameplayScreen.noone;
                    foreach (Country c in cont)
                    {
                        if (c.getOwner() != this && c.getOwner() != GameplayScreen.noone)
                        {
                            otherOwner = c.getOwner();
                            break;
                        }
                    }

                    float myTerrs = 1;
                    float otherTerrs = 0;
                    foreach (Country c in cont)
                    {
                        if (c.getOwner() == this)
                            myTerrs++;
                        if (c.getOwner() == otherOwner)
                            otherTerrs++;
                    }

                    if (otherTerrs == cont.Count - 1)
                        myContinents.Add(cont, 0.99f);
                    else
                        myContinents.Add(cont, myTerrs / cont.Count);
                }
            }
            return myContinents;
        }

        private float attackValue(Country c, Dictionary<Continent, float> myContinents)
        {
            int attackers = 0;
            int myStrength = 0;
            foreach (Country n in c)
            {
                if (n.getOwner() == this && n.isActive == false)
                {
                    myStrength += adjustedStrength(n,1) - 1;
                    attackers++;
                }
            }

            // make sure to include any of our attacks in progress in the value
            foreach (Movement m in gScreen.movements)
            {
                if (m.dest == c && m.origin.getOwner() == this)
                {
                    myStrength += adjustedStrength(m.origin, 0);
                    attackers++;
                }
            }

            float theirStrength = adjustedStrength(c, attackers);

            // value is ratio of our strength to their's, higher is better
            float value = myStrength / theirStrength;
                
            // adjust desirability for continent value
            float contValue;
            myContinents.TryGetValue(c.getContinent(), out contValue);
            value = 1 + (value - 1)*contValue;
            return value;
        }

        private float defenseValue(Country c, Dictionary<Continent, float> myContinents)
        {
            float myStrength = adjustedStrength(c,0) - 1;
            float theirStrength = 0;
            float contValue;
            myContinents.TryGetValue(c.getContinent(), out contValue);

            foreach (Country n in c)
            {
                // enemy countries nearby make this a better target for a transfer
                if (n.getOwner() != this)
                {
                    theirStrength += adjustedStrength(n, 0);
                }

                // if country borders a continent we control, give it some bonus
                if (n.getContinent() != c.getContinent())
                {
                    float otherVal;
                    myContinents.TryGetValue(n.getContinent(), out otherVal);
                    if (otherVal * 0.9 > contValue)
                    {
                        contValue = otherVal * 0.9f;
                    }
                }
            }

            // value is ratio of their strength to ours (higher is more desirable target)
            float value = theirStrength / myStrength;

            // apply continent factors only to potential destinations
            if(value > 1)
                value = 1 + (value - 1) * contValue;
            // potential sorces become easier to steal from for low-value continents
            if (value < 1)
                value *= contValue;

            // if this country has no enemies, set value to negative its strength
            if (value == 0)
                value = -c.getStrength();

            return value;
        }

        // return the "true" strength of a territory including incoming transfers
        private int adjustedStrength(Country c, int attackBias)
        {
            int strength = c.getStrength();

            int attackers = attackBias;
            int transfers = 0;
            foreach (Movement m in gScreen.movements)
            {
                if (m.dest == c)
                {
                    if (m.origin.getOwner() == this)
                        transfers++;
                    else
                        attackers++;
                }
                if (m.origin == c)
                    attackers++;
            }

            // only count incoming transfers if there are more of them than outgoing transfers
            if (transfers >= attackers && transfers >= 0)
            {
                foreach (Movement m in gScreen.movements)
                {
                    if (m.dest == c && m.origin.getOwner() == this)
                        strength += m.origin.getStrength() - 1;
                }
            }

            return strength;
        }
    }
}
