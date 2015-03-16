using MasterBot;
using MasterBot.SubBot;
using MasterBot.Room;
using MasterBot.Room.Block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace MasterDig.Zombies
{
	class Zombies
	{
		private IBot bot;
		public static List<Zombie> zombies = new List<Zombie>(); //computer zombies

		public Zombies(IBot bot)
		{
			this.bot = bot;
		}

		public void SpawnZombie(int x, int y)
		{
			zombies.Add(new Zombie(x, y, bot));
		}

		public void RemoveZombies()
		{
			foreach (Zombie zombie in zombies)
				zombie.Remove();
			zombies.Clear();
		}
	}
}
