using MasterDig.Inventory.InventoryItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory
{
    public class ItemManager
    {
        private static Dictionary<int, Ore> oreTranslator = new Dictionary<int, Ore>();
        private static Dictionary<string, Ore> oreTranslatorByName = new Dictionary<string, Ore>();
        private static Dictionary<string, InventoryItem> itemTranslator = new Dictionary<string, InventoryItem>();

        static ItemManager()
        {
            AddOre((int)Blocks.Stone, new Ore("stone", 5, 10, 1, 5, 0));
            AddOre((int)Blocks.Copper, new Ore("copper", 25, 5, 2, 10, 4));
            AddOre((int)Blocks.Iron, new Ore("iron", 30, 8, 3, 14, 12));
            AddOre((int)Blocks.Gold, new Ore("gold", 75, 15, 14, 18, 20));
            AddOre((int)Blocks.Emerald, new Ore("emerald", 25, 5, 0, 24, 30));
            AddOre((int)Blocks.Ruby, new Ore("ruby", 25, 5, 0, 30, 40));
            AddOre((int)Blocks.Sapphire, new Ore("sapphire", 25, 5, 0, 36, 50));
            AddOre((int)Blocks.Diamond, new Ore("diamond", 25, 5, 0, 56, 60));

            itemTranslator.Add("dynamite", new ItemDynamite());
        }

        public static InventoryItem GetItemFromOreId(int blockId)
        {
            if (oreTranslator.ContainsKey(blockId))
                return oreTranslator[blockId].GetItem();
            return null;
        }

        public static InventoryItem GetItemFromName(string name)
        {
            if (itemTranslator.ContainsKey(name))
                return itemTranslator[name];
            return null;
        }

        public static Ore GetOreByName(string name)
        {
            if (oreTranslatorByName.ContainsKey(name))
                return oreTranslatorByName[name];
            return null;
        }

        public static IShopItem GetShopItemByName(string name)
        {
            if (oreTranslatorByName.ContainsKey(name))
                return oreTranslatorByName[name];
            else if(itemTranslator.ContainsKey(name))
            {
                InventoryItem item = itemTranslator[name];
                if (item is IShopItem)
                    return (IShopItem)item;
            }
            return null;
        }

        private static void AddOre(int block, Ore ore)
        {
            oreTranslator.Add(block, ore);
            itemTranslator.Add(ore.Name, ore.GetItem());
            oreTranslatorByName.Add(ore.Name, ore);
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
