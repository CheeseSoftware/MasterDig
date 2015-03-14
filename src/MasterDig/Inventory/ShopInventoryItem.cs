using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory
{
	class ShopInventoryItem : InventoryItem, IShopItem
	{
		private int _buyPrice;
		private int _sellPrice;

		public ShopInventoryItem(string name, int buyPrice, int sellPrice)
			: base(name)
		{
			_buyPrice = buyPrice;
			_sellPrice = sellPrice;
		}

		public int BuyPrice { get { return _buyPrice; } }

		public int SellPrice { get { return _sellPrice; } }
	}
}
