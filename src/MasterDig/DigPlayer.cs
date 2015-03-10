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
        public string MinLevel { get { return minLevel; } }
    }

    public class DigPlayer
    {
        Stopwatch betaDigTimer = new Stopwatch();
        public Inventory.Inventory inventory = new Inventory.Inventory(100);
        Dictionary<string, Ability> abilities;
        SafeList<Ability> newAbilities;
        protected int xp = 0;
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

        public void Save()
        {
            lock (saveloadLockObject)
            {
                if (!Directory.Exists(@"data\)"))
                    Directory.CreateDirectory(@"data\");
                if (!Directory.Exists(@"data\inventories\)"))
                    Directory.CreateDirectory(@"data\inventories\");

                string inventoryPath = @"data\inventories\" + player.Name;
                if (!File.Exists(inventoryPath))
                    File.Create(inventoryPath).Close();
                inventory.Save(inventoryPath);

                string dataPath = @"data\" + player.Name;
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(dataPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                formatter.Serialize(stream, digXp);
                formatter.Serialize(stream, digMoney);
                stream.Close();
            }
        }

        public void Load()
        {
            lock (saveloadLockObject)
            {
                if (!Directory.Exists(@"data\)"))
                    Directory.CreateDirectory(@"data\");
                if (!Directory.Exists(@"data\inventories\)"))
                    Directory.CreateDirectory(@"data\inventories\");

                string inventoryPath = @"data\inventories\" + player.Name;
                if (!File.Exists(inventoryPath))
                    return;
                inventory.Load(inventoryPath);

                string dataPath = @"data\" + player.Name;
                if (!File.Exists(dataPath))
                    return;

                digLevel_ = 0;
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                digXp = (int)formatter.Deserialize(stream);
                digMoney_ = (int)formatter.Deserialize(stream);
                stream.Close();
                xpRequired = getXpRequired(digLevel);
            }
        }

        public void addAbility(Ability ability)
        {
            if (!abilities.ContainsKey(ability))
                abilities.Add(ability.name, ability);
        }

        public bool hasAbility(string abilityName)
        {

            if (!abilities.ContainsKey(abilityName))
                return false;

            if (abilities[ability].MinLevel <= digLevel_)
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

        private static int getXpRequired(int level) { return BetterMath.Fibonacci(level + 2) * 8; }

        public int digXp
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
                            if (xp < xpRequired)
                                updateNewAbilities();
                        }
                    else
                        xpRequired = getXpRequired((digLevel_ = getLevel(xp)));
                }
            }
        }

        public void updateNewAbilities()
        {
            newAbilities.Clear();

            foreach (Ability ability in abilities)
            {
                if (ability.MinLevel == this.digLevel_)
                    newAbilities.add(ability);
            }

            if (abilities.Count == 1)
            {
                this.player.Reply("You can now " + abilities[0].AbilityText + "! :P");
            }
            else if (abilities.Count > 1)
            {
                string abilityText = "";
                string lastAbility = "";
                foreach (Ability ability in abilities)
                {

                    abilityText += lastAbility + ", ";
                    lastAbility = ability.AbilityText;
                }
                abilityText = abilityText.Substring(abilityText.Length - 2);

                // TODO: Split long texts!...
                this.player.Reply("You can now " + abilityText + " and " + lastAbility + "! :O");
            }
        }

        private static int getLevel(int xp)
        {
            int level = 0;

            while (xp > getXpRequired(level))
                level++;

            return level;
        }

    }
}
