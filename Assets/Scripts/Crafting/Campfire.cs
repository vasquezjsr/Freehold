using Freehold.Items;
using Freehold.Needs;
using UnityEngine;

namespace Freehold.Crafting
{
    public class Campfire : MonoBehaviour
    {
        public bool IsLit { get; private set; }

        Transform _flame;
        Light _light;

        public void ConfigureVisuals(Transform flame, Light light)
        {
            _flame = flame;
            _light = light;
            SetLit(false);
        }

        public string GetPrompt(Inventory inventory)
        {
            if (!IsLit)
            {
                return "[E] Light campfire";
            }

            if (inventory.Has(ItemType.RawFood))
            {
                return "[E] Cook raw food";
            }

            if (inventory.Has(ItemType.DirtyWaterBottle))
            {
                return "[E] Boil dirty water";
            }

            return "Campfire lit — bring raw food or dirty water";
        }

        public void Interact(Inventory inventory, PlayerNeeds needs)
        {
            if (!IsLit)
            {
                SetLit(true);
                return;
            }

            if (inventory.TryConsume(ItemType.RawFood))
            {
                inventory.Add(ItemType.CookedFood);
                return;
            }

            if (inventory.TryConsume(ItemType.DirtyWaterBottle))
            {
                inventory.Add(ItemType.CleanWaterBottle);
            }
        }

        void SetLit(bool lit)
        {
            IsLit = lit;
            if (_flame != null)
            {
                _flame.gameObject.SetActive(lit);
            }

            if (_light != null)
            {
                _light.enabled = lit;
            }
        }
    }
}
