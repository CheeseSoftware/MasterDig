﻿using MasterBot;
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

        private bool isDug(int blockId)
        {
            if (blockId == 4 || blockId == 414)
                return true;
            else if (blockId == 119 || blockId == 369) // water and mud
                return true;
            else
                return false;
        }

        private void DigBlock(int x, int y, IPlayer player, float digStrength, bool mining, bool explosion = false)
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
                InventoryItem temp = ItemManager.GetItemFromOreId(block.Id);
                if (temp != null)
                {
                    blockId = 414;

                    if (!player.HasMetadata("digplayer"))
                        player.SetMetadata("digplayer", new DigPlayer(player));
                    DigPlayer digPlayer = (DigPlayer)player.GetMetadata("digplayer");

                    if (digPlayer.digLevel >= Convert.ToInt32(temp.GetData("diglevel")))
                    {
                        if (digHardness[x, y] <= digStrength)
                        {
                            digHardness[x, y] = 0F;

                            InventoryItem newsak = new InventoryItem(temp);

                            digPlayer.inventory.AddItem(newsak, 1);
                            int oldLevel = digPlayer.digLevel;
                            digPlayer.digXp += Convert.ToInt32(temp.GetData("xpgain"));
                            int newLevel = digPlayer.digLevel;
                            if (newLevel > oldLevel)
                                player.Reply("You have leveled up to level " + newLevel + "!");
                        }
                    }
                    else
                    {
                        return;
                    }

                }
            }

            if (explosion)
                blockId = 414;

            switch (block.Id)
            {
                case BlockIds.Blocks.Sand.BROWN:
                case BlockIds.Blocks.Sand.GRAY:
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
                digHardness[x, y] = 0f;

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
                    if (blockId == Skylight.BlockIds.Blocks.Sand.BROWN)
                        digHardness[x, y] = 1F;
                    else
                        digHardness[x, y] = 15F;
                }
                else if (ItemManager.GetItemFromOreId(blockId) != null)
                {
                    digHardness[x, y] = ItemManager.GetOreByName(ItemManager.GetItemFromOreId(blockId).Name).Hardness;
                }
                else if (!isDug(blockId))
                    digHardness[x, y] = -1f;
            }
        }

    }
}
