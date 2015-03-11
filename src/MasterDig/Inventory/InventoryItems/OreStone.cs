using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory.InventoryItems
{
    class OreStone : Ore
    {
        public OreStone()
            : base("stone")
        {
            data.Add("xpgain", 1);
            data.Add("buyprice", 10);
            data.Add("sellprice", 1);
            data.Add("dighardness", 5);
            data.Add("levelrequired", 0);
        }
    }
}
