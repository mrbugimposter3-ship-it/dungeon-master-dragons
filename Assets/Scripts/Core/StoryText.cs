namespace DungeonMaster
{
    public static class StoryText
    {
        public const string Title = "DUNGEON MASTER";
        public const string Subtitle = "The Great Dragon Lair";

        // Original island paragraph was credited to Zork. This is new prose that keeps
        // the same landmarks: volcano, fire river, sulfurous lake, palace, scrolls, mine, farms, sewers.
        public const string IslandStory =
            "You stand among blackened trunks on a volcanic island. An active volcano owns the horizon. " +
            "Molten rock surges from its crown and runs back into the dark, and the glare on the stone is blood-red. " +
            "Ash ticks against your sleeves. The air smells of brimstone, and the ground is warm underfoot.\n\n" +
            "To the east, a river of fire leaves the high ground as three great falls. " +
            "To the west, steam boils off a sulfurous lake where something huge shifts inside a cage. " +
            "Southwest is an old palace of pale stone. Southeast, a monument hung with scrolls. " +
            "Northwest, the mouth of a mine. Northeast, fields that stay dark even under the volcano.\n\n" +
            "The beast you came for is not in the open. It lurks in the sewers under the Dragon's Gate, " +
            "north, at the foot of the volcano. Find armour and weapons. The gate opens for the sewer key and the mithril axe.\n\n" +
            "South of the island, a bamboo raft knocks against the shore. It will take you away. " +
            "Leaving is not the same as killing the dragon.";

        public const string PitchBlack =
            "It is pitch black. You are likely to be eaten by the hidden basilisk.\nLight the torch.";

        public const string HowToPlay =
            "Move with W A S D. The mouse looks. Shift sprints. Space hops a short step. V swaps third person and first person.\n\n" +
            "E uses whatever is in front of you: light the torch, take an item, open the chest, speak to a hazard.\n" +
            "The basilisk, the lava falls, and the bamboo raft each ask before they end the journey. They are not the dragon.\n\n" +
            "I opens the pack. Select an item to read it, drop it, or eat the cheese. " +
            "Dropped gear waits in the Drop Room, southwest. The torch falls at your feet instead. " +
            "If it stays dark, the beast in the dark finds you.\n\n" +
            "The panel on the left shows attack, defence, and fire resistance. Those numbers are what the dragon fight uses.\n" +
            "J reads the island story. M expands the minimap. Esc pauses.\n\n" +
            "The mine and the farm barn are dark past their doors. A lit torch is required to step through.\n" +
            "The Dragon's Gate wants the sewer key and the mithril axe, even if you carry a stronger weapon.\n\n" +
            "In the lair the fight is turn-based. 1 Whirlwind, 2 Doom Hammer, 3 Cobra Strike. " +
            "The dragon answers every swing with fire, and sometimes with Bone Crush or Wing Smash.\n\n" +
            "Minimap letters: M mine, F farms, A keep, G guild, D dragon, R drops, B basilisk, L lava, T raft. North is up.";

        public const string BasiliskBody =
            "Three rows of teeth, each as sharp as a dagger, a nose the size of a baked potato, dripping " +
            "with yellow-streaked snot, feet like sprawling roots lashing as the monster advances, " +
            "a roar like a thousand nightmarish ghouls. It grins.\n\n" +
            "This is not a battle you can win with a sword. Kill?";

        public const string LavaBody =
            "The Lava Falls. Largest fall of molten lava. Comprised of three great falls, the huge molten " +
            "rock at the base invites you to jump.\n\n" +
            "Jump?";

        public const string RaftBody =
            "You see a nice looking bamboo shaft which you can ride away from the island of doom. " +
            "You managed to find the shore.\n\n" +
            "Ride away? The dragon will still be here.";

        public const string GateBody =
            "The Dragon's Gate opens with a silent creak. Boiling steam screams its way through the " +
            "cracked walls. You see a shadow move in the far end of the dungeon. A hidden beast " +
            "lurking in the filth of the sewers. Your torch falls from your hand from the sweat of " +
            "your fear. All is quiet. Too quiet. It is pitch black, the darkness rages in.\n\n" +
            "THEN SUDDENLY, the Demon Dragon blows with full magnitude a breath of molten fire. The " +
            "speed and power of the Dragon's breath lights up the cave-like sewers, and you " +
            "manage to see his thick, brutal, deadly, scaly skin. You are left with no choice but " +
            "to fight him, and eliminate his hidden presence.";

        public const string DragonLore =
            "The Dragon is a sinister, lurking presence in the dark places of the earth. Its favorite " +
            "diet is adventurers, but its insatiable appetite is tempered by its fear of light. No " +
            "dragon has ever been seen by the light of day, and few have survived its fearsome jaws " +
            "to tell the tale. The beast lurks in the upper regions of the land, under the gate.";

        public const string WinBody =
            "You killed the hidden beast, the demon, the Dragon!\n\n" +
            "You have attained the highest honor and title for this game.";

        public const string DragonLossBody =
            "Like the rest of those who tried to kill the hidden Dragon, death overtakes you.";

        public const string DarknessBody =
            "You can no longer see. The darkness rages in.\n" +
            "The hidden beast, no longer scared by your light, cuts your jugular vein.";

        public const string CheeseBody =
            "You eat the cheese, and the poison kills you.";

        public const string BasiliskDeath =
            "You faint just by the presence of the hidden basilisk.";

        public const string LavaDeath =
            "Your body burns by the heat of the lava.";

        public const string RaftEscape =
            "You ride away peacefully while the volcano erupts, deafening your ears.\n\n" +
            "You are alive. The dragon is not dead. The title was not earned.";

        public const string BrazierClue =
            "The bucket of water smothers a spout of flame. In the steam, scratches in the rock read: " +
            "the gate yields only to the sewer key and the mithril axe.";

        public const string Credit =
            "A 3D remake of the 2014 text adventure by bonako. Gear now reaches the fight.";
    }
}
