using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory.InventoryItems
{
    class OreRuby : Ore
    {
        public OreRuby()
            : base("ruby")
        {
            data.Add("xpgain", 5);
            data.Add("buyprice", 5);
            data.Add("sellprice", 0);
            data.Add("dighardness", 30);
            data.Add("levelrequired", 20);
        }
    }
}
