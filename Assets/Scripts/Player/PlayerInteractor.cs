using Freehold.Crafting;
using Freehold.Items;
using Freehold.Needs;
using Freehold.UI;
using UnityEngine;

namespace Freehold.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] float interactRange = 2.6f;

        Inventory _inventory;
        PlayerNeeds _needs;
        InteractionPrompt _prompt;
        CampfirePlacer _campfirePlacer;

        public void Configure(Inventory inventory, PlayerNeeds needs, InteractionPrompt prompt, CampfirePlacer placer)
        {
            _inventory = inventory;
            _needs = needs;
            _prompt = prompt;
            _campfirePlacer = placer;
        }

        void Update()
        {
            if (_inventory == null || _prompt == null)
            {
                return;
            }

            HandleConsumables();

            var nearestPickup = FindNearestPickup();
            var nearestCampfire = FindNearestCampfire();

            if (nearestPickup != null)
            {
                _prompt.Show($"[E] Pick up {nearestPickup.PromptLabel}");
                if (Input.GetKeyDown(KeyCode.E))
                {
                    nearestPickup.Collect(_inventory);
                }
                return;
            }

            if (nearestCampfire != null)
            {
                _prompt.Show(nearestCampfire.GetPrompt(_inventory));
                if (Input.GetKeyDown(KeyCode.E))
                {
                    nearestCampfire.Interact(_inventory, _needs);
                }
                return;
            }

            if (_campfirePlacer != null && _campfirePlacer.CanPlace(_inventory))
            {
                _prompt.Show("[C] Place campfire (3 sticks)");
                if (Input.GetKeyDown(KeyCode.C))
                {
                    _campfirePlacer.TryPlace(_inventory, transform);
                }
                return;
            }

            _prompt.Hide();
        }

        void HandleConsumables()
        {
            if (Input.GetKeyDown(KeyCode.R) && _inventory.TryConsume(ItemType.CookedFood))
            {
                _needs.Eat(35f);
                _prompt.Flash("Ate cooked food (+hunger)");
            }

            if (Input.GetKeyDown(KeyCode.T) && _inventory.TryConsume(ItemType.CleanWaterBottle))
            {
                _needs.Drink(40f);
                _inventory.Add(ItemType.PlasticBottle);
                _prompt.Flash("Drank clean water (+thirst)");
            }

            // Dirty water is drinkable but less effective — boil for better restore.
            if (Input.GetKeyDown(KeyCode.Y) && _inventory.TryConsume(ItemType.DirtyWaterBottle))
            {
                _needs.Drink(12f);
                _inventory.Add(ItemType.PlasticBottle);
                _prompt.Flash("Drank dirty water (weak restore)");
            }

            if (Input.GetKeyDown(KeyCode.F) && _inventory.TryConsume(ItemType.RawFood))
            {
                _needs.Eat(10f);
                _prompt.Flash("Ate raw food (weak restore)");
            }
        }

        ItemPickup FindNearestPickup()
        {
            ItemPickup best = null;
            var bestDist = interactRange;
            var pickups = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);
            foreach (var pickup in pickups)
            {
                var dist = Vector3.Distance(transform.position, pickup.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = pickup;
                }
            }

            return best;
        }

        Campfire FindNearestCampfire()
        {
            Campfire best = null;
            var bestDist = interactRange;
            var fires = FindObjectsByType<Campfire>(FindObjectsSortMode.None);
            foreach (var fire in fires)
            {
                var dist = Vector3.Distance(transform.position, fire.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = fire;
                }
            }

            return best;
        }
    }
}
