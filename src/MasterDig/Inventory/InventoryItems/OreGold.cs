using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory.InventoryItems
{
    class OreGold : Ore
    {
        public OreGold()
            : base("gold")
        {
            data.Add("xpgain", 15);
            data.Add("buyprice", 15);
            data.Add("sellprice", 14);
            data.Add("dighardness", 18);
            data.Add("levelrequired", 10);
        }
    }
}
