using CardGame.Platform.Storage;

namespace CardGame.Platform.Economy
{
    public sealed class EconomyService : IEconomyService
    {
        private const int MatchCost = 1;
        private readonly Wallet wallet;
        private readonly LocalPlayerData storage;

        public int Coins => wallet.Coins;

        public EconomyService(LocalPlayerData storage)
        {
            this.storage = storage;
            wallet = new Wallet();
            wallet.Initialize(storage.LoadCoins());
        }

        public bool TryStartMatch(out string error)
        {
            if (!wallet.TrySpend(MatchCost))
            {
                error = "Not enough coins. Use the Mock IAP button to buy 10 coins.";
                return false;
            }
            Save();
            error = string.Empty;
            return true;
        }

        public void GrantCoins(int amount)
        {
            wallet.AddCoins(amount);
            Save();
        }

        public void Save() => storage.SaveCoins(wallet.Coins);
    }
}
