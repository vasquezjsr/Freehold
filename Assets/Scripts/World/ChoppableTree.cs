using System.Collections;
using Freehold.Art.Pack;
using Freehold.Items;
using UnityEngine;

namespace Freehold.World
{
    /// <summary>Axe interaction: chop → damage → fall → wood/stick loot.</summary>
    public class ChoppableTree : MonoBehaviour
    {
        [SerializeField] float maxHealth = 100f;
        [SerializeField] float damagePerChop = 34f;

        float _health;
        bool _falling;
        bool _felled;

        public bool IsAvailable => !_falling && !_felled;

        void Awake()
        {
            _health = maxHealth;
        }

        public string GetPrompt(Inventory inventory)
        {
            if (!IsAvailable)
            {
                return null;
            }

            if (inventory == null || !inventory.Has(ItemType.Axe))
            {
                return "Need an axe to chop";
            }

            var hitsLeft = Mathf.CeilToInt(_health / damagePerChop);
            return hitsLeft <= 1 ? "[E] Chop tree (final hit)" : $"[E] Chop tree ({hitsLeft} hits left)";
        }

        public bool TryChop(Inventory inventory, out bool felled)
        {
            felled = false;
            if (!IsAvailable || inventory == null || !inventory.Has(ItemType.Axe))
            {
                return false;
            }

            _health -= damagePerChop;
            if (_health > 0f)
            {
                return true;
            }

            felled = true;
            StartCoroutine(FallAndYield());
            return true;
        }

        IEnumerator FallAndYield()
        {
            _falling = true;
            var start = transform.rotation;
            var tipDir = transform.right;
            if (tipDir.sqrMagnitude < 0.01f)
            {
                tipDir = Vector3.right;
            }

            tipDir.y = 0f;
            tipDir.Normalize();
            var end = Quaternion.AngleAxis(88f, tipDir) * start;
            const float duration = 1.1f;
            var t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(start, end, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }

            var basePos = transform.position;
            SpawnYield(ItemType.Wood, basePos + new Vector3(0.4f, 0.05f, 0.1f));
            SpawnYield(ItemType.Wood, basePos + new Vector3(-0.35f, 0.05f, 0.25f));
            SpawnYield(ItemType.Stick, basePos + new Vector3(0.15f, 0.05f, -0.4f));
            SpawnYield(ItemType.Stick, basePos + new Vector3(-0.2f, 0.05f, -0.15f));
            SpawnYield(ItemType.Stick, basePos + new Vector3(0.5f, 0.05f, 0.35f));

            _felled = true;
            _falling = false;
            Destroy(gameObject);
        }

        static void SpawnYield(ItemType type, Vector3 position)
        {
            PackPropFactory.CreatePickup(type, position);
        }
    }
}
