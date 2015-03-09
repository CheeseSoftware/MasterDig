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
            : base(new object[] { "dynamite", 0, 60, 30, (float)15, DateTime.Now})
        {
            
        }

        public float Strength { get { return (float)GetDataAt(4); } }

        public DateTime DatePlaced { get { return (DateTime)GetDataAt(5); } }
    }
}
