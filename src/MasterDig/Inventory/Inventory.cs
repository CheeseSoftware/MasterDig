using MasterBot;
using MasterBot.Inventory;
using MasterBot.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory
{
    public class Inventory : IInventory
    {
        private Dictionary<string, Pair<IInventoryItem, int>> storedItems;
        public int capacity { get; set; }

        public Inventory(int size)
        {
            storedItems = new Dictionary<string, Pair<IInventoryItem, int>>(size);
            capacity = size;
        }

        public IInventoryItem GetItem(string name)
        {
            lock (storedItems)
            {
                if (storedItems.ContainsKey(name))
                    return storedItems[name].first;
                return null;
            }
        }

        public int GetItemCount(string name)
        {
            lock (storedItems)
            {
                if (storedItems.ContainsKey(name))
                    return storedItems[name].second;
                return 0;
            }
        }

        public int GetItemCount(IInventoryItem item)
        {
            lock (storedItems)
            {
                if (storedItems.ContainsKey(item.Name))
                    return storedItems[item.Name].second;
                return 0;
            }
        }

        public List<Pair<IInventoryItem, int>> GetItems()
        {
            lock (storedItems)
            {
                return storedItems.Values.ToList();
            }
        }

        public bool RemoveItem(IInventoryItem item, int amount)
        {
            return RemoveItem(item.Name, amount);
        }

        public bool RemoveItem(string item, int amount)
        {
            lock (storedItems)
            {
                if (storedItems.ContainsKey(item))
                {
                    var i = storedItems[item];
                    if (i.second > amount)
                    {
                        i.second -= amount;
                        return true;
                    }
                    else
                    {
                        storedItems.Remove(i.first.Name);
                        return true;
                    }
                }
                return false;
            }
        }

        public bool AddItem(IInventoryItem item, int amount)
        {
            lock (storedItems)
            {
                if (storedItems.ContainsKey(item.Name))
                {
                    storedItems[item.Name].second += amount;
                    return true;
                }
                else if (storedItems.Count < capacity)
                {
                    storedItems.Add(item.Name, new Pair<IInventoryItem, int>(item, amount));
                    return true;
                }
                return false;
            }
        }

        public override string ToString()
        {
            lock (storedItems)
            {
                string contents = "Inventory: ";
                foreach (Pair<IInventoryItem, int> i in storedItems.Values)
                {
                    contents += i.second + " " + i.first.Name + ", ";
                }
                contents = contents.Remove(contents.Length - 2);
                return contents;
            }
        }

        public bool Contains(string item)
        {
            lock (storedItems)
            {
                if (storedItems.ContainsKey(item))
                    return true;
                return false;
            }
        }

        public bool Contains(IInventoryItem item)
        {
            return (Contains(item.Name));
        }

        public SaveFile Save(SaveFile saveFile)
        {
            //For each item in inventory
            foreach (KeyValuePair<string, Pair<IInventoryItem, int>> data in storedItems)
            {
                saveFile.AddNode(new NodePath("inventory." + data.Value.first.Name + ".amount"), new Node(data.Value.second.ToString()));
                //For each data entry in item
                foreach (KeyValuePair<string, object> entry in data.Value.first.GetData())
                {
                    saveFile.AddNode(new NodePath("inventory." + data.Value.first.Name + ".data." + entry.Key), new Node(entry.Value.ToString()));
                }
            }
            return saveFile;
        }

        public void Load(SaveFile file)
        {
            Dictionary<string, Node> nodes = file.Nodes;
            if (nodes.ContainsKey("inventory"))
            {
                Node inventory = nodes["inventory"];
                //For each item in inventory
                foreach (KeyValuePair<string, Node> item in inventory.Nodes)
                {
					IInventoryItem inventoryItem = new InventoryItem(item.Key);
                    int amount = int.Parse(item.Value.Nodes["amount"].Value);
                    //For each data entry in item
                    foreach (KeyValuePair<string, Node> data in item.Value.Nodes)
                    {
                        if (data.Key.Equals("amount"))
                            continue;
                        inventoryItem.SetData(data.Key, data.Value.Value);
                    }
                    AddItem(inventoryItem, amount);
                }
            }
        }
    }

}
