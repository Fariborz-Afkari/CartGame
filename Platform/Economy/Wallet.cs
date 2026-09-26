namespace CardGame.Platform.Economy
{
    public sealed class Wallet
    {
        public int Coins { get; private set; }
        public Wallet(int initialCoins = 0) { Coins = initialCoins < 0 ? 0 : initialCoins; }
        public void Initialize(int amount) => Coins = amount < 0 ? 0 : amount;
        public bool TrySpend(int amount)
        {
            if (amount < 0 || Coins < amount) return false;
            Coins -= amount;
            return true;
        }
        public void AddCoins(int amount)
        {
            if (amount > 0) Coins += amount;
        }
    }
}
