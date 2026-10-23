using System;

namespace DungeonMaster
{
    /// <summary>
    /// Turn resolution for the dragon fight.
    /// The 2014 Java fight constructed a brand-new Rooms object, so attack, defence,
    /// and fire resistance were always the defaults (3, 3, 1) no matter what you carried.
    /// These formulas take the live stats. At those defaults the numbers match the original
    /// (whirlwind 24, doom hammer 30, cobra strike 18, fire breath 13, bone crush 45, wing smash 50).
    /// Fire breath in the Java source grew as defence and fire resistance grew. That is inverted
    /// here: fire resistance reduces the breath, and defence reduces bone crush and wing smash.
    /// </summary>
    public static class CombatMath
    {
        public const double PlayerMaxHealth = 175;
        public const double DragonMaxHealth = 550;
        public const int BaseAttack = 3;
        public const int BaseDefence = 3;
        public const int BaseFireRes = 1;
        public const double HealAmount = 60;

        public struct Profile
        {
            public double whirlWind;
            public double doomHammer;
            public double cobraStrike;
            public double fireBreath;
            public double boneCrush;
            public double wingSmash;
        }

        public struct Round
        {
            public bool valid;
            public string moveName;
            public double damageToDragon;
            public double damageToPlayer;
            public double healing;
            public bool crit;
            public bool boneCrush;
            public bool wingSmash;
            public bool healed;
        }

        public static Profile Build(int attack, int defence, int fireRes)
        {
            attack = Math.Max(1, attack);
            defence = Math.Max(1, defence);

            var profile = new Profile();
            profile.whirlWind = 8.0 * attack;
            profile.doomHammer = 10.0 * attack;
            profile.cobraStrike = 6.0 * attack;

            // 13 is the original breath at defence 3, fire resistance 1.
            // Each point of fire resistance away from that base changes the breath by 7.5%.
            double breath = 13.0 * (1.0 - 0.075 * (fireRes - 1));
            if (breath < 3.0) breath = 3.0;
            if (breath > 40.0) breath = 40.0;
            profile.fireBreath = breath;

            // Original bone-crush formula, now fed the gear you are actually wearing.
            double denom = defence + Math.Max(1, fireRes);
            profile.boneCrush = 80.0 / denom + 25.0;

            // Wing smash was a flat 50. Armour softens it; unarmoured it stays 50.
            double smash = 50.0 - (defence - 3) * 1.15;
            if (smash < 18.0) smash = 18.0;
            profile.wingSmash = smash;
            return profile;
        }

        /// <summary>
        /// roll is the original 0..20 inclusive fate number, rolled once per turn.
        /// </summary>
        public static Round Resolve(string move, Profile profile, int roll)
        {
            var round = new Round();
            double baseHit = 0;
            double critMultiplier = 0;
            bool crit = false;
            bool heal = false;

            switch (move)
            {
                case "ww":
                    round.moveName = "Whirlwind";
                    baseHit = profile.whirlWind;
                    if (roll == 4 || roll == 5 || roll == 6 || roll == 9)
                    {
                        crit = true;
                        critMultiplier = 5;
                    }
                    if (roll == 10 || roll == 9) heal = true;
                    break;
                case "dh":
                    round.moveName = "Doom Hammer";
                    baseHit = profile.doomHammer;
                    if (roll == 4 || roll == 5)
                    {
                        crit = true;
                        critMultiplier = 3;
                    }
                    if (roll == 10) heal = true;
                    break;
                case "cs":
                    round.moveName = "Cobra Strike";
                    baseHit = profile.cobraStrike;
                    if (roll == 4 || roll == 5 || roll == 6)
                    {
                        crit = true;
                        critMultiplier = 6;
                    }
                    if (roll == 10 || roll == 9 || roll == 8 || roll == 7) heal = true;
                    break;
                default:
                    round.valid = false;
                    round.moveName = "None";
                    return round;
            }

            round.valid = true;
            round.crit = crit;
            round.damageToDragon = baseHit + (crit ? baseHit * critMultiplier : 0);
            round.damageToPlayer = profile.fireBreath;

            if (roll == 18 || roll == 20)
            {
                round.boneCrush = true;
                round.damageToPlayer += profile.boneCrush;
            }

            if (roll == 19)
            {
                round.wingSmash = true;
                round.damageToPlayer += profile.wingSmash;
            }

            if (heal)
            {
                round.healed = true;
                round.healing = HealAmount;
            }

            return round;
        }
    }
}
