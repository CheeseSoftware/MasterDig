using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory.InventoryItems
{
    class ItemDynamite : InventoryItem
    {
        public ItemDynamite()
            : base(new object[] { "Dynamite", 0, 20, 15, (float)5, DateTime.Now})
        {
            
        }

        public float Strength { get { return (float)GetDataAt(4); } }

        public DateTime DatePlaced { get { return (DateTime)GetDataAt(5); } }
    }
}
