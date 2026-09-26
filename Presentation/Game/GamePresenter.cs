using CardGame.Core.Game;
using CardGame.Games.SimpleCardGame;
using CardGame.Platform.Economy;
using CardGame.Platform.Iap;
using CardGame.Platform.Storage;

namespace CardGame.Presentation.Game
{
    public sealed class GamePresenter
    {
        public GameEngine Engine { get; }
        public IEconomyService Economy { get; }
        public IIapService Iap { get; }

        public string Status { get; private set; } = "Ready.";

        public GamePresenter()
        {
            Engine = new GameEngine(
                "SimpleCardGame",
                startingPlayerId: 0);

            Economy = new EconomyService(
                new LocalPlayerData());

            Iap = new MockIapService();
        }

        public void StartMatch()
        {
            string error;

            if (!Economy.TryStartMatch(out error))
            {
                Status = error;
                return;
            }

            Engine.StartMatch(
                SimpleCardGameRules.CreateDeck(),
                SimpleCardGameRules.StartingHandSize);

            Status = "Match started. Choose a card.";
        }

        public void PlayerAction(GameAction action)
        {
            if (Engine.SubmitAction(action))
            {
                Status = "Action resolved.";
            }
            else
            {
                Status = "Action rejected.";
            }
        }

        public void BuyCoins()
        {
            Iap.PurchaseCoins(result =>
            {
                if (result.Success)
                {
                    Economy.GrantCoins(result.CoinsGranted);

                    Status =
                        result.Message +
                        " +" +
                        result.CoinsGranted +
                        " coins.";
                }
                else
                {
                    Status = result.Message;
                }
            });
        }
    }
}
