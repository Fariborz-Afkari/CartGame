using System.Collections.Generic;
using CardGame.Core.Cards;
using CardGame.Core.Game;
using CardGame.Core.Players;
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

        public IReadOnlyList<Card> GetPlayerHand()
        {
            PlayerState player =
                Engine.State.FindPlayer(0);

            if (player == null)
                return new List<Card>();

            return player.Hand.Cards;
        }

        public bool DrawCard()
        {
            bool result = Engine.SubmitAction(
                GameAction.DrawCard(0));

            Status = result
                ? "Card drawn."
                : "Cannot draw a card.";

            return result;
        }

        public bool PlayCard(
            int cardId,
            int? targetPlayerId = null)
        {
            bool result = Engine.SubmitAction(
                GameAction.PlayCard(
                    0,
                    cardId,
                    targetPlayerId));

            Status = result
                ? "Card played."
                : "Cannot play this card.";

            return result;
        }

        public void EndTurn()
        {
            bool result = Engine.SubmitAction(
                GameAction.EndTurn(0));

            Status = result
                ? "Turn ended."
                : "Cannot end turn.";
        }

        public void BuyCoins()
        {
            Iap.PurchaseCoins(result =>
            {
                if (result.Success)
                {
                    Economy.GrantCoins(
                        result.CoinsGranted);

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
