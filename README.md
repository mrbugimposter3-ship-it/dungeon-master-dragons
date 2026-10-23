# Dungeon Master: The Great Dragon Lair

A playable 3D remake of the 2014 Java text adventure [DUNGEON MASTER](https://github.com/bonako/projects/tree/HEAD/DungeonMaster/src/dungeonMaster) by bonako. The volcanic island, the rooms, the items, the torch, and the turn-based dragon fight are the same journey. Movement, gear, and the three hazards are no longer lines in a console.

## Unity version

**6000.3.27f1** (Unity 6.3 LTS). The version is pinned in `ProjectSettings/ProjectVersion.txt`.

The project uses the Universal Render Pipeline (`com.unity.render-pipelines.universal` 17.3.0) and uGUI. The first time the editor opens, `Assets/Editor/UrpProjectSetup.cs` creates the pipeline assets under `Assets/Settings/` and assigns them. Materials also fall back to the built-in Standard shader if no pipeline is active, so a missing pipeline shows up as flat lighting rather than a compile error.

## Open and play

1. Install **Unity 6000.3.27f1** with the Unity Hub (Windows, macOS, or Linux).
2. Hub → **Add** → **Add project from disk**, and choose this folder.
3. Open `Assets/Scenes/Main.unity`. If the editor starts on an empty scene, the setup script opens Main once packages have imported.
4. Wait until the Console stops compiling. The first import creates `Assets/Settings/URP_Asset.asset`.
5. Press **Play**.

If the island is magenta, use the menu **Dungeon Master → Setup URP**, then press Play again.

The scene itself is only a camera. `GameBootstrap` builds the island, the player, and the UI when Play starts, so the scene file does not depend on script GUIDs.

## Controls

| Input | Action |
| --- | --- |
| W A S D | Move |
| Mouse | Look |
| Shift | Sprint |
| Space | Short hop |
| V | Third person / first person |
| E | Use what is in front of you |
| I | Pack |
| J | Island story |
| M | Expand or shrink the minimap |
| Esc | Pause, or close the open panel |
| 1 2 3 | Whirlwind, Doom Hammer, Cobra Strike |

Walk up to an item and press E. The pack shows attack, defence, and fire resistance for the selected item. The panel on the left is the live total the dragon uses.

## The island

North is +Z. Each cell is 36 metres. The minimap letters match the 2014 map, except the three places that used to share the letter H:

| Letter | Place | What is there |
| --- | --- | --- |
| M | Mithril Mine | Cloth armour, mithril axe. Dark past the mouth: a lit torch is required. |
| F | Dark Farms | Iron sword, Zamorak ice whip. The barn is the same. |
| A | Adamantine Keep | Sewer key, Zamorak kiteshield, poison cheese, bucket of water. |
| G | Guild of Scrolls | Chest key. The anti-fire cape is inside the chest, which starts locked. |
| D | Dragon's Gate | Needs the sewer key, then the mithril axe, even if you carry a stronger weapon. |
| R | Drop Room | Gear you drop lands here. The torch falls at your feet instead. |
| B | Basilisk | A confirm. Saying yes is a faint, not a hit-point fight. |
| L | Lava Falls | A confirm. Jumping is a burn. The brazier beside it takes the bucket and gives a clue. |
| T | Bamboo raft | A confirm. Riding away is an escape: you live, the dragon does not die, and it is not a win. |

Losing the light starts a ten-second darkness. At the end of it the hidden beast finds you. The mine mouth and the barn door push you back out if the torch is unlit. The keep and the guild do not.

## The fight

Health exists only in the lair: you have 175, the dragon has 550. One roll from 0 to 20 decides the turn, as in the Java `Fight` class.

| Move | Base, before crits | Crit rolls | Heal rolls |
| --- | --- | --- | --- |
| Whirlwind | 8 × attack | 4, 5, 6, 9 (extra ×5) | 9, 10 (+60, capped at 175) |
| Doom Hammer | 10 × attack | 4, 5 (extra ×3) | 10 |
| Cobra Strike | 6 × attack | 4, 5, 6 (extra ×6) | 7, 8, 9, 10 |

The dragon always answers with fire breath. Rolls 18 and 20 add Bone Crush. Roll 19 adds Wing Smash. If both of you reach 0 on the same turn, the dragon is checked first, so it is a win.

At the original default stats (attack 3, defence 3, fire resistance 1) the hits match the 2014 numbers: whirlwind 24, doom hammer 30, cobra strike 18, fire breath 13, bone crush 45, wing smash 50.

## What changed from the Java original

- **Gear reaches the fight.** `Fight`, `BattleWarrior`, and `BattleDragon` each constructed a new `Rooms`, so `getAttack`, `getDefence`, and `getfireRes` were always 3, 3, and 1. Combat now uses the pack you are carrying. Stats stack for everything you hold. There is no separate equip step, which matches the original intent.
- **Fire resistance reduces fire breath.** The Java breath formula grew as defence and fire resistance grew, which fought the line "excellent against fire". Breath is now `13 × (1 − 0.075 × (fireRes − 1))`, clamped between 3 and 40. Cloth (fire resistance 0) makes breath slightly worse. The cape (10) cuts it to a little above 4. Bone crush still uses the original `80 / (defence + fire) + 25`. Wing smash softens as defence rises and stays 50 when defence is 3.
- **The torch requirement is spatial.** In the original, the mine and farm checks did not actually block entry, and dropping the torch killed you on the same line. Here the mine and the barn refuse entry without a lit torch. Dropping it starts the darkness timer after a short grace. Other rooms never required a torch, and they still do not.
- **Basilisk, lava, and the raft are three different places.** The original minimap letter H covered all three, and "ride away" printed a game-over that read like a death. Basilisk and lava ask, then kill in their own words. The raft asks, then ends as an escape.
- **The guild chest starts locked.** The original line said you had opened it earlier. The cape appears only after the chest key is used.
- **Heal text matches the number.** The Java message said +50 while the code added 60, with no cap. Healing is +60 and cannot pass 175. Crit multipliers are unchanged. The log prints the damage that was actually dealt.
- **Score.** Still starts at 10 and still adds 2 per second and 500 for the kill. Pickups add 25, the chest and the brazier add 40. Pause and menus freeze the clock.
- **`Statistics.plusAttack` returned defence.** That method was unused. It is not in this project.
- **Presentation.** Third-person exploration (first person on V), a main menu, pause, win, death, and escape screens, a minimap, and lighting that drops with the torch. Audio is procedural. Drop a clip on `Assets/Resources/Audio/` named `pickup`, `hit`, `roar`, `ui`, `win`, `lose`, `ambient`, `torch`, `swing`, `heal`, or `crit` and it replaces the synth of that name.
- **The opening island paragraph is new.** The 2014 game printed "Description credit to Zork." That paragraph is not reproduced here. The new text keeps the same landmarks: volcano, fire river, sulfurous lake, palace, scrolls, mine, farms, sewers, and the shore. Room lines, the basilisk, the falls, the raft, the gate, and the win and loss lines are the student-authored ones. "Dragon art by John Vander Zwaag" referred to ASCII lettering. This remake builds the dragon from primitives.

## First time you press Play

- The menu should name the game and offer Play, How to play, and Quit.
- The torch is on the ground in front of you. Without it the screen and the fog close in, and after about ten seconds the run ends.
- Cloth, the axe, the sword, the whip, the shield, and the cape should change the attack, defence, and fire-resistance numbers on the left, and those numbers should change the damage printed in the lair.
- The mine and the barn should shove you back out until the torch is lit.
- The basilisk, the falls, and the raft should each ask before they end the run, and the raft should use the escape screen.

This environment did not have the Unity editor, so the project has not been imported or entered Play Mode here. The fight math was checked outside Unity. What to watch on the first real open: package import, the URP asset appearing under `Assets/Settings/`, the Console after the first compile, and whether point lights read as torch-bright (the code sets Unity 6 light units to candela).
