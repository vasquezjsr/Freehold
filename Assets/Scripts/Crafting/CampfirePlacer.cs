using Freehold.Items;
using Freehold.World;
using UnityEngine;

namespace Freehold.Crafting
{
    public class CampfirePlacer : MonoBehaviour
    {
        const int StickCost = 3;

        public bool CanPlace(Inventory inventory)
        {
            return inventory != null && inventory.Has(ItemType.Stick, StickCost);
        }

        public bool TryPlace(Inventory inventory, Transform player)
        {
            if (!CanPlace(inventory) || player == null)
            {
                return false;
            }

            if (!inventory.TryConsume(ItemType.Stick, StickCost))
            {
                return false;
            }

            var position = player.position + player.forward * 2f;
            position.y = 0.05f;
            PrimitiveFactory.CreateCampfire(position);
            return true;
        }
    }
}
