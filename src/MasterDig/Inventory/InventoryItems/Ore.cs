using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory.InventoryItems
{
    class Ore : InventoryItem
    {
        public Ore(string name)
            : base(name)
        {

        }

        public int XPGain { get { return (int)GetData("xpgain"); } }

        public int BuyPrice { get { return (int)GetData("buyprice"); } }

        public int SellPrice { get { return (int)GetData("sellprice"); } }

        public int Hardness { get { return (int)GetData("dighardness"); } }

        public int LevelRequired { get { return (int)GetData("levelrequired"); } }
    }
}
