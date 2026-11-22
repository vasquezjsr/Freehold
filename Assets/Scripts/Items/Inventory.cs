using System;
using System.Collections.Generic;
using UnityEngine;

namespace Freehold.Items
{
    /// <summary>
    /// Pocket vs backpack capacity. Pockets only accept pocket-sized items;
    /// axe / tools / wood require an equipped backpack.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        public const int PocketCapacity = 4;
        public const int BackpackCapacity = 8;

        readonly Dictionary<ItemType, int> _pockets = new Dictionary<ItemType, int>();
        readonly Dictionary<ItemType, int> _backpack = new Dictionary<ItemType, int>();

        public bool HasBackpackEquipped { get; private set; } = true;

        public event Action Changed;

        public int PocketUsed => CountItems(_pockets);
        public int BackpackUsed => CountItems(_backpack);

        public void EquipBackpack()
        {
            if (HasBackpackEquipped)
            {
                return;
            }

            HasBackpackEquipped = true;
            Changed?.Invoke();
        }

        public int GetCount(ItemType type)
        {
            return GetIn(_pockets, type) + GetIn(_backpack, type);
        }

        public bool Has(ItemType type, int amount = 1)
        {
            return GetCount(type) >= amount;
        }

        /// <summary>Add with capacity rules. Returns false + reason when blocked.</summary>
        public bool TryAdd(ItemType type, out string failReason, int amount = 1)
        {
            failReason = null;
            if (amount <= 0)
            {
                return true;
            }

            if (type == ItemType.Backpack)
            {
                EquipBackpack();
                return true;
            }

            var def = ItemDefinition.Get(type);
            for (var i = 0; i < amount; i++)
            {
                if (def.RequiresBackpack)
                {
                    if (!HasBackpackEquipped)
                    {
                        failReason = "Need a backpack for tools / wood";
                        return false;
                    }

                    if (BackpackUsed >= BackpackCapacity)
                    {
                        failReason = "Backpack full";
                        return false;
                    }

                    AddTo(_backpack, type, 1);
                    continue;
                }

                if (PocketUsed < PocketCapacity)
                {
                    AddTo(_pockets, type, 1);
                    continue;
                }

                if (HasBackpackEquipped && BackpackUsed < BackpackCapacity)
                {
                    AddTo(_backpack, type, 1);
                    continue;
                }

                failReason = HasBackpackEquipped ? "Pockets and backpack full" : "Pockets full — equip a backpack";
                return false;
            }

            Changed?.Invoke();
            return true;
        }

        public void Add(ItemType type, int amount = 1)
        {
            TryAdd(type, out _, amount);
        }

        public bool TryConsume(ItemType type, int amount = 1)
        {
            if (!Has(type, amount))
            {
                return false;
            }

            var remaining = amount;
            remaining = ConsumeFrom(_pockets, type, remaining);
            if (remaining > 0)
            {
                ConsumeFrom(_backpack, type, remaining);
            }

            Changed?.Invoke();
            return true;
        }

        public IReadOnlyDictionary<ItemType, int> Snapshot()
        {
            var combined = new Dictionary<ItemType, int>();
            foreach (var pair in _pockets)
            {
                combined[pair.Key] = pair.Value;
            }

            foreach (var pair in _backpack)
            {
                combined[pair.Key] = combined.TryGetValue(pair.Key, out var n) ? n + pair.Value : pair.Value;
            }

            return combined;
        }

        public IReadOnlyDictionary<ItemType, int> SnapshotPockets() => _pockets;

        public IReadOnlyDictionary<ItemType, int> SnapshotBackpack() => _backpack;

        static int GetIn(Dictionary<ItemType, int> bag, ItemType type)
        {
            return bag.TryGetValue(type, out var count) ? count : 0;
        }

        static void AddTo(Dictionary<ItemType, int> bag, ItemType type, int amount)
        {
            bag[type] = GetIn(bag, type) + amount;
        }

        static int ConsumeFrom(Dictionary<ItemType, int> bag, ItemType type, int amount)
        {
            var have = GetIn(bag, type);
            if (have <= 0 || amount <= 0)
            {
                return amount;
            }

            var take = Mathf.Min(have, amount);
            var left = have - take;
            if (left <= 0)
            {
                bag.Remove(type);
            }
            else
            {
                bag[type] = left;
            }

            return amount - take;
        }

        static int CountItems(Dictionary<ItemType, int> bag)
        {
            var total = 0;
            foreach (var pair in bag)
            {
                total += pair.Value;
            }

            return total;
        }
    }
}
