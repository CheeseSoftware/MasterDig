using MasterBot;
using MasterBot.Room.Block;
using MasterDig.Inventory;
using MasterDig.Inventory.InventoryItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig
{
	public static class Shop
	{
		private static int xPos = 0;
		private static int yPos = 0;
		public static IBot bot;
		//public static Dictionary<string, InventoryItem> shopInventory = new Dictionary<string, InventoryItem>(ItemManager.itemTranslator);

		static Shop()
		{
		}

		static public void BuyItem(DigPlayer player, string itemName, int amount = 1)
		{
			InventoryItem item = ItemManager.GetItemFromName(itemName);
			IShopItem shopItem = ItemManager.GetShopItemByName(itemName);
			if (item != null && shopItem != null)
			{
				int totalItemPrice = shopItem.BuyPrice * amount;
				if (player.digMoney >= totalItemPrice)
				{
					player.digMoney -= totalItemPrice;
					player.inventory.AddItem(new InventoryItem(item), amount);
					player.Player.Reply("You bought " + amount + " " + itemName + "!");
				}
				else
				{
					if (amount == 1)
						player.Player.Reply("You need " + totalItemPrice + " money to buy that item.");
					else
						player.Player.Reply("You need " + totalItemPrice + " money, " + shopItem.BuyPrice + " each, to buy those items.");
				}
			}
			else
			{
				if (itemName != "")
					player.Player.Reply("That item does not exist.");
				player.Player.Reply("You can buy these items:");
				List<InventoryItem> shopItems = ItemManager.GetBuyableItems();
				string temp = "";
				foreach (InventoryItem oitem in shopItems)
				{
					temp += oitem.Name + (shopItems.Last().Name.Equals(oitem.Name) ? "" : ", ");
				}
				player.Player.Send(temp);
			}
		}

		static public void SellItem(DigPlayer player, string itemName, int amount = 1)
		{
			InventoryItem item = ItemManager.GetItemFromName(itemName);
			if (item != null)
			{
				IShopItem shopItem = ItemManager.GetShopItemByName(itemName);
				if (shopItem != null)
				{
					int itemSellPrice = shopItem.SellPrice;
					if (player.inventory.Contains(item) && player.inventory.GetItemCount(item) >= amount)
					{
						if (player.inventory.RemoveItem(item, amount))
						{
							player.digMoney += itemSellPrice * amount;
							string prefix = (amount > 1 ? "Items" : "Item");
							player.Player.Reply(prefix + " sold! You received " + (itemSellPrice * amount) + " money.");
						}
						else
							player.Player.Reply("Internal error, try doing that again.");
					}
					else
						player.Player.Reply("You do not have enough of that item.");
				}
				else
					player.Player.Reply("You can't sell that item.");
			}
			else
			{
				if (itemName != "")
					player.Player.Reply("That item does not exist.");
			}
		}

		static public void SetLocation(int x, int y)
		{
			if (xPos != null && yPos != null)
				bot.Room.setBlock(xPos, yPos, new NormalBlock(0, 0));
			bot.Room.setBlock(x, y, new NormalBlock(Skylight.BlockIds.Blocks.Pirate.CHEST, 0));
			xPos = x;
			yPos = y;
		}

		static public bool IsPlayerClose(IPlayer player)
		{
			for (int i = 0; i < 9; i++)
			{
				int x = i / 3 - 1;
				int y = i % 3 - 1;
				if (bot.Room.getBlock(0, x + player.BlockX, y + player.BlockY).Id == Skylight.BlockIds.Blocks.Pirate.CHEST)
					return true;
			}
			return false;
		}

	}
}
