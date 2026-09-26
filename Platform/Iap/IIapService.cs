using System;

namespace CardGame.Platform.Iap
{
    public interface IIapService
    {
        void PurchaseCoins(
            int amount,
            Action<bool> completed);
    }
}
