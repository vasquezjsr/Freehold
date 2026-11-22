using Freehold.Art;
using Freehold.Items;
using Freehold.Needs;
using UnityEngine;

namespace Freehold.Crafting
{
    public class Campfire : MonoBehaviour
    {
        const float CookDuration = 3.5f;

        public bool IsLit { get; private set; }

        Transform _flame;
        Light _light;
        Transform _foodSlot;
        GameObject _cookingVisual;

        enum CookState
        {
            Idle,
            Cooking,
            Ready
        }

        CookState _cookState;
        float _cookReadyAt;

        public void ConfigureVisuals(Transform flame, Light light)
        {
            _flame = flame;
            _light = light;

            var slot = new GameObject("FoodSlot");
            slot.transform.SetParent(transform, false);
            slot.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            _foodSlot = slot.transform;

            SetLit(false);
        }

        public string GetPrompt(Inventory inventory)
        {
            if (!IsLit)
            {
                return "[E] Light campfire";
            }

            if (_cookState == CookState.Ready)
            {
                return "[E] Take cooked food";
            }

            if (_cookState == CookState.Cooking)
            {
                var left = Mathf.Max(0f, _cookReadyAt - Time.time);
                return $"Cooking… {left:0.0}s";
            }

            if (inventory != null && inventory.Has(ItemType.RawFood))
            {
                return "[E] Place food on fire to cook";
            }

            if (inventory != null && inventory.Has(ItemType.DirtyWaterBottle))
            {
                return "[E] Boil dirty water";
            }

            return "Campfire lit — bring raw food or dirty water";
        }

        /// <summary>Returns a short flash message when something happened; null if no-op.</summary>
        public string Interact(Inventory inventory, PlayerNeeds needs)
        {
            if (!IsLit)
            {
                SetLit(true);
                return "Campfire lit";
            }

            if (_cookState == CookState.Ready)
            {
                if (!inventory.TryAdd(ItemType.CookedFood, out var fail))
                {
                    return fail ?? "Cannot take cooked food";
                }

                ClearCookingVisual();
                _cookState = CookState.Idle;
                return "Took cooked food";
            }

            if (_cookState == CookState.Cooking)
            {
                return null;
            }

            if (inventory.TryConsume(ItemType.RawFood))
            {
                _cookState = CookState.Cooking;
                _cookReadyAt = Time.time + CookDuration;
                ShowCookingVisual();
                return "Food placed on fire";
            }

            if (inventory.TryConsume(ItemType.DirtyWaterBottle))
            {
                inventory.Add(ItemType.CleanWaterBottle);
                return "Boiled water";
            }

            return null;
        }

        void Update()
        {
            if (_cookState != CookState.Cooking)
            {
                return;
            }

            if (Time.time < _cookReadyAt)
            {
                return;
            }

            _cookState = CookState.Ready;
            ShowReadyVisual();
        }

        void ShowCookingVisual()
        {
            ClearCookingVisual();
            if (_foodSlot == null)
            {
                return;
            }

            var mesh = LowPolyMeshes.Food(false);
            _cookingVisual = MeshBuilder.CreateMeshObject("CookingFood", mesh, LowPolyMeshes.Mat(ArtPalette.RawFood), _foodSlot.position, false);
            _cookingVisual.transform.SetParent(_foodSlot, true);
            _cookingVisual.transform.localScale = Vector3.one * 0.85f;
        }

        void ShowReadyVisual()
        {
            ClearCookingVisual();
            if (_foodSlot == null)
            {
                return;
            }

            var mesh = LowPolyMeshes.Food(true);
            _cookingVisual = MeshBuilder.CreateMeshObject("CookedFood", mesh, LowPolyMeshes.Mat(ArtPalette.CookedFood), _foodSlot.position, false);
            _cookingVisual.transform.SetParent(_foodSlot, true);
            _cookingVisual.transform.localScale = Vector3.one * 0.85f;
        }

        void ClearCookingVisual()
        {
            if (_cookingVisual != null)
            {
                Destroy(_cookingVisual);
                _cookingVisual = null;
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
