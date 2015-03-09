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
                IPlayer player = (IPlayer)cmdSource;
                if (!player.HasMetadata("digplayer"))
                    player.SetMetadata("digplayer", new DigPlayer(player));
                DigPlayer digPlayer = (DigPlayer)player.GetMetadata("digplayer");

                AddUnsavedPlayer(player);

                switch (cmd)
                {
                    case "dynamite":
                        {
                            ItemDynamite dynamite = new ItemDynamite();
                            if (digPlayer.inventory.RemoveItem(dynamite, 1) || player.IsGod)
                            {
                                bot.Say(player.Name + " has placed a big barrel of dynamite! Hide!!");
                                bot.Room.setBlock(player.BlockX, player.BlockY + 1, new NormalBlock(163, 0));
                                dynamites.Add(
                                    new Pair<BlockPos, ItemDynamite>(new BlockPos(0, player.BlockX, player.BlockY + 1), dynamite)
                                    );
                            }
                        }
                        break;
                    case "dig":
                    case "help":
                    case "commands":
                    case "digcommands":
                        player.Reply("Here are the commands:");
                        player.Reply("!xp, !level, !inventory, !xpleft");
                        player.Reply("!buy, !sell, !money, !levelforores, !save");
                        break;
                    case "levelforores":
                        player.Reply("stone: 0,   copper: 2,   iron: 6,");
                        player.Reply("gold: 10,   emerald: 15,   ruby: 20,");
                        player.Reply("ruby: 20,   sapphire: 25,   diamond: 30");
                        break;
                    case "generate":
                        if (player.IsOp)
                        {
                            bot.Say("Generating new map..");
                            digHardness = new float[bot.Room.Width, bot.Room.Height];
                            Generate(bot.Room.Width, bot.Room.Height);
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
                            }
                            else
                                player.Reply("That player doesn't exist.");
                        }
                        else
                            player.Reply("Usage: !givexp <player> <xp>");
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
                            bot.Room.setBlock(player.BlockX, player.BlockY, new NormalBlock(Skylight.BlockIds.Blocks.Pirate.CHEST, 0));
                        }
                        break;
                    case "money":
                        player.Reply("Money: " + digPlayer.digMoney);
                        break;
                    case "buy":
                        if (player.BlockX > Shop.xPos - 2 && player.BlockX < Shop.xPos + 2 && player.BlockY > Shop.yPos - 2 && player.BlockY < Shop.yPos + 2)
                        {
                            if (args.Length > 1)
                            {
                                if (DigBlockMap.itemTranslator.ContainsKey(args[0].ToLower()))
                                {
                                    InventoryItem item = DigBlockMap.itemTranslator[args[0].ToLower()];
                                    int itemPrice = Shop.GetBuyPrice(item);

                                    int amount = 1;
                                    if (args.Length >= 2)
                                        int.TryParse(args[1], out amount);
                                    if (digPlayer.digMoney >= (itemPrice * amount))
                                    {
                                        digPlayer.digMoney -= itemPrice;
                                        digPlayer.inventory.AddItem(new InventoryItem(item.GetData()), amount);
                                        player.Reply("Item bought!");
                                    }
                                    else
                                        player.Reply("You do not have enough money.");
                                }
                                else
                                    player.Reply("The requested item does not exist.");
                            }
                            else
                                player.Reply("Please specify what you want to buy.");
                        }
                        else
                            player.Reply("You aren't near the shop.");
                        break;
                    case "sell":
                        if (player.BlockX > Shop.xPos - 2 && player.BlockX < Shop.xPos + 2 && player.BlockY > Shop.yPos - 2 && player.BlockY < Shop.yPos + 2)
                        {
                            if (args.Length > 1)
                            {
                                string itemName = args[0].ToLower();
                                if (DigBlockMap.itemTranslator.ContainsKey(itemName))
                                {
                                    InventoryItem item = DigBlockMap.itemTranslator[itemName];
                                    int itemSellPrice = Shop.GetSellPrice(item);
                                    int amount = 1;
                                    if (args.Length >= 2)
                                        int.TryParse(args[1], out amount);
                                    if (digPlayer.inventory.Contains(item) != -1 && digPlayer.inventory.GetItemCount(item) >= amount)
                                    {
                                        digPlayer.digMoney += itemSellPrice * amount;
                                        if (!digPlayer.inventory.RemoveItem(item, amount))
                                            throw new Exception("Could not remove item?D:");
                                        player.Reply("Item sold! You received " + (itemSellPrice * amount) + " money.");
                                    }
                                    else
                                        player.Reply("You do not have enough of that item.");
                                }
                                else
                                    player.Reply("The item does not exist.");
                            }
                            else
                                player.Reply("Please specify what you want to sell.");
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
                                    blocksToRemove.Add(new Pair<BlockWithPos, double>(new BlockWithPos(xx, yy, new NormalBlock(414, 0)), distanceFromCenter));


                            }
                        }
                    }

                    blocksToRemove.Sort((s1, s2) => s1.second.CompareTo(s2.second));

                    foreach (var block in blocksToRemove)
                        DigBlock(block.first.X, block.first.Y, null, (int)Math.Floor(1/block.second*50)*((float)r.Next(100)/100+1), false, true);
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
