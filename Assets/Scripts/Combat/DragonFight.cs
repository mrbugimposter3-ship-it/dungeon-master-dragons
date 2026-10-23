using System.Collections;
using UnityEngine;

namespace DungeonMaster
{
    public class DragonFight : MonoBehaviour
    {
        public double playerHp = CombatMath.PlayerMaxHealth;
        public double dragonHp = CombatMath.DragonMaxHealth;
        public bool busy;
        public DragonAvatar avatar;

        public void ResetFight()
        {
            StopAllCoroutines();
            playerHp = CombatMath.PlayerMaxHealth;
            dragonHp = CombatMath.DragonMaxHealth;
            busy = false;
        }

        public void Choose(string move)
        {
            var game = GameManager.Instance;
            if (busy || game == null || game.State != PlayState.Combat) return;
            StartCoroutine(Resolve(move));
        }

        IEnumerator Resolve(string move)
        {
            busy = true;
            var game = GameManager.Instance;
            var profile = game.Inventory.CombatProfile();
            int roll = Random.Range(0, 21);
            var round = CombatMath.Resolve(move, profile, roll);
            if (!round.valid)
            {
                game.Hud.CombatLog("That's not an attack you can do.");
                busy = false;
                yield break;
            }

            game.Hud.CombatLog(round.moveName + " hits for " + (int)round.damageToDragon + ".");
            Sfx("swing");
            if (avatar != null) avatar.Flinch();
            yield return new WaitForSeconds(0.35f);

            dragonHp -= round.damageToDragon;
            if (round.crit)
            {
                game.Hud.CombatLog("Critical Hit!");
                Sfx("crit");
                game.Shake(0.35f);
            }

            game.Hud.CombatLog("The dragon breathes molten fire. -" + (int)profile.fireBreath + ".");
            if (avatar != null) avatar.Breathe();
            Sfx("roar");
            game.Shake(0.28f);
            yield return new WaitForSeconds(0.4f);

            if (round.boneCrush)
            {
                game.Hud.CombatLog("The Dragon crushes your bones. -" + (int)profile.boneCrush + ".");
                if (avatar != null) avatar.Smash();
                Sfx("hit");
                game.Shake(0.6f);
                yield return new WaitForSeconds(0.3f);
            }
            if (round.wingSmash)
            {
                game.Hud.CombatLog("Wing Smash! -" + (int)profile.wingSmash + ".");
                if (avatar != null) avatar.Smash();
                Sfx("hit");
                game.Shake(0.85f);
                yield return new WaitForSeconds(0.3f);
            }

            playerHp -= round.damageToPlayer;
            if (round.healed)
            {
                playerHp += round.healing;
                if (playerHp > CombatMath.PlayerMaxHealth) playerHp = CombatMath.PlayerMaxHealth;
                game.Hud.CombatLog("+60 Health bonus!");
                Sfx("heal");
            }

            if (playerHp < 0) playerHp = 0;
            if (dragonHp < 0) dragonHp = 0;
            game.Hud.CombatLog("You " + (int)playerHp + "    Dragon " + (int)dragonHp);
            game.Hud.RefreshCombat(playerHp, dragonHp, profile);

            // The original loop tested the dragon before the player, so a mutual kill is a win.
            if (dragonHp <= 0)
            {
                if (avatar != null) avatar.Die();
                Sfx("win");
                yield return new WaitForSeconds(1.15f);
                game.WinDragon();
            }
            else if (playerHp <= 0)
            {
                Sfx("lose");
                yield return new WaitForSeconds(0.55f);
                game.LoseDragon();
            }

            busy = false;
        }

        static void Sfx(string name)
        {
            if (SoundBoard.Instance != null) SoundBoard.Instance.Play(name);
        }
    }
}
