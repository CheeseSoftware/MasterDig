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

namespace MasterDig
{
    public partial class MasterDig : ASubBot, IPlugin
    {

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

            // Adds the SubBot to the SubBot handler so the SubBot will get callbacks.
            bot.SubBotHandler.AddSubBot(this, true);
        }

        public override void onEnable()
        {
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
                    //Generate(m.GetInt(10), m.GetInt(11));
                    digHardness = new float[bot.Room.Width, bot.Room.Height];

                    resetDigHardness();
                    break;

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
                        // bool purple = (bot.isBB) ? false : m.GetBoolean(10);
                        //bool hasLevitation = (bot.isBB) ? false : m.GetBoolean(11);

                        int blockX = (int)(playerPosX / 16 + 0.5);
                        int blockY = (int)(playerPosY / 16 + 0.5);

                        IPlayer player = bot.Room.getPlayer(userId);
                        if (player == null)
                            return;

                        int digRange = 2;
                        int digStrength = 2;

                        int blockId = (bot.Room.getBlock(0, blockX + (int)horizontal, blockY + (int)vertical).Id);
                        if (isDigable(blockId))//(blockId >= Skylight.BlockIds.Blocks.Sand.BROWN - 5 && blockId <= Skylight.BlockIds.Blocks.Sand.BROWN)
                        {

                            if (digRange > 1)
                            {
                                for (int x = (horizontal == 1) ? -1 : -digRange + 1; x < ((horizontal == -1) ? 2 : digRange); x++)
                                {
                                    for (int y = (vertical == 1) ? -1 : -digRange + 1; y < ((vertical == -1) ? 2 : digRange); y++)
                                    {
                                        Console.WriteLine("snor är :" + x.ToString() + "    och skit är: " + y.ToString());


                                        if (true)//(blockId >= Skylight.BlockIds.Blocks.Sand.BROWN - 5 && blockId <= Skylight.BlockIds.Blocks.Sand.BROWN)
                                        {
                                            float distance = (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
                                            //float distanceB = (float)Math.Sqrt(Math.Pow(x - horizontal, 2) + Math.Pow(y - vertical, 2))*1.5F;

                                            //float distance = (distanceA < distanceB)? distanceA:distanceB;

                                            //if (distance == 0)
                                            //    DigBlock(blockX + x + (int)Math.Ceiling(horizontal), blockY + y + (int)Math.Ceiling(vertical), player, player.digStrength, false);
                                            if (distance <= 1.41421357 * (digRange - 1) || distance < 1.4142)
                                                DigBlock(blockX + x + (int)Math.Ceiling(horizontal), blockY + y + (int)Math.Ceiling(vertical), player, (digRange - distance) * digStrength, false);
                                        }
                                    }
                                }
                                return;
                            }
                        }
                        {
                            if (horizontal == 0 || vertical == 0)
                                DigBlock(blockX + (int)horizontal, blockY + (int)vertical, player, digStrength, true);

                            blockId = bot.Room.getBlock(0, blockX, blockY).Id;
                            DigBlock(blockX, blockY, player, digStrength, true);

                        }
                    }
                    break;

                case "b":
                    {
                        int blockId = m.GetInt(3);
                        int x = m.GetInt(1);
                        int y = m.GetInt(2);

                        resetBlockHardness(x, y, blockId);
                    }
                    break;

            }
        }

        public override void onCommand(string cmd, string[] args, ICmdSource cmdSource)
        {
            if (cmdSource is IPlayer && (IPlayer)cmdSource != null)
            {
                IPlayer normalPlayer = (IPlayer)cmdSource;
                DigPlayer player;
                if (digPlayers.TryGetValue(normalPlayer, out player) && player != null)
                {

                    switch (args[0])
                    {
                        case "digcommands":
                            cmdSource.Reply("commands: !xp, !level, !inventory, !xpleft, !buy <item> <amount>, !sell <item> <amount>, ");
                            break;
                        case "generate":
                            if (normalPlayer.IsOp)
                            {
                                digHardness = new float[bot.Room.Width, bot.Room.Height];
                                Generate(bot.Room.Width, bot.Room.Height);
                            }
                            break;
                        /*case "givexp":
                            if (normalPlayer.IsOp && args.Length >= 3)
                            {
                                DigPlayer receiver;
                                lock (bot.playerList)
                                {
                                    if (bot.nameList.ContainsKey(args[1]))
                                    {
                                        receiver = bot.playerList[bot.nameList[args[1]]];
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                int xp = Int32.Parse(args[2]);

                                receiver.digXp += xp;

                            }
                            break;

                        //case "cheat":

                        case "xp":
                            lock (bot.playerList)
                                bot.connection.Send("say", name + ": Your xp: " + bot.playerList[userId].digXp);
                            break;
                        case "xpleft":
                            lock (bot.playerList)
                                bot.connection.Send("say", name + ": You need" + (bot.playerList[userId].xpRequired_ - bot.playerList[userId].digXp).ToString() + " for level " + bot.playerList[userId].digLevel.ToString());
                            break;
                        case "level":
                            lock (bot.playerList)
                                bot.connection.Send("say", name + ": Level: " + bot.playerList[userId].digLevel);
                            break;
                        case "inventory":
                            {
                                lock (bot.playerList)
                                    bot.connection.Send("say", name + ": " + bot.playerList[userId].inventory.ToString());
                            }
                            break;
                        case "save":
                            {
                                lock (bot.playerList)
                                    bot.playerList[userId].Save();
                            }
                            break;

                        case "setshop":
                            {
                                lock (bot.playerList)
                                {
                                    int x = bot.playerList[userId].blockX;
                                    int y = bot.playerList[userId].blockY;
                                    Shop.SetLocation(x, y);
                                    bot.connection.Send("say", "Shop set at " + x + " " + y);
                                    bot.Room.DrawBlock(Block.CreateBlock(0, x, y, 9, 0));
                                }
                            }
                            break;
                        case "money":
                            {
                                lock (bot.playerList)
                                    bot.connection.Send("say", name + ": Money: " + bot.playerList[userId].digMoney);
                            }
                            break;
                        case "setmoney":
                            {
                            }
                            break;
                        case "buy":
                            {
                                lock (bot.playerList)
                                {
                                    BotPlayer p = bot.playerList[userId];
                                    if (p.blockX > Shop.xPos - 2 && p.blockX < Shop.xPos + 2)
                                    {
                                        if (p.blockY > Shop.yPos - 2 && p.blockY < Shop.yPos + 2)
                                        {
                                            if (args.Length > 1)
                                            {
                                                if (DigBlockMap.itemTranslator.ContainsKey(args[1].ToLower()))
                                                {
                                                    InventoryItem item = DigBlockMap.itemTranslator[args[1].ToLower()];
                                                    int itemPrice = Shop.GetBuyPrice(item);

                                                    int amount = 1;
                                                    if (args.Length >= 3)
                                                        int.TryParse(args[2], out amount);
                                                    if (p.digMoney >= (itemPrice * amount))
                                                    {
                                                        p.digMoney -= itemPrice;
                                                        p.inventory.AddItem(new InventoryItem(item.GetData()), amount);
                                                        bot.connection.Send("say", "Item bought!");
                                                    }
                                                    else
                                                        bot.connection.Send("say", name + ": You do not have enough money.");
                                                }
                                                else
                                                    bot.connection.Send("say", name + ": The requested item does not exist.");
                                            }
                                            else
                                                bot.connection.Send("say", name + ": Please specify what you want to buy.");
                                        }
                                    }
                                    bot.connection.Send("say", name + ": You aren't near the shop.");
                                }
                            }
                            break;
                        case "sell":
                            {
                                lock (bot.playerList)
                                {
                                    BotPlayer p = bot.playerList[userId];
                                    if (p.blockX > Shop.xPos - 2 && p.blockX < Shop.xPos + 2)
                                    {
                                        if (p.blockY > Shop.yPos - 2 && p.blockY < Shop.yPos + 2)
                                        {
                                            if (args.Length > 1)
                                            {
                                                string itemName = args[1].ToLower();
                                                if (DigBlockMap.itemTranslator.ContainsKey(itemName))
                                                {
                                                    InventoryItem item = DigBlockMap.itemTranslator[itemName];
                                                    int itemSellPrice = Shop.GetSellPrice(item);
                                                    int amount = 1;
                                                    if (args.Length >= 3)
                                                        int.TryParse(args[2], out amount);
                                                    if (p.inventory.Contains(item) != -1 && p.inventory.GetItemCount(item) >= amount)
                                                    {
                                                        p.digMoney += itemSellPrice * amount;
                                                        if (!p.inventory.RemoveItem(item, amount))
                                                            throw new Exception("Could not remove item?D:");
                                                        bot.connection.Send("say", name + ": Item sold! You received " + (itemSellPrice * amount) + " money.");
                                                    }
                                                    else
                                                        bot.connection.Send("say", name + ": You do not have enough of that item.");
                                                }
                                                else
                                                    bot.connection.Send("say", name + ": The item does not exist.");
                                            }
                                            else
                                                bot.connection.Send("say", name + ": Please specify what you want to sell.");
                                        }
                                    }
                                    bot.connection.Send("say", name + ": You aren't near the shop.");
                                }
                            }
                            break;

                        case "!placestones":
                            {

                            }
                            break;*/

                    }
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

            lock (dugBlocksToPlaceQueueLock)
            {
                while (dugBlocksToPlaceQueue.Count > bot.Room.Width * bot.Room.Height / 20)
                {
                    BlockWithPos block = dugBlocksToPlaceQueue.Dequeue();
                    bot.Room.setBlock(block.X, block.Y, block.Block);
                }
            }
        }

    }
}
