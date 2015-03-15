using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerIOClient;
using System.IO;
using System.Runtime.Serialization;
using MasterBot;
using MasterDig.Inventory;
using System.Runtime.Serialization.Formatters.Binary;
using MasterBot.IO;
using MasterBot.Inventory;

namespace MasterDig
{
	public class Ability
	{
		string name;
		string abilityText = "do some stuff";
		int minLevel;

		public Ability(string name, string abilityText, int minLevel)
		{
			this.name = name;
			this.abilityText = abilityText;
			this.minLevel = minLevel;
		}

		public string Name { get { return name; } }
		public string AbilityText { get { return abilityText; } }
		public int MinLevel { get { return minLevel; } }
	}

	public class DigPlayer : IInventoryContainer
	{
		Stopwatch betaDigTimer = new Stopwatch();
		private Inventory.Inventory inventory = new Inventory.Inventory(100);
		Dictionary<string, Ability> abilities;
		SafeList<Ability> newAbilities;
		protected float xp = 0;
		protected int xpRequired;
		protected int digLevel_ = 0;
		protected int digMoney_ = 0;
		protected bool betaDig = false;
		protected bool fastDig = true;
		protected IPlayer player;
		private object saveloadLockObject = new object();

		public DigPlayer(IPlayer player)
		{
			this.player = player;
			Load();
		}

		~DigPlayer()
		{
			Save();
		}

		public static DigPlayer FromPlayer(IPlayer player)
		{
			if (!player.HasMetadata("digplayer"))
				player.SetMetadata("digplayer", new DigPlayer(player));
			return (DigPlayer)player.GetMetadata("digplayer");
		}

		public void Save()
		{
			lock (saveloadLockObject)
			{
				if (!Directory.Exists(@"data\)"))
					Directory.CreateDirectory(@"data\");

				string dataPath = @"data\" + player.Name;
				SaveFile saveFile = new SaveFile(dataPath);
				inventory.Save(saveFile);

				Node playerData = new Node("playerdata");
				playerData.AddNode("digxp", new Node(xp.ToString()));
				playerData.AddNode("digmoney", new Node(digMoney_.ToString()));
				saveFile.AddNode("playerdata", playerData);

				saveFile.Save();

				/*IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(dataPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                formatter.Serialize(stream, digXp);
                formatter.Serialize(stream, digMoney);
                stream.Close();*/
			}
		}

		public void Load()
		{
			lock (saveloadLockObject)
			{
				if (!Directory.Exists(@"data\)"))
					Directory.CreateDirectory(@"data\");

				string dataPath = @"data\" + player.Name;
				if (!File.Exists(dataPath))
					return;

				SaveFile saveFile = new SaveFile(dataPath);
				saveFile.Load();

				if (saveFile.Nodes.ContainsKey("playerdata"))
				{
					Node playerData = saveFile.Nodes["playerdata"];
					if (playerData.Nodes.ContainsKey("digxp"))
						xp = float.Parse(playerData.GetNode("digxp").Value);
					if (playerData.Nodes.ContainsKey("digmoney"))
						digMoney_ = int.Parse(playerData.GetNode("digmoney").Value);
				}

				inventory.Load(saveFile);

				/*digLevel_ = 0;
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                digXp = (int)formatter.Deserialize(stream);
                digMoney_ = (int)formatter.Deserialize(stream);
                stream.Close();
                xpRequired = getXpRequired(digLevel);*/
			}
		}

		public void addAbility(Ability ability)
		{
			if (!abilities.ContainsKey(ability.Name))
				abilities.Add(ability.Name, ability);
		}

		public bool hasAbility(string abilityName)
		{

			if (!abilities.ContainsKey(abilityName))
				return false;

			if (abilities[abilityName].MinLevel <= digLevel_)
				return true;
			else
				return false;


		}

		public SafeList<Ability> NewAbilities { get { return newAbilities; } }

		public int digRange { get { return ((digLevel_ > 0 && fastDig) ? 2 : 1) + ((betaDig) ? 1 : 0); } }

		public int xpRequired_ { get { return xpRequired; } }

		public int digLevel { get { return digLevel_; } }

		public int digMoney { get { return digMoney_; } set { digMoney_ = value; } }

		public int digStrength { get { return 1 + digLevel / 4; } }

		public IPlayer Player { get { return this.player; } }

		private static int getXpRequired(int level) { return BetterMath.Fibonacci(level + 2) * 8; }

		public float digXp
		{
			get { return xp; }
			set
			{
				if (value > xp)
				{
					xp = value;
					if (xp >= xpRequired)
						while (xp >= xpRequired)
						{
							xpRequired = getXpRequired(++digLevel_);
							if (xp <= xpRequired)
								updateNewAbilities();
						}
					else
						xpRequired = getXpRequired((digLevel_ = getLevel(xp)));
				}
				else
				{
					xp = value;
					xpRequired = getXpRequired((digLevel_ = getLevel(xp)));
					updateNewAbilities();
				}
			}
		}

		public IInventory Inventory
		{
			get
			{
				return inventory;
			}
		}

		public void updateNewAbilities()
		{
			return;
			newAbilities.Clear();

			foreach (Ability ability in abilities.Values)
			{
				if (ability.MinLevel == this.digLevel_)
					newAbilities.Add(ability);
			}

			if (abilities.Count == 1)
			{
				this.player.Reply("You can now " + abilities.First().Value.AbilityText + "! :P");
			}
			else if (abilities.Count > 1)
			{
				string abilityText = "";
				string lastAbility = "";
				foreach (Ability ability in abilities.Values)
				{

					abilityText += lastAbility + ", ";
					lastAbility = ability.AbilityText;
				}
				abilityText = abilityText.Substring(abilityText.Length - 2);

				// TODO: Split long texts!...
				this.player.Reply("You can now " + abilityText + " and " + lastAbility + "! :O");
			}
		}

		private static int getLevel(float xp)
		{
			int level = 0;

			while (xp > getXpRequired(level))
				level++;

			return level;
		}

	}
}
