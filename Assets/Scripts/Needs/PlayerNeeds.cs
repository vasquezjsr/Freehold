using UnityEngine;

namespace Freehold.Needs
{
    public class PlayerNeeds : MonoBehaviour
    {
        [SerializeField] float maxHunger = 100f;
        [SerializeField] float maxThirst = 100f;
        [SerializeField] float maxStamina = 100f;
        [SerializeField] float hungerDrainPerSecond = 1.2f;
        [SerializeField] float thirstDrainPerSecond = 1.6f;
        [SerializeField] float staminaRegenPerSecond = 14f;
        [SerializeField] float staminaDrainPerSecond = 22f;
        [SerializeField] float lowNeedThreshold = 15f;

        public float Hunger { get; private set; }
        public float Thirst { get; private set; }
        public float Stamina { get; private set; }

        public float HungerNormalized => Hunger / maxHunger;
        public float ThirstNormalized => Thirst / maxThirst;
        public float StaminaNormalized => Stamina / maxStamina;

        public bool IsCriticallyHungry => Hunger <= lowNeedThreshold;
        public bool IsCriticallyThirsty => Thirst <= lowNeedThreshold;
        public bool HasStamina => Stamina > 1f;

        void Awake()
        {
            Hunger = maxHunger * 0.75f;
            Thirst = maxThirst * 0.7f;
            Stamina = maxStamina;
        }

        void Update()
        {
            Hunger = Mathf.Max(0f, Hunger - hungerDrainPerSecond * Time.deltaTime);
            Thirst = Mathf.Max(0f, Thirst - thirstDrainPerSecond * Time.deltaTime);
        }

        public bool TrySpendStamina(float deltaTime)
        {
            if (Stamina <= 0f)
            {
                return false;
            }

            Stamina = Mathf.Max(0f, Stamina - staminaDrainPerSecond * deltaTime);
            return Stamina > 0f;
        }

        public void RegenStamina(float deltaTime)
        {
            Stamina = Mathf.Min(maxStamina, Stamina + staminaRegenPerSecond * deltaTime);
        }

        public void Eat(float amount)
        {
            Hunger = Mathf.Min(maxHunger, Hunger + amount);
        }

        public void Drink(float amount)
        {
            Thirst = Mathf.Min(maxThirst, Thirst + amount);
        }
    }
}
