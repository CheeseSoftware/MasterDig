using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDig.Inventory
{
    [Serializable]
    public class InventoryItem
    {
        private object[] data;

        public InventoryItem(object[] data)
        {
            this.data = data;
        }

        public InventoryItem(InventoryItem item)
        {
            this.data = item.data;
        }

        public string GetName()
        {
            return (string)this.data[0];
        }

        public object[] GetData()
        {
            return data;
        }

        public object GetDataAt(int index)
        {
            return (data[index]);
        }

        public void SetData(object[] data)
        {
            this.data = data;
        }

        public void SetDataAt(object data, int index)
        {
            this.data[index] = data;
        }

        public override bool Equals(object obj)
        {
            InventoryItem item = obj as InventoryItem;
            return /*item.GetData() == GetData() && */item.GetName() == GetName();
        }

        public bool Equals(InventoryItem item)
        {
            return /*item.GetData() == GetData() && */item.GetName() == GetName();
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
            return /*a.GetData() != b.GetData() || */a.GetName() != b.GetName();
        }

        public static bool operator ==(InventoryItem a, InventoryItem b)
        {
            return /*a.GetData() == b.GetData() && */a.GetName() == b.GetName();
        }

    }
}
