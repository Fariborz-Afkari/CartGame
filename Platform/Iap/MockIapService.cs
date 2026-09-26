using System;

namespace CardGame.Platform.Iap
{
    public sealed class MockIapService : IIapService
    {
        public const string ProductId = "coins_10";

        public void PurchaseCoins(Action<PurchaseResult> completed)
        {
            completed?.Invoke(new PurchaseResult(true, ProductId, 10, "Mock purchase completed."));
        }
    }
}
