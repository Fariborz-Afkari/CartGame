using CardGame.Core.AI;
using CardGame.Core.Cards;
using CardGame.Core.Game;
using CardGame.Core.Players;
using CardGame.Games.SimpleCardGame;
using CardGame.Platform.Economy;
using CardGame.Platform.Iap;
using CardGame.Platform.Storage;
using System;
using System.Collections.Generic;

namespace CardGame.Presentation.Game
{
    public sealed class GamePresenter : IDisposable
    {
        public sealed class CardViewData
        {
            public int InstanceId { get; }
            public string Name { get; }
            public int Value { get; }

            public CardViewData(
                int instanceId,
                string name,
                int value)
            {
                InstanceId = instanceId;
                Name = name;
                Value = value;
            }
        }

        public sealed class TargetViewData
        {
            public int PlayerId { get; }
            public string PlayerName { get; }

            public TargetViewData(
                int playerId,
                string playerName)
            {
                PlayerId = playerId;
                PlayerName = playerName;
            }
        }

        public sealed class CardInteraction
        {
            public int CardId { get; }
            public bool RequiresTarget { get; }
            public IReadOnlyList<TargetViewData> Targets { get; }

            public CardInteraction(
                int cardId,
                bool requiresTarget,
                IReadOnlyList<TargetViewData> targets)
            {
                CardId = cardId;
                RequiresTarget = requiresTarget;
                Targets = targets;
            }
        }

        private const int HumanPlayerId = 0;

        private readonly GameEngine _engine;
        private readonly GameFlowController _flowController;
        private const int CoinsPerPurchase = 10;

        public IEconomyService Economy { get; }
        public IIapService Iap { get; }

        public string Status { get; private set; } = "Ready.";

        public event Action Changed;

        public GamePresenter()
        {
            _engine =
                new GameEngine(
                    "SimpleCardGame",
                    startingPlayerId: HumanPlayerId);

            Economy =
                new EconomyService(
                    new LocalPlayerData());

            Iap =
                new MockIapService();

            _flowController =
                new GameFlowController(
                    _engine,
                    new BasicAiStrategy());

            _engine.EventProduced += OnEngineEvent;
        }

        public void Dispose()
        {
            _engine.EventProduced -= OnEngineEvent;
        }

        private void OnEngineEvent(GameEvent gameEvent)
        {
            Changed?.Invoke();
        }

        // --------------------------------------------------
        // Match
        // --------------------------------------------------

        public void StartMatch()
        {
            if (!CanStartMatch)
            {
                Status = Coins < 1
                    ? "Not enough coins."
                    : "Cannot start a new match.";

                NotifyChanged();
                return;
            }

            string error;
            bool started = Economy.TryStartMatch(out error);

            if (!started)
            {
                Status = string.IsNullOrEmpty(error)
                    ? "Cannot start match."
                    : error;

                NotifyChanged();
                return;
            }

            _engine.StartMatch(
                SimpleCardGameRules.CreateDeck(),
                SimpleCardGameRules.StartingHandSize);

            Status = "Match started. Choose a card.";

            NotifyChanged();
        }

        // --------------------------------------------------
        // Presentation state
        // --------------------------------------------------

        public IReadOnlyList<CardViewData> GetPlayerHand()
        {
            PlayerState player =
                _engine.State.FindPlayer(
                    HumanPlayerId);

            if (player == null)
                return Array.Empty<CardViewData>();

            List<CardViewData> result =
                new List<CardViewData>(
                    player.Hand.Count);

            for (int i = 0;
                 i < player.Hand.Count;
                 i++)
            {
                Card card =
                    player.Hand.Cards[i];

                if (card == null ||
                    card.Definition == null)
                {
                    continue;
                }

                result.Add(
                    new CardViewData(
                        card.InstanceId,
                        card.Definition.Name,
                        card.Definition.Value));
            }

            return result;
        }

        public CardInteraction GetCardInteraction(
            int cardId)
        {
            PlayerState player =
                _engine.State.FindPlayer(
                    HumanPlayerId);

            if (player == null)
            {
                return new CardInteraction(
                    cardId,
                    false,
                    Array.Empty<TargetViewData>());
            }

            Card card =
                player.Hand.Find(cardId);

            if (card == null ||
                card.Definition == null)
            {
                return new CardInteraction(
                    cardId,
                    false,
                    Array.Empty<TargetViewData>());
            }

            if (card.Definition.Effect != CardEffect.Damage)
            {
                return new CardInteraction(
                    cardId,
                    false,
                    Array.Empty<TargetViewData>());
            }

            List<TargetViewData> targets =
                GetDamageTargets(player);

            return new CardInteraction(
                cardId,
                targets.Count > 0,
                targets);
        }

        private List<TargetViewData> GetDamageTargets(
            PlayerState actor)
        {
            List<TargetViewData> targets =
                new List<TargetViewData>();

            for (int i = 0;
                 i < _engine.State.Players.Count;
                 i++)
            {
                PlayerState player =
                    _engine.State.Players[i];

                if (player == null)
                    continue;

                if (player.Id == actor.Id)
                    continue;

                if (player.Health <= 0)
                    continue;

                targets.Add(
                    new TargetViewData(
                        player.Id,
                        player.Name));
            }

            return targets;
        }

        public bool IsPlayerTurn
        {
            get
            {
                return _engine.State.CurrentPlayerId ==
                       HumanPlayerId;
            }
        }

        public bool IsGameOver
        {
            get
            {
                return _engine.State.IsGameOver;
            }
        }

        public string CurrentPlayerName
        {
            get
            {
                PlayerState player =
                    _engine.State.FindPlayer(
                        _engine.State.CurrentPlayerId);

                return player != null
                    ? player.Name
                    : string.Empty;
            }
        }

        public int Coins
        {
            get { return Economy.Coins; }
        }

        public bool CanStartMatch
        {
            get { return Coins >= 1 && !IsGameOver; }
        }
        // --------------------------------------------------
        // Commands
        // --------------------------------------------------

        public bool DrawCard()
        {
            bool result =
                _engine.SubmitAction(
                    GameAction.DrawCard(
                        HumanPlayerId));

            Status =
                result
                    ? "Card drawn."
                    : "Cannot draw a card.";

            NotifyChanged();

            return result;
        }

        public bool PlayCard(
            int cardId,
            int? targetPlayerId = null)
        {
            bool result =
                _engine.SubmitAction(
                    GameAction.PlayCard(
                        HumanPlayerId,
                        cardId,
                        targetPlayerId));

            Status =
                result
                    ? "Card played."
                    : "Cannot play this card.";

            NotifyChanged();

            return result;
        }

        public void EndTurn()
        {
            bool result =
                _engine.SubmitAction(
                    GameAction.EndTurn(
                        HumanPlayerId));

            if (!result)
            {
                Status = "Cannot end turn.";
                NotifyChanged();
                return;
            }

            _flowController.RunAiTurns();

            Status =
                _engine.State.IsGameOver
                    ? "Match ended."
                    : "Turn ended.";

            NotifyChanged();
        }

        public void BuyCoins()
        {
            Iap.PurchaseCoins(
                CoinsPerPurchase,
                success =>
                {
                    if (!success)
                    {
                        Status = "Purchase failed.";
                        NotifyChanged();
                        return;
                    }

                    Economy.GrantCoins(CoinsPerPurchase);

                    Status = "Coins purchased.";
                    NotifyChanged();
                });
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }

    }
}
