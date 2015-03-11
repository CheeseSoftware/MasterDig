using Graphics.Tools.Noise;
using MasterBot;
using MasterBot.Room;
using MasterBot.Room.Block;
using MasterBot.SubBot;
using MasterDig.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig
{
    public partial class MasterDig : ASubBot, IPlugin
    {
        IBlockDrawer generatorDrawer;

        private void Generate(int width, int height, int seed)
        {
            int centerHoleDiameter = 10;

            generatorDrawer = bot.Room.BlockDrawerPool.CreateBlockDrawer(1);
            Random random = new Random();
            Graphics.Tools.Noise.Primitive.SimplexPerlin noise = new Graphics.Tools.Noise.Primitive.SimplexPerlin(seed, NoiseQuality.Best);
            BlockMap blockMap = new BlockMap(bot, width, height);

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    double distanceFromCenter = Math.Sqrt(Math.Pow(x - width / 2, 2) + Math.Pow(y - height / 2, 2)) / ((width > height) ? width : height) * 2;
                    double distanceFromCenterPow = Math.Pow(distanceFromCenter, 1.5);

                    if (noise.GetValue(x * 0.015625F, y * 0.015625F, 0) > 1 - 0.25F * distanceFromCenterPow)                 // slimy mud
                        blockMap.setBlock(x, y, new NormalBlock(21, 0));

                    else if (noise.GetValue(x * 0.03125F, y * 0.03125F, 32) > 1 - 0.75 * distanceFromCenter)      // slimy mud
                        blockMap.setBlock(x, y, new NormalBlock(21, 0));

                    else if (noise.GetValue(x * 0.015625F, y * 0.015625F, 48) > 1 - 0.5 * distanceFromCenter) // Water
                        blockMap.setBlock(x, y, new NormalBlock(197, 0));

                    else if (noise.GetValue(x * 0.03125F, y * 0.03125F, 64) > 1 - 0.75 * distanceFromCenter) //wet stones
                        blockMap.setBlock(x, y, new NormalBlock(197, 0));

                    else if (noise.GetValue(x * 0.0078125F, y * 0.0078125F, 96) > 1 - 0.75 * distanceFromCenterPow)
                        blockMap.setBlock(x, y, new NormalBlock((int)Blocks.Stone, 0));

                    else if (noise.GetValue(x * 0.015625F, y * 0.015625F, 128) > 1 - 0.75 * distanceFromCenter)
                        blockMap.setBlock(x, y, new NormalBlock((int)Blocks.Stone, 0));

                    else if (distanceFromCenter > 0.5)
                        blockMap.setBlock(x, y, new NormalBlock((int)Skylight.BlockIds.Blocks.Sand.GRAY, 0));

                    else// if (noise.GetValue(x * 0.015625F, y * 0.015625F, 160) > 0)
                        blockMap.setBlock(x, y, new NormalBlock(Skylight.BlockIds.Blocks.Sand.BROWN, 0));

                }
            }

            Queue<BlockWithPos> blockQueue = new Queue<BlockWithPos>();

            for (int i = 0; i < 64; i++)
                blockQueue.Enqueue(new BlockWithPos(random.Next(1, width - 1), random.Next(1, height - 1), new NormalBlock((int)Blocks.Stone, 0)));
            for (int i = 0; i < 64; i++)
                blockQueue.Enqueue(new BlockWithPos(random.Next(1, width - 1), random.Next(1, height - 1), new NormalBlock((int)Blocks.Copper, 0)));
            for (int i = 0; i < 32; i++)
                blockQueue.Enqueue(new BlockWithPos(random.Next(1, width - 1), random.Next(1, height - 1), new NormalBlock((int)Blocks.Iron, 0)));
            for (int i = 0; i < 16; i++)
                blockQueue.Enqueue(new BlockWithPos(random.Next(1, width - 1), random.Next(1, height - 1), new NormalBlock((int)Blocks.Gold, 0)));
            for (int i = 0; i < 8; i++)
                blockQueue.Enqueue(new BlockWithPos(random.Next(1, width - 1), random.Next(1, height - 1), new NormalBlock((int)Blocks.Emerald, 0)));

            int amount = 1536;//2048 later

            while (blockQueue.Count > 0 && amount > 0)
            {
                BlockWithPos block = blockQueue.Dequeue();

                blockMap.setBlock(block.X, block.Y, block.Block);

                if (random.Next(8) == 0)
                {
                    BlockWithPos block2 = null;

                    switch (random.Next(4))
                    {
                        case 0: block2 = new BlockWithPos(block.X + 1, block.Y, block.Block); break;
                        case 1: block2 = new BlockWithPos(block.X, block.Y + 1, block.Block); break;
                        case 2: block2 = new BlockWithPos(block.X - 1, block.Y, block.Block); break;
                        case 3: block2 = new BlockWithPos(block.X, block.Y - 1, block.Block); break;
                    }

                    Console.WriteLine("s");

                    if (block2 != blockMap.getBlock(0, block2.X, block2.Y) && block2.X > 1 && block2.Y > 1 && block2.X < width - 1 && block2.Y < height - 1)
                    {
                        blockQueue.Enqueue(block2);
                        blockMap.setBlock(block2.X, block2.Y, block2.Block);
                        amount--;
                    }
                }

                blockQueue.Enqueue(block);
            }

            //Make hole in center for the shop
            for (int x = width / 2 - (centerHoleDiameter / 2 + 1); x < width / 2 + centerHoleDiameter / 2; x++)
            {
                for (int y = height / 2 - (centerHoleDiameter / 2 + 1); y < height / 2 + centerHoleDiameter / 2; y++)
                {
                    blockMap.setBlock(x, y, new NormalBlock(414, 0));

                }
            }

            blockMap.setBlock(width / 2 - 1, height / 2 - 1, new NormalBlock(255, 0));
            Shop.SetLocation(width / 2 - 1, height / 2 - 2);
            blockMap.setBlock(width / 2 - 1, height / 2 - 2, new NormalBlock(Skylight.BlockIds.Blocks.Pirate.CHEST, 0));


            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (blockMap.getBlock(0, x, y) != null)
                    {
                        IBlock block = blockMap.getBlock(0, x, y);
                        generatorDrawer.PlaceBlock(x, y, block);
                        IBlock background = null;
                        switch (block.Id)
                        {
                            case 197:
                                background = new NormalBlock(574, 1);
                                break;
                            case 21:
                                background = new NormalBlock(630, 1);
                                break;
                            default:
                                background = new NormalBlock(584, 1);
                                break;
                        }
                        generatorDrawer.PlaceBlock(x, y, background);
                        resetBlockHardness(x, y, blockMap.getBlock(0, x, y).Id);
                    }
                }
            }
            generatorDrawer.Start();
        }
    }
}
