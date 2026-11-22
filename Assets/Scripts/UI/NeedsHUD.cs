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
                _hintText.text = "WASD move · Shift sprint · Space jump · E interact · C campfire · R eat cooked · T drink clean";
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

            var snap = _inventory.Snapshot();
            if (snap.Count == 0)
            {
                _inventoryText.text = "Inventory: empty";
                return;
            }

            var parts = new System.Collections.Generic.List<string>();
            foreach (var pair in snap)
            {
                var name = ItemDefinition.Get(pair.Key).DisplayName;
                parts.Add($"{name} x{pair.Value}");
            }

            _inventoryText.text = "Inventory: " + string.Join(" · ", parts);
        }
    }
}
