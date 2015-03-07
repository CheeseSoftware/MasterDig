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

namespace MasterDig
{
    public class DigPlayer
    {
        Stopwatch betaDigTimer = new Stopwatch();
        public Inventory.Inventory inventory = new Inventory.Inventory(100);
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
            return;
            lock (saveloadLockObject)
            {
                string path = @"data\" + player.Name;
                if (!File.Exists(path))
                    File.Create(path);
                Pair<IFormatter, Stream> writeStuff = inventory.Save(path);
                writeStuff.first.Serialize(writeStuff.second, digXp);
                writeStuff.first.Serialize(writeStuff.second, digMoney);
                writeStuff.second.Close();
            }
        }

        public void Load()
        {
            return;
            lock (saveloadLockObject)
            {
                string path = @"data\" + player.Name;
                if (!File.Exists(path))
                    File.Create(path);
                digLevel_ = 0;
                Pair<IFormatter, Stream> writeStuff = inventory.Load(path);
                digXp = (int)writeStuff.first.Deserialize(writeStuff.second);
                digMoney_ = (int)writeStuff.first.Deserialize(writeStuff.second);
                writeStuff.second.Close();
                xpRequired = getXpRequired(digLevel);
            }
        }

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
                            xpRequired = getXpRequired(++digLevel_);
                    else
                        xpRequired = getXpRequired((digLevel_ = getLevel(xp)));
                }
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
