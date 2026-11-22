using Freehold.Items;
using Freehold.Needs;
using UnityEngine;
using UnityEngine.UI;

namespace Freehold.UI
{
    public class NeedsHUD : MonoBehaviour
    {
        PlayerNeeds _needs;
        Inventory _inventory;
        Image _hungerFill;
        Image _thirstFill;
        Image _staminaFill;
        Text _inventoryText;
        Text _hintText;
        bool _showPanel = true;

        public void Configure(PlayerNeeds needs, Inventory inventory, Image hunger, Image thirst, Image stamina, Text inventoryText, Text hintText)
        {
            _needs = needs;
            _inventory = inventory;
            _hungerFill = hunger;
            _thirstFill = thirst;
            _staminaFill = stamina;
            _inventoryText = inventoryText;
            _hintText = hintText;

            if (_inventory != null)
            {
                _inventory.Changed += RefreshInventory;
                RefreshInventory();
            }

            if (_hintText != null)
            {
                _hintText.text =
                    "WASD move · Shift sprint · Space jump · E interact/chop/take food · C campfire · I backpack · R eat cooked · T drink clean";
            }
        }

        void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.Changed -= RefreshInventory;
            }
        }

        void Update()
        {
            if (_needs == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                _showPanel = !_showPanel;
                RefreshInventory();
            }

            SetFill(_hungerFill, _needs.HungerNormalized, new Color(0.85f, 0.45f, 0.2f));
            SetFill(_thirstFill, _needs.ThirstNormalized, new Color(0.25f, 0.65f, 0.95f));
            SetFill(_staminaFill, _needs.StaminaNormalized, new Color(0.35f, 0.85f, 0.45f));
        }

        void SetFill(Image image, float normalized, Color color)
        {
            if (image == null)
            {
                return;
            }

            image.fillAmount = Mathf.Clamp01(normalized);
            image.color = color;
        }

        void RefreshInventory()
        {
            if (_inventoryText == null || _inventory == null)
            {
                return;
            }

            if (!_showPanel)
            {
                _inventoryText.text = $"[I] Pockets {_inventory.PocketUsed}/{Inventory.PocketCapacity} · Backpack {_inventory.BackpackUsed}/{Inventory.BackpackCapacity}";
                return;
            }

            var pockets = FormatBag(_inventory.SnapshotPockets());
            var pack = _inventory.HasBackpackEquipped
                ? FormatBag(_inventory.SnapshotBackpack())
                : "(not equipped)";

            _inventoryText.text =
                $"Pockets ({_inventory.PocketUsed}/{Inventory.PocketCapacity}): {pockets}\n" +
                $"Backpack ({_inventory.BackpackUsed}/{Inventory.BackpackCapacity}): {pack}\n" +
                "Pocket items only in pockets · Axe / tools / wood need backpack · [I] toggle";
        }

        static string FormatBag(System.Collections.Generic.IReadOnlyDictionary<ItemType, int> bag)
        {
            if (bag == null || bag.Count == 0)
            {
                return "empty";
            }

            var parts = new System.Collections.Generic.List<string>();
            foreach (var pair in bag)
            {
                parts.Add($"{ItemDefinition.Get(pair.Key).DisplayName} x{pair.Value}");
            }

            return string.Join(" · ", parts);
        }
    }
}
