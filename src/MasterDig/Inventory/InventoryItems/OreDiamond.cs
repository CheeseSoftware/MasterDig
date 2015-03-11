using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory.InventoryItems
{
    class OreDiamond : Ore
    {
        public OreDiamond()
            : base("diamond")
        {
            data.Add("xpgain", 5);
            data.Add("buyprice", 5);
            data.Add("sellprice", 0);
            data.Add("dighardness", 56);
            data.Add("levelrequired", 30);
        }
    }
}
