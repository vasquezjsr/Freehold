using System;
using System.Collections.Generic;
using UnityEngine;

namespace Freehold.Items
{
    public class Inventory : MonoBehaviour
    {
        readonly Dictionary<ItemType, int> _counts = new Dictionary<ItemType, int>();

        public event Action Changed;

        public int GetCount(ItemType type)
        {
            return _counts.TryGetValue(type, out var count) ? count : 0;
        }

        public bool Has(ItemType type, int amount = 1)
        {
            return GetCount(type) >= amount;
        }

        public void Add(ItemType type, int amount = 1)
        {
            if (amount <= 0)
            {
                return;
            }

            _counts[type] = GetCount(type) + amount;
            Changed?.Invoke();
        }

        public bool TryConsume(ItemType type, int amount = 1)
        {
            if (!Has(type, amount))
            {
                return false;
            }

            var remaining = GetCount(type) - amount;
            if (remaining <= 0)
            {
                _counts.Remove(type);
            }
            else
            {
                _counts[type] = remaining;
            }

            Changed?.Invoke();
            return true;
        }

        public IReadOnlyDictionary<ItemType, int> Snapshot()
        {
            return _counts;
        }
    }
}
