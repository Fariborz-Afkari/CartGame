using CardGame.Core.Cards;

namespace CardGame.Core.Players
{
    /// <summary>
    /// Represents the runtime state of a player.
    ///
    /// PlayerState owns player-specific data such as health and hand.
    /// Game rules are handled by GameRules.
    /// </summary>
    public sealed class PlayerState
    {
        public int Id { get; }
        public string Name { get; }
        public bool IsHuman { get; }

        public int MaxHealth { get; }
        public int Health { get; private set; }

        public bool Guarding { get; set; }

        /// <summary>
        /// Cards currently held by this player.
        /// </summary>
        public Hand Hand { get; }

        public PlayerState(
            int id,
            string name,
            bool isHuman,
            int maxHealth = 12)
        {
            if (maxHealth <= 0)
                throw new System.ArgumentOutOfRangeException(
                    nameof(maxHealth),
                    "Max health must be greater than zero.");

            Id = id;
            Name = name;
            IsHuman = isHuman;
            MaxHealth = maxHealth;
            Health = maxHealth;

            Hand = new Hand();
        }

        /// <summary>
        /// Applies damage to the player.
        /// Guarding reduces incoming damage by half, rounded up.
        /// </summary>
        public void Damage(int amount)
        {
            if (amount <= 0)
                return;

            int finalDamage =
                Guarding
                    ? (amount + 1) / 2
                    : amount;

            Health =
                Health - finalDamage < 0
                    ? 0
                    : Health - finalDamage;

            Guarding = false;
        }

        /// <summary>
        /// Restores player health up to MaxHealth.
        /// </summary>
        public void Heal(int amount)
        {
            if (amount <= 0)
                return;

            Health =
                Health + amount > MaxHealth
                    ? MaxHealth
                    : Health + amount;

            Guarding = false;
        }

        /// <summary>
        /// Removes all cards from the player's hand.
        /// </summary>
        internal void ClearHand()
        {
            Hand.Clear();
        }
    }
}
