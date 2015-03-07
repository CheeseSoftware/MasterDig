using MasterBot;
using MasterBot.Room;
using MasterBot.Room.Block;
using MasterBot.SubBot;
using MasterDig.Inventory;
using Skylight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig
{
    public partial class MasterDig : ASubBot, IPlugin
    {
        protected Queue<BlockWithPos> dugBlocksToPlaceQueue = new Queue<BlockWithPos>();
        protected object dugBlocksToPlaceQueueLock = 0;
        protected float[,] digHardness;
        private IBlockDrawer blockDrawer;

        private bool isDigable(int blockId)
        {
            if (blockId >= Skylight.BlockIds.Blocks.Sand.BROWN - 5 && blockId <= Skylight.BlockIds.Blocks.Sand.BROWN)
                return true;
            else if (blockId >= 16 && blockId <= 21)
                return true;
            else if (blockId == Skylight.BlockIds.Blocks.JungleRuins.BLUE)
                return true;
            else
                return false;
        }

        private void DigBlock(int x, int y, IPlayer player, float digStrength, bool mining)
        {
            if (digHardness == null)
                resetDigHardness();

            if (!(x > 0 && y > 0 && x < bot.Room.Width && y < bot.Room.Height))
                return;

            if (digHardness[x, y] <= 0)
                return;

            IBlock block = bot.Room.getBlock(0, x, y);
            int blockId = -1;

            if (mining)
            {
                if (DigBlockMap.blockTranslator.ContainsKey(block.Id))
                {
                    blockId = 414;
                    InventoryItem temp = DigBlockMap.blockTranslator[block.Id];

                    if(!player.HasMetadata("digplayer"))
                        player.SetMetadata("digplayer", new DigPlayer(player));
                    DigPlayer digPlayer = (DigPlayer)player.GetMetadata("digplayer");

                    if (digPlayer.digLevel >= Convert.ToInt32(temp.GetDataAt(5)))
                     {
                         //Shop.shopInventory[DigBlockMap.blockTranslator[block.blockId]].GetDataAt(3)//för hårdhet
                         if (digHardness[x, y] <= digStrength)
                         {
                             InventoryItem newsak = new InventoryItem(temp.GetData());
                             digPlayer.inventory.AddItem(newsak, 1);
                             digPlayer.digXp += Convert.ToInt32(temp.GetDataAt(1));
                         }
                     }
                     else
                     {
                         return;
                     }

                }
            }

            switch (block.Id)
            {
                case BlockIds.Blocks.Sand.BROWN:
                    blockId = 414;
                    break;

                case BlockIds.Blocks.JungleRuins.BLUE:
                    blockId = BlockIds.Action.Liquids.WATER;
                    break;

                case 21:
                    blockId = 369;//BlockIds.Action.Liquids.MUD;
                    break;

                default:
                    if (blockId == -1)
                        return;
                    else
                        break;
            }

            digHardness[x, y] -= digStrength;

            if (digHardness[x, y] <= 0)
            {
                bot.Room.setBlock(x, y, new NormalBlock(blockId));
                lock (dugBlocksToPlaceQueueLock)
                    dugBlocksToPlaceQueue.Enqueue(new BlockWithPos(x, y, block));
            }
        }

        private void resetDigHardness()
        {
            digHardness = new float[bot.Room.Width, bot.Room.Height];

            for (int y = 0; y < bot.Room.Height; y++)
            {
                for (int x = 0; x < bot.Room.Width; x++)
                {
                    resetBlockHardness(x, y, bot.Room.getBlock(0, x, y).Id);
                }
            }
        }

        private void resetBlockHardness(int x, int y, int blockId)
        {
            if (bot.Connected)
            {
                if (x < 0 || y < 0 || x >= bot.Room.Width || y >= bot.Room.Height)

                    if (digHardness == null)
                    {
                        digHardness = new float[bot.Room.Width, bot.Room.Height];
                    }

                if (digHardness == null)
                    return;

                if (isDigable(blockId))
                {
                    digHardness[x, y] = 1F;
                }
                else if (DigBlockMap.blockTranslator.ContainsKey(blockId))
                {
                    if (Shop.shopInventory.ContainsKey(DigBlockMap.blockTranslator[blockId].GetName()))
                        digHardness[x, y] = Convert.ToInt32(Shop.shopInventory[DigBlockMap.blockTranslator[blockId].GetName()].GetDataAt(4));
                }
            }
        }

    }
}
