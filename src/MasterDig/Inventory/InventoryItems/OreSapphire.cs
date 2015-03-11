using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory.InventoryItems
{
    class OreSapphire : Ore
    {
        public OreSapphire()
            : base("sapphire")
        {
            data.Add("xpgain", 5);
            data.Add("buyprice", 5);
            data.Add("sellprice", 0);
            data.Add("dighardness", 36);
            data.Add("levelrequired", 25);
        }
    }
}
