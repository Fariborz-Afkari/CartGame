using System;

namespace CardGame.Platform.Iap
{
    public readonly struct PurchaseResult
    {
        public bool Success { get; }
        public string ProductId { get; }
        public int CoinsGranted { get; }
        public string Message { get; }

        public PurchaseResult(bool success, string productId, int coinsGranted, string message)
        {
            Success = success; ProductId = productId; CoinsGranted = coinsGranted; Message = message;
        }
    }

    public interface IIapService
    {
        void PurchaseCoins(Action<PurchaseResult> completed);
    }
}
