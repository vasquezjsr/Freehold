using UnityEngine;

namespace Freehold.Items
{
    public class ItemPickup : MonoBehaviour
    {
        public ItemType ItemType { get; private set; }
        public string PromptLabel { get; private set; }

        public void Configure(ItemType type)
        {
            ItemType = type;
            var def = ItemDefinition.Get(type);
            PromptLabel = def.DisplayName;
            gameObject.name = $"Pickup_{def.DisplayName}";
            gameObject.tag = "Pickup";
        }

        public bool TryCollect(Inventory inventory, out string failReason)
        {
            failReason = null;
            if (inventory == null)
            {
                failReason = "No inventory";
                return false;
            }

            if (!inventory.TryAdd(ItemType, out failReason))
            {
                return false;
            }

            Destroy(gameObject);
            return true;
        }

        public void Collect(Inventory inventory)
        {
            TryCollect(inventory, out _);
        }
    }
}
