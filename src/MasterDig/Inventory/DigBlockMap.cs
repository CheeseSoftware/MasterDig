using MasterDig.Inventory.InventoryItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory
{
    public class DigBlockMap
    {
        public static Dictionary<int, InventoryItem> blockTranslator = new Dictionary<int, InventoryItem>();
        public static Dictionary<string, InventoryItem> itemTranslator = new Dictionary<string, InventoryItem>();
        static DigBlockMap()
        {
            blockTranslator.Add((int)Blocks.Stone, new OreStone());
            blockTranslator.Add((int)Blocks.Copper, new OreCopper());
            blockTranslator.Add((int)Blocks.Iron, new OreIron());
            blockTranslator.Add((int)Blocks.Gold, new OreGold());
            blockTranslator.Add((int)Blocks.Emerald, new OreEmerald());
            blockTranslator.Add((int)Blocks.Ruby, new OreRuby());
            blockTranslator.Add((int)Blocks.Sapphire, new OreSapphire());
            blockTranslator.Add((int)Blocks.Diamond, new OreDiamond());

            foreach (InventoryItem i in blockTranslator.Values)
                itemTranslator.Add(i.Name, i);

            itemTranslator.Add("dynamite", new ItemDynamite());
        }
    }
    public enum Blocks
    {
        Stone = Skylight.BlockIds.Blocks.Castle.BRICK,
        Iron = Skylight.BlockIds.Blocks.Metal.SILVER,
        Copper = Skylight.BlockIds.Blocks.Metal.BRONZE,
        Gold = Skylight.BlockIds.Blocks.Metal.GOLD,
        Diamond = Skylight.BlockIds.Blocks.Minerals.CYAN,
        Ruby = Skylight.BlockIds.Blocks.Minerals.RED,
        Sapphire = Skylight.BlockIds.Blocks.Minerals.BLUE,
        Emerald = Skylight.BlockIds.Blocks.Minerals.GREEN
    };
}
