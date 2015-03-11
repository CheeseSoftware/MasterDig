using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory.InventoryItems
{
    class OreCopper : Ore
    {
        public OreCopper()
            : base("copper")
        {
            data.Add("xpgain", 5);
            data.Add("buyprice", 5);
            data.Add("sellprice", 2);
            data.Add("dighardness", 10);
            data.Add("levelrequired", 2);
        }
    }
}
