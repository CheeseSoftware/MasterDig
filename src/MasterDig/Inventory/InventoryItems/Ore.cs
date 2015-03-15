using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory.InventoryItems
{
	public class Ore : InventoryItem, IShopItem
	{
		private int _XPGain;
		private int _buyPrice;
		private int _sellPrice;
		private int _hardness;
		private int _levelRequired;

		public Ore(string name, int XPGain, int buyPrice, int sellPrice, int hardness, int levelRequired)
			: base(name)
		{
			_XPGain = XPGain;
			_buyPrice = buyPrice;
			_sellPrice = sellPrice;
			_hardness = hardness;
			_levelRequired = levelRequired;
		}

		public int XPGain { get { return _XPGain; } }

		public int BuyPrice { get { return _buyPrice; } }

		public int SellPrice { get { return _sellPrice; } }

		public int Hardness { get { return _hardness; } }

		public int LevelRequired { get { return _levelRequired; } }
	}
}
