namespace CardGame.Core.Players
{
    public sealed class PlayerState
    {
        public int Id { get; }
        public string Name { get; }
        public bool IsHuman { get; }
        public int MaxHealth { get; }
        public int Health { get; private set; }
        public bool Guarding { get; set; }

        public PlayerState(int id, string name, bool isHuman, int maxHealth = 12)
        {
            Id = id;
            Name = name;
            IsHuman = isHuman;
            MaxHealth = maxHealth;
            Health = maxHealth;
        }

        public void Damage(int amount)
        {
            if (amount <= 0) return;
            var finalDamage = Guarding ? (amount + 1) / 2 : amount;
            Health = Health - finalDamage < 0 ? 0 : Health - finalDamage;
            Guarding = false;
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;
            Health = Health + amount > MaxHealth ? MaxHealth : Health + amount;
            Guarding = false;
        }
    }
}
