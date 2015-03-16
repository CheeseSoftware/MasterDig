using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterBot.Room.Block;
using System.Threading;
using System.Diagnostics;
using MasterBot.SubBot;
using System.Drawing;
using MasterBot;

namespace MasterDig.Zombies
{
	class Zombie
	{
		public static int searchRange = 10;

		private IBot bot;
		public int x, y;
		private int ticksToNextSearch = 0;
		private int ticksBetweenNewPath = 0;
		private IPlayer target = null;
		private PathFinding pathFinding;
		private Stack<Node> currentPath = null;
		private static int msPerTick = 200;
		private Stopwatch playerTickStopwatch = new Stopwatch();
		private System.Timers.Timer updateTimer;

		private Queue<BlockWithPos> tail = new Queue<BlockWithPos>();
		private Dictionary<BlockPos, IBlock> tailDic = new Dictionary<BlockPos, IBlock>();

		public Zombie(int x, int y, IBot bot)
		{
			this.bot = bot;
			this.x = x;
			this.y = y;
			pathFinding = new PathFinding();

			updateTimer = new System.Timers.Timer();
			updateTimer.Elapsed += delegate { Tick(); };
			playerTickStopwatch.Start();
			updateTimer.Start();
		}

		public void Remove()
		{
			updateTimer.Stop();
			playerTickStopwatch.Stop();
			foreach (BlockWithPos block in tail)
			{
				bot.Room.setBlock(block.X, block.Y, block.Block);
			}
		}

		public void Tick()
		{
			long elapsed = playerTickStopwatch.ElapsedMilliseconds;
			if (elapsed >= msPerTick)
			{
				playerTickStopwatch.Restart();
				ticksToNextSearch--;
				if (ticksToNextSearch <= 0)
				{
					//Find a target
					IPlayer target = GetNearestPlayer(searchRange);
					if (target != null)
					{
						this.target = target;
					}
					ticksToNextSearch = 5;
				}

				//Generate path
				ticksBetweenNewPath--;
				if (ticksBetweenNewPath <= 0 || currentPath == null || currentPath.Count <= 0)
				{
					if (target != null && !(x == target.BlockX && y == target.BlockY))
					{
						pathFinding = new PathFinding();
						int xx = target.BlockX;
						int yy = target.BlockY;
						DateTime first = DateTime.Now;
						currentPath = pathFinding.FindPath(x, y, xx, yy, new List<Zombie>(Zombies.zombies), bot);
						DateTime second = DateTime.Now;
						Console.WriteLine("Pahtfinding took " + (second - first).TotalMilliseconds);
						ticksBetweenNewPath = 1;
					}
					else
						ticksBetweenNewPath = 2;
				}

				//Walk with path
				if (currentPath != null && currentPath.Count > 0)
				{
					Node next = currentPath.Pop();
					x = next.x;
					y = next.y;
					BlockPos pos = new BlockPos(0, x, y);

					while (tailDic.ContainsKey(pos) && currentPath.Count > 0)
					{
						next = currentPath.Pop();
						x = next.x;
						y = next.y;
						pos = new BlockPos(0, x, y);
					}

					if (tailDic.ContainsKey(pos))
						return;


					pos = new BlockPos(0, x, y); // tail-free block we are moving to
					IBlock toReplace = bot.Room.getBlock(0, pos.X, pos.Y);
					tailDic.Add(pos, toReplace);
					tail.Enqueue(new BlockWithPos(pos.X, pos.Y, toReplace));

					bot.Room.setBlock(pos.X, pos.Y, new NormalBlock(196, 0));

					//As we move, remove the last block of the tail
					if (tail.Count > 6)
					{
						BlockWithPos toRemove = tail.Dequeue();
						BlockPos toRemovePos = new BlockPos(0, toRemove.X, toRemove.Y);

						if (toRemove.Block != null)
						{
							bot.Room.setBlock(toRemove.X, toRemove.Y, toRemove.Block);
						}

						if (tailDic.ContainsKey(toRemovePos))
							tailDic.Remove(toRemovePos);
					}

					if (target != null && !target.IsGod && target.BlockX == x && target.BlockY == y)
					{
						KillPlayer(target);
						target = null;
					}
				}
			}
			else if (elapsed > 2)
				Thread.Sleep((int)(msPerTick - elapsed) - 1);
		}

		private void KillPlayer(IPlayer player)
		{
			bot.Connection.Send("say", "Player " + player.Name + " was brutally murdered!");
			bot.ChatSayer.Command("/kill " + player.Name);
			bot.ChatSayer.Command("/teleport " + player.Name + " " + 1 + " " + 1);
		}

		private IPlayer GetNearestPlayer(int range)
		{
			ICollection<IPlayer> players = bot.Room.Players;
			//Queue<IPlayer> closePlayers = new Queue<IPlayer>();
			//SortedList<IPlayer, int> closePlayers = new SortedList<IPlayer, int>();
			Dictionary<IPlayer, int> closePlayers = new Dictionary<IPlayer, int>();

			if (players.Count > 0)
			{
				foreach (IPlayer player in players)
				{
					if (!player.IsGod)
					{
						double distance = Math.Sqrt(Math.Abs((x - player.BlockX) ^ 2 + (y - player.BlockY) ^ 2));
						closePlayers.Add(player, (int)distance);
					}
				}

				List<KeyValuePair<IPlayer, int>> myList = closePlayers.ToList();
				myList.Sort((firstPair, nextPair) =>
				{
					return firstPair.Value.CompareTo(nextPair.Value);
				}
				);
				return myList.Count > 0 ? myList.First().Key : null;
			}
			return null;
		}
	}
}
