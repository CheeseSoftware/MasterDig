using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory.InventoryItems
{
    class ItemDynamite : InventoryItem, IShopItem
    {
        private DigPlayer placer;

        public ItemDynamite()
            : base("dynamite")
        {
            data.Add("explosionstrength", 15);
            data.Add("dateplaced", DateTime.Now);
        }

        public float Strength { get { return (int)GetData("explosionstrength"); } }

        public DateTime DatePlaced { get { return (DateTime)GetData("dateplaced"); } }

        public int BuyPrice { get { return 60; } }

        public int SellPrice { get { return 30; } }

        public DigPlayer Placer { get { return placer; } set { placer = value; } }
    }
}
