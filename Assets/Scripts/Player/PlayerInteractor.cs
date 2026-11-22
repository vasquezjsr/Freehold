using Freehold.Art.Pack;
using Freehold.Crafting;
using Freehold.Items;
using Freehold.Needs;
using Freehold.UI;
using Freehold.World;
using UnityEngine;

namespace Freehold.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] float interactRange = 2.6f;
        /// <summary>Axe reach measured from bark surface (not tree root center).</summary>
        [SerializeField] float chopReachFromBark = 1.85f;

        Inventory _inventory;
        PlayerNeeds _needs;
        InteractionPrompt _prompt;
        CampfirePlacer _campfirePlacer;
        float _chopCooldownUntil;

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
            var nearestTree = FindNearestTree();

            if (nearestPickup != null)
            {
                _prompt.Show($"[E] Pick up {nearestPickup.PromptLabel}");
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (!nearestPickup.TryCollect(_inventory, out var fail))
                    {
                        _prompt.Flash(fail ?? "Cannot pick up");
                    }
                }

                return;
            }

            if (nearestCampfire != null)
            {
                _prompt.Show(nearestCampfire.GetPrompt(_inventory));
                if (Input.GetKeyDown(KeyCode.E))
                {
                    var msg = nearestCampfire.Interact(_inventory, _needs);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        _prompt.Flash(msg);
                    }
                }

                return;
            }

            if (nearestTree != null && _inventory.Has(ItemType.Axe) && Time.time >= _chopCooldownUntil)
            {
                _prompt.Show(nearestTree.GetPrompt(_inventory));
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (nearestTree.TryChop(_inventory, out var felled))
                    {
                        PackRigLocomotionVisual.TriggerChop(gameObject);
                        PlayerLocomotionVisualChop.Trigger(gameObject);
                        _chopCooldownUntil = Time.time + 0.55f;
                        _prompt.Flash(felled ? "Tree felled — wood & sticks" : "Chop!");
                    }
                }

                return;
            }

            if (nearestTree != null && !_inventory.Has(ItemType.Axe))
            {
                var tip = nearestTree.GetPrompt(_inventory);
                if (!string.IsNullOrEmpty(tip))
                {
                    _prompt.Show(tip);
                    return;
                }
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

        ChoppableTree FindNearestTree()
        {
            ChoppableTree best = null;
            var bestDist = chopReachFromBark;
            var trees = FindObjectsByType<ChoppableTree>(FindObjectsSortMode.None);
            foreach (var tree in trees)
            {
                if (tree == null || !tree.IsAvailable)
                {
                    continue;
                }

                // Range from bark, so standing outside a thick trunk still prompts/chops.
                var dist = tree.DistanceToBark(transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = tree;
                }
            }

            return best;
        }
    }

    /// <summary>Chop swing hook for procedural humanoid fallback.</summary>
    public static class PlayerLocomotionVisualChop
    {
        public static void Trigger(GameObject player)
        {
            var loco = player.GetComponent<Freehold.Art.PlayerLocomotionVisual>();
            loco?.PlayChop();
        }
    }
}
