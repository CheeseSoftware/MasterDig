using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterBot;
using MasterBot.SubBot;
using MasterBot.Room;
using System.Threading;
using MasterBot.Room.Block;
using System.Windows.Forms;
using MasterDig.Inventory;
using MasterDig.Inventory.InventoryItems;

namespace MasterDig
{
	public partial class MasterDig : ASubBot, IPlugin
	{

		HashSet<IPlayer> playersToSave = new HashSet<IPlayer>();
		Queue<IPlayer> playersToSaveQueue = new Queue<IPlayer>();
		List<Pair<BlockPos, ItemDynamite>> dynamites = new List<Pair<BlockPos, ItemDynamite>>();

		public MasterDig()
			: base(null)
		{

		}

		public string PluginName
		{
			get { return "MasterDig"; }
		}

		public override bool HasTab
		{
			get { return true; }
		}

		public override string SubBotName
		{
			get { return "MasterDig"; }
		}

		public void PerformAction(IBot bot)
		{
			// This is required since I can't fix this in the constructor. :/
			this.bot = bot;

			// 0 is the Priority, it's the lowest possible.
			blockDrawer = bot.Room.BlockDrawerPool.CreateBlockDrawer(0);

			// Starts the BlockDrawer.
			blockDrawer.Start();

			EnableTick(1000);

			// Adds the SubBot to the SubBot handler so the SubBot will get callbacks.
			bot.SubBotHandler.AddSubBot(this, true);
		}

		public override void onEnable()
		{
			Shop.bot = bot;
		}

		public override void onDisable()
		{
		}

		public override void onConnect()
		{
		}

		public override void onDisconnect(string reason)
		{
		}

		public override void onMessage(PlayerIOClient.Message m)
		{
			switch (m.Type)
			{
				case "init":
				case "reset":
					digHardness = new float[bot.Room.Width, bot.Room.Height];
					resetDigHardness();
					break;
				case "m":
					{
						int userId = m.GetInt(0);
						float playerPosX = m.GetFloat(1);
						float playerPosY = m.GetFloat(2);
						float speedX = m.GetFloat(3);
						float speedY = m.GetFloat(4);
						float modifierX = m.GetFloat(5);
						float modifierY = m.GetFloat(6);
						float horizontal = m.GetFloat(7);
						float vertical = m.GetFloat(8);
						int Coins = m.GetInt(9);

						int blockX = (int)(playerPosX / 16 + 0.5);
						int blockY = (int)(playerPosY / 16 + 0.5);

						IPlayer player = bot.Room.getPlayer(userId);
						if (player == null || player.IsGod || player.IsMod)
							return;

						if (!player.HasMetadata("digplayer"))
							player.SetMetadata("digplayer", new DigPlayer(player));
						DigPlayer digPlayer = (DigPlayer)player.GetMetadata("digplayer");

						int digRange = digPlayer.digRange;
						int digStrength = digPlayer.digStrength;

						int blockId = (bot.Room.getBlock(0, blockX + (int)horizontal, blockY + (int)vertical).Id);
						if (isDigable(blockId))//(blockId >= Skylight.BlockIds.Blocks.Sand.BROWN - 5 && blockId <= Skylight.BlockIds.Blocks.Sand.BROWN)
						{

							if (digRange > 1)
							{
								for (int x = (horizontal == 1) ? -1 : -digRange + 1; x < ((horizontal == -1) ? 2 : digRange); x++)
								{
									for (int y = (vertical == 1) ? -1 : -digRange + 1; y < ((vertical == -1) ? 2 : digRange); y++)
									{
										float distance = (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
										if (distance <= 1.41421357 * (digRange - 1) || distance < 1.4142)
											DigBlock(blockX + x + (int)Math.Ceiling(horizontal), blockY + y + (int)Math.Ceiling(vertical), player, (digRange - distance) * digStrength, false);
									}
								}
								AddUnsavedPlayer(player);
								return;
							}
						}
						{
							if (horizontal == 0 || vertical == 0)
								DigBlock(blockX + (int)horizontal, blockY + (int)vertical, player, digStrength, true);

							blockId = bot.Room.getBlock(0, blockX, blockY).Id;
							DigBlock(blockX, blockY, player, digStrength, true);
							AddUnsavedPlayer(player);
						}
					}
					break;

				case "b":
					{
						int layer = m.GetInt(0);
						int blockId = m.GetInt(3);
						int x = m.GetInt(1);
						int y = m.GetInt(2);

						if (layer == 0)
							resetBlockHardness(x, y, blockId);
					}
					break;

			}
		}

		public override void onCommand(string cmd, string[] args, ICmdSource cmdSource)
		{
			if (cmdSource is IPlayer && (IPlayer)cmdSource != null)
			{
				IPlayer player = (IPlayer)cmdSource;
				DigPlayer digPlayer = DigPlayer.FromPlayer(player);

				AddUnsavedPlayer(player);

				switch (cmd)
				{
					case "dynamite":
						{
							ItemDynamite dynamite = new ItemDynamite();
							dynamite.Placer = digPlayer;
							if (digPlayer.inventory.RemoveItem(dynamite, 1) || player.IsGod)
							{
								bot.Say(player.Name + " has placed a big barrel of dynamite! Hide!!");
								bot.Room.setBlock(player.BlockX, player.BlockY, new NormalBlock(163, 0));
								dynamites.Add(
									new Pair<BlockPos, ItemDynamite>(new BlockPos(0, player.BlockX, player.BlockY), dynamite)
									);
							}
							else
								player.Reply("You have no dynamite! Buy it at the shop.");
						}
						break;
					case "dig":
					case "help":
					case "commands":
					case "digcommands":
						player.Reply("Here are the commands: !xp, !level, !inventory, !xpleft, !buy, !sell, !money, !levelforores, !save");
						break;
					case "levelforores":
						{
							string total = "";
							foreach (Ore ore in ItemManager.GetOres())
							{
								total += ore.Name + ": " + ore.LevelRequired + (ItemManager.GetOres().Last().Name.Equals(ore.Name) ? "" : ", ");
							}
							player.Reply(total);
						}
						break;
					case "generate":
						if (player.IsOp)
						{
							int seed = -1;

							if (args.Length >= 1)
							{
								Int32.TryParse(args[0], out seed);
							}
							if (seed == -1)
								seed = random.Next();

							bot.Say("Generating new map with seed " + seed + ".");
							digHardness = new float[bot.Room.Width, bot.Room.Height];
							Generate(bot.Room.Width, bot.Room.Height, seed);
						}
						break;
					case "givexp":
						if (player.IsOp && args.Length >= 2)
						{
							IPlayer receiver = bot.Room.getPlayer(args[0]);
							if (receiver != null)
							{
								int xp = Int32.Parse(args[1]);
								if (!receiver.HasMetadata("digplayer"))
									receiver.SetMetadata("digplayer", new DigPlayer(receiver));
								DigPlayer receiverDigPlayer = (DigPlayer)receiver.GetMetadata("digplayer");
								receiverDigPlayer.digXp += xp;
								AddUnsavedPlayer(receiver);
							}
							else
								player.Reply("That player doesn't exist.");
						}
						else
							player.Reply("Usage: !givexp <player> <xp>");
						break;
					case "setxp":
						if (player.IsOp && args.Length >= 2)
						{
							IPlayer receiver = bot.Room.getPlayer(args[0]);
							if (receiver != null)
							{
								int xp = Int32.Parse(args[1]);
								if (!receiver.HasMetadata("digplayer"))
									receiver.SetMetadata("digplayer", new DigPlayer(receiver));
								DigPlayer receiverDigPlayer = (DigPlayer)receiver.GetMetadata("digplayer");
								receiverDigPlayer.digXp = xp;
								AddUnsavedPlayer(receiver);
							}
							else
								player.Reply("That player doesn't exist.");
						}
						else
							player.Reply("Usage: !setxp <player> <xp>");
						break;
					case "givemoney":
						if (player.IsOp && args.Length >= 2)
						{
							IPlayer receiver = bot.Room.getPlayer(args[0]);
							if (receiver != null)
							{
								int money = Int32.Parse(args[1]);
								if (!receiver.HasMetadata("digplayer"))
									receiver.SetMetadata("digplayer", new DigPlayer(receiver));
								DigPlayer receiverDigPlayer = (DigPlayer)receiver.GetMetadata("digplayer");
								receiverDigPlayer.digMoney += money;
								AddUnsavedPlayer(receiver);
							}
							else
								player.Reply("That player doesn't exist.");
						}
						else
							player.Reply("Usage: !givemoney <player> <money>");
						break;
					case "setmoney":
						if (player.IsOp && args.Length >= 2)
						{
							IPlayer receiver = bot.Room.getPlayer(args[0]);
							if (receiver != null)
							{
								int money = Int32.Parse(args[1]);
								if (!receiver.HasMetadata("digplayer"))
									receiver.SetMetadata("digplayer", new DigPlayer(receiver));
								DigPlayer receiverDigPlayer = (DigPlayer)receiver.GetMetadata("digplayer");
								receiverDigPlayer.digMoney = money;
								AddUnsavedPlayer(receiver);
							}
							else
								player.Reply("That player doesn't exist.");
						}
						else
							player.Reply("Usage: !setmoney <player> <money>");
						break;
					case "xp":
						player.Reply("XP: " + digPlayer.digXp);
						break;
					case "xpleft":
						player.Reply("You need " + (digPlayer.xpRequired_ - digPlayer.digXp).ToString() + " XP for level " + (digPlayer.digLevel + 1).ToString());
						break;
					case "level":
						player.Reply("Level: " + digPlayer.digLevel);
						break;
					case "inventory":
						player.Reply(digPlayer.inventory.ToString());
						break;
					case "save":
						player.Reply("Saved!");
						digPlayer.Save();
						break;
					case "setshop":
						if (player.IsOp)
						{
							Shop.SetLocation(player.BlockX, player.BlockY);
							player.Reply("Shop set at X:" + player.BlockX + " Y:" + player.BlockY);
						}
						break;
					case "money":
						player.Reply("Money: " + digPlayer.digMoney);
						break;
					case "buy":
						if (Shop.IsPlayerClose(player))
						{
							if (args.Length >= 1)
							{
								string itemName = args[0].ToLower();
								int amount = 1;

								if (args.Length >= 2)
								{
									if (!int.TryParse(args[1], out amount))
										amount = 1;
								}

								if (amount < 1)
								{
									player.Reply("You can't buy a negative amount of items.");
									break;
								}

								Shop.BuyItem(digPlayer, itemName, amount);
							}
							else
							{
								player.Reply("Usage: !buy <item> [amount=1]");
								Shop.BuyItem(digPlayer, "", 0);
							}
						}
						else
							player.Reply("You aren't near the shop.");
						break;
					case "sell":
						if (Shop.IsPlayerClose(player))
						{
							if (args.Length >= 1)
							{
								string itemName = args[0].ToLower();
								int amount = 1;

								if (args.Length >= 2)
								{
									if (!int.TryParse(args[1], out amount))
										amount = 1;
								}

								if (amount < 1)
								{
									player.Reply("You can't buy a negative amount of items.");
									break;
								}

								Shop.SellItem(digPlayer, itemName, amount);
							}
							else
							{
								player.Reply("Usage: !sell <item> [amount=1]");
								Shop.BuyItem(digPlayer, "", 0);
							}
						}
						else
							player.Reply("You aren't near the shop.");
						break;
				}
			}
		}

		public override void onBlockChange(int x, int y, IBlock newBlock, IBlock oldBlock)
		{

		}

		public override void onTick()
		{
			if (!bot.Room.HasCode)
				return;

			DigPlayer digPlayer = null;
			lock (playersToSave)
			{

				if (playersToSaveQueue.Count > 0)
				{
					IPlayer player = playersToSaveQueue.Dequeue();
					playersToSave.Remove(player);

					digPlayer = (DigPlayer)player.GetMetadata("digplayer");

				}
			}
			if (digPlayer != null)
			{
				digPlayer.Save();
			}

			lock (dugBlocksToPlaceQueueLock)
			{
				while (dugBlocksToPlaceQueue.Count > bot.Room.Width * bot.Room.Height / 20)
				{
					BlockWithPos block = dugBlocksToPlaceQueue.Dequeue();
					if (digHardness[block.X, block.Y] == 0f)
						bot.Room.setBlock(block.X, block.Y, block.Block);
				}
			}

			Pair<BlockPos, ItemDynamite> toRemove = null;
			List<Pair<BlockPos, ItemDynamite>>.Enumerator e = dynamites.GetEnumerator();
			while (e.MoveNext())
			{
				if ((DateTime.Now - e.Current.second.DatePlaced).Seconds >= 5)
				{
					Random r = new Random();
					int x = e.Current.first.X;
					int y = e.Current.first.Y;
					float strength = e.Current.second.Strength;
					List<Pair<BlockWithPos, double>> blocksToRemove = new List<Pair<BlockWithPos, double>>();
					for (int xx = (int)(x - strength); xx < x + strength; xx++)
					{
						for (int yy = (int)(y - strength); yy < y + strength; yy++)
						{
							double distanceFromCenter = Math.Sqrt(Math.Pow(x - xx, 2) + Math.Pow(y - yy, 2));
							if (distanceFromCenter <= strength)
							{
								bool shouldRemove = (r.Next((int)((distanceFromCenter / strength) * 100)) <= 50 ? true : false);
								if (shouldRemove)
								{
									blocksToRemove.Add(new Pair<BlockWithPos, double>(new BlockWithPos(xx, yy, new NormalBlock(414, 0)), distanceFromCenter));

									if (e.Current.second.Placer != null)
									{
										int blockIdReplaced = bot.Room.getBlock(0, xx, yy).Id;
										InventoryItem oreItem = ItemManager.GetItemFromOreId(blockIdReplaced);
										if (oreItem != null && r.Next(4) == 0)
										{
											e.Current.second.Placer.inventory.AddItem(oreItem, 1);
										}
									}
								}


							}
						}
					}

					blocksToRemove.Sort((s1, s2) => s1.second.CompareTo(s2.second));
					bot.Room.setBlock(e.Current.first.X, e.Current.first.Y, new NormalBlock(414, 0));
					foreach (var block in blocksToRemove)
						DigBlock(block.first.X, block.first.Y, null, (int)Math.Floor(1 / block.second * 50) * ((float)r.Next(100) / 100 + 1), false, true);
					blocksToRemove.Clear();


					toRemove = e.Current;
					break;
				}
			}
			if (toRemove != null)
				dynamites.Remove(toRemove);
		}

		public void AddUnsavedPlayer(IPlayer player)
		{
			lock (playersToSave)
			{
				if (!playersToSave.Contains(player))
				{
					playersToSave.Add(player);
					playersToSaveQueue.Enqueue(player);
				}
			}
		}

	}
}
