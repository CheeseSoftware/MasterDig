﻿using MasterBot.IO;
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
    public class Inventory
    {
        private Dictionary<int, Pair<InventoryItem, int>> storedItems;
        public int capacity { get; set; }

        public Inventory(int size)
        {
            storedItems = new Dictionary<int, Pair<InventoryItem, int>>(size);
            capacity = size;
        }

        public int GetFreeSlot()
        {
            for (int i = 0; i < capacity; i++)
            {
                if (!storedItems.ContainsKey(i))
                    return i;
            }
            return -1;
        }

        public int GetSlot(InventoryItem item)
        {
            lock (storedItems)
            {
                foreach (KeyValuePair<int, Pair<InventoryItem, int>> i in storedItems)
                {
                    if (i.Value.first == item)
                        return i.Key;
                }
                return -1;
            }
        }

        public InventoryItem GetItem(int slot)
        {
            lock (storedItems)
            {
                return storedItems[slot].first;
            }
        }

        public int GetItemCount(int slot)
        {
            lock (storedItems)
            {
                return storedItems[slot].second;
            }
        }

        public int GetItemCount(InventoryItem item)
        {
            lock (storedItems)
            {
                foreach (Pair<InventoryItem, int> i in storedItems.Values)
                {
                    if (i.first == item)
                    {
                        return i.second;
                    }
                }
                return 0;
            }
        }

        public InventoryItem GetItemByName(string name)
        {
            lock (storedItems)
            {
                foreach (Pair<InventoryItem, int> i in storedItems.Values)
                {
                    if (i.first.Name == name)
                        return (i.first);
                }
                return null;
            }
        }

        public List<Pair<InventoryItem, int>> GetItems()
        {
            lock (storedItems)
            {
                return storedItems.Values.ToList();
            }
        }

        public bool RemoveItem(InventoryItem item, int amount)
        {
            InventoryItem itemToRemove = null;
            bool removeAll = false;
            lock (storedItems)
            {
                foreach (Pair<InventoryItem, int> i in storedItems.Values)
                {
                    if (i.first == item)
                    {
                        if (i.second > amount)
                        {
                            i.second -= amount;
                            return true;
                        }
                        else
                        {
                            itemToRemove = i.first;
                            removeAll = true;
                        }
                    }
                }
                if (removeAll)
                {
                    storedItems.Remove(GetSlot(item));
                    return true;
                }
                return false;
            }
        }

        public bool AddItem(InventoryItem item, int amount)
        {
            lock (storedItems)
            {
                int slot = Contains(item);
                if (slot != -1)
                {
                    storedItems[slot].second += amount;
                    return true;
                }
                else if (storedItems.Count != capacity)
                {
                    int freeSlot = GetFreeSlot();
                    if (freeSlot != -1)
                    {
                        storedItems.Add(freeSlot, new Pair<InventoryItem, int>(item, amount));
                        return true;
                    }
                }
                return false;
            }
        }

        public override string ToString()
        {
            lock (storedItems)
            {
                string contents = "Inventory: ";
                foreach (Pair<InventoryItem, int> i in storedItems.Values)
                {
                    contents += i.second + " " + i.first.Name + ", ";
                }
                contents = contents.Remove(contents.Length - 3);
                return contents;
            }
        }

        public int Contains(InventoryItem item)
        {
            lock (storedItems)
            {
                foreach (KeyValuePair<int, Pair<InventoryItem, int>> i in storedItems)
                {
                    if (i.Value.first == item)
                    {
                        return i.Key;
                    }
                }
                return -1;
            }
        }

        public void Save(string path)
        {
            SaveFile saveFile = new SaveFile(path);
            foreach(KeyValuePair<int, Pair<InventoryItem, int>> data in storedItems)
            {
                saveFile.AddNode(new NodePath("inventory." + data.Value.first.Name + ".amount"), new Node(data.Value.second.ToString()));
                foreach(KeyValuePair<string, object> entry in data.Value.first.GetData())
                {
                    saveFile.AddNode(new NodePath("inventory." + data.Value.first.Name + "." + entry.Key), new Node(entry.Value.ToString()));
                }
            }
            saveFile.AddNode(new NodePath("data.xp"), new Node("353"));
            saveFile.Save();
        }

        public void Load(string path)
        {
            return;
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            storedItems = (Dictionary<int, Pair<InventoryItem, int>>)formatter.Deserialize(stream);
            stream.Close();
        }
    }

}
