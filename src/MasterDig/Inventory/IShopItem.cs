using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory
{
    public interface IShopItem
    {
        int BuyPrice { get; }
        int SellPrice { get; }
    }
}
