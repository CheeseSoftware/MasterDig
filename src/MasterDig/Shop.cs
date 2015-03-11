using MasterDig.Inventory;
using MasterDig.Inventory.InventoryItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig
{
    public static class Shop
    {
        public static int xPos = 0;
        public static int yPos = 0;

        public static Dictionary<string, InventoryItem> shopInventory = new Dictionary<string, InventoryItem>(DigBlockMap.itemTranslator);
        static Shop()
        {
            
        }

        static public int GetBuyPrice(InventoryItem item)
        {
            return (int)item.GetData("buyprice");
        }

        static public int GetSellPrice(InventoryItem item)
        {
            return (int)item.GetData("sellprice");
        }

        /*static public InventoryItem Buy(string itemName, int amount)
        {
            InventoryItem temp = shopInventory[itemName];
            temp.SetAmount(amount);
            return temp;
        }

        static public int Sell(string itemName, int amount)
        {
            return ((int)shopInventory[itemName].GetDataAt(2)) * amount;
        }*/

        static public void SetLocation(int x, int y)
        {
            xPos = x;
            yPos = y;
        }

    }
}
