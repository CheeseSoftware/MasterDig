using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory
{
    public class InventoryItem
    {
        protected Dictionary<string, object> data = new Dictionary<string,object>();

        public InventoryItem(string name)
        {
            data.Add("name", name);
        }

        public InventoryItem(InventoryItem item)
        {
            this.data = item.data;
        }

        public string Name { get { return (string)data["name"]; } }

        public Dictionary<string, object> GetData()
        {
            return data;
        }

        public object GetData(string key)
        {
            if (data.ContainsKey(key))
                return data[key];
            return null;
        }

        public Boolean HasData(string key)
        {
            return data.ContainsKey(key);
        }

        public void SetData(Dictionary<string, object> data)
        {
            this.data = data;
        }

        public void SetData(string key, object value)
        {
            if (data.ContainsKey(key))
                this.data[key] = value;
            else
                data.Add(key, value);
        }

        public override bool Equals(object obj)
        {
            //TODO: FIX EQUALS
            InventoryItem item = obj as InventoryItem;
            return item.Name == Name;
        }

        public bool Equals(InventoryItem item)
        {
            return item.Name == Name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 64;
                hash = hash * 21 + data.GetHashCode();
                return hash;
            }
        }

        public static bool operator !=(InventoryItem a, InventoryItem b)
        {
            return a.Name != b.Name;
        }

        public static bool operator ==(InventoryItem a, InventoryItem b)
        {
            return a.Name == b.Name;
        }

    }
}
