using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory.InventoryItems
{
    class OreIron : Ore
    {
        public OreIron()
            : base("iron")
        {
            data.Add("xpgain", 6);
            data.Add("buyprice", 8);
            data.Add("sellprice", 3);
            data.Add("dighardness", 14);
            data.Add("levelrequired", 6);
        }
    }
}
