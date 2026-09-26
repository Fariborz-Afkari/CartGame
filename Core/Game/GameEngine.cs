using System;
using System.Collections.Generic;
using CardGame.Core.Cards;
using CardGame.Core.Players;

namespace CardGame.Core.Game
{
    /// <summary>
    /// Coordinates the flow of a game.
    ///
    /// GameEngine:
    /// - receives GameAction instances
    /// - validates turn ownership
    /// - delegates gameplay rules to GameRules
    /// - records and publishes GameEvent instances
    /// - controls turn progression and match lifecycle
    ///
    /// GameEngine does not know the composition of a specific game's deck.
    /// The concrete game supplies the cards when a match starts.
    /// </summary>
    public sealed class GameEngine
    {
        private readonly int _startingPlayerId;

        public GameState State { get; }

        public event Action<GameEvent> EventProduced;

        public GameEngine(
            string gameId,
            int startingPlayerId = 0)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException(
                    "Game id cannot be empty.",
                    nameof(gameId));

            _startingPlayerId = startingPlayerId;

            State = new GameState(
                gameId,
                startingPlayerId);
        }

        /// <summary>
        /// Starts a match without a predefined deck.
        ///
        /// This overload is kept for Core consumers that want to
        /// configure the deck separately.
        /// </summary>
        public void StartMatch()
        {
            StartMatch(null, 0);
        }

        /// <summary>
        /// Starts a match using the supplied concrete card instances.
        /// The deck is shuffled and each player receives the requested
        /// number of opening cards.
        /// </summary>
        public void StartMatch(
            IEnumerable<Card> initialDeck,
            int initialHandSize)
        {
            if (initialHandSize < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(initialHandSize));

            State.Reset();

            AddDefaultPlayers();

            if (State.FindPlayer(_startingPlayerId) == null)
            {
                throw new InvalidOperationException(
                    $"Starting player {_startingPlayerId} does not exist.");
            }

            if (initialDeck != null)
            {
                State.Deck.AddRange(initialDeck);
                State.Deck.Shuffle();
            }

            DealOpeningHands(initialHandSize);

            if (initialHandSize > 0 &&
                State.Deck.Count == 0 &&
                !AllPlayersHaveCards())
            {
                throw new InvalidOperationException(
                    "The supplied deck does not contain enough cards " +
                    "for the requested opening hands.");
            }

            State.Phase = GamePhase.TurnStart;
            State.IsGameOver = false;
            State.CurrentPlayerId = _startingPlayerId;
            State.TurnNumber = 1;
            State.WinnerId = null;

            Emit(GameEvent.MatchStarted());

            StartTurn(_startingPlayerId);
        }

        public bool SubmitAction(GameAction action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (State.IsGameOver)
            {
                Reject(
                    action.PlayerId,
                    "The match has already ended.");

                return false;
            }

            if (!IsPlayerTurn(action.PlayerId))
            {
                Reject(
                    action.PlayerId,
                    "It is not this player's turn.");

                return false;
            }

            switch (action.Type)
            {
                case GameActionType.DrawCard:
                    return ResolveDrawCard(action);

                case GameActionType.PlayCard:
                    return ResolvePlayCard(action);

                case GameActionType.EndTurn:
                    return EndTurn(action.PlayerId);

                default:
                    Reject(
                        action.PlayerId,
                        "Unsupported game action.");

                    return false;
            }
        }

        private bool ResolveDrawCard(GameAction action)
        {
            PlayerState actor =
                State.FindPlayer(action.PlayerId);

            if (actor == null)
            {
                Reject(
                    action.PlayerId,
                    "The player does not exist.");

                return false;
            }

            if (!GameRules.Resolve(State, action))
            {
                Reject(
                    action.PlayerId,
                    "The card cannot be drawn.");

                return false;
            }

            Card drawnCard =
                actor.Hand.Cards[actor.Hand.Count - 1];

            Emit(GameEvent.CardDrawn(
                action.PlayerId,
                drawnCard.InstanceId));

            return true;
        }

        private bool ResolvePlayCard(GameAction action)
        {
            if (!action.CardId.HasValue)
            {
                Reject(
                    action.PlayerId,
                    "PlayCard requires a card id.");

                return false;
            }

            if (!GameRules.Resolve(State, action))
            {
                Reject(
                    action.PlayerId,
                    "The card cannot be played.");

                return false;
            }

            Emit(GameEvent.CardPlayed(
                action.PlayerId,
                action.CardId.Value,
                action.TargetPlayerId));

            CheckGameOver();

            return true;
        }

        private void DealOpeningHands(int handSize)
        {
            if (handSize <= 0)
                return;

            for (int round = 0; round < handSize; round++)
            {
                for (int i = 0; i < State.Players.Count; i++)
                {
                    Card card = State.Deck.Draw();

                    if (card == null)
                        throw new InvalidOperationException(
                            "The supplied deck does not contain enough " +
                            "cards for the opening hands.");

                    State.Players[i].Hand.Add(card);
                }
            }
        }

        private bool AllPlayersHaveCards()
        {
            for (int i = 0; i < State.Players.Count; i++)
            {
                if (State.Players[i].Hand.Count == 0)
                    return false;
            }

            return true;
        }

        private bool EndTurn(int playerId)
        {
            Emit(GameEvent.TurnEnded(playerId));

            if (CheckGameOver())
                return true;

            int nextPlayerId =
                FindNextLivingPlayer(playerId);

            if (nextPlayerId < 0)
            {
                EndMatch(null);
                return true;
            }

            State.CurrentPlayerId = nextPlayerId;
            State.TurnNumber++;

            StartTurn(nextPlayerId);

            return true;
        }

        private void StartTurn(int playerId)
        {
            PlayerState player =
                State.FindPlayer(playerId);

            if (player == null || player.Health <= 0)
            {
                EndMatch(null);
                return;
            }

            State.Phase = GamePhase.Turn;

            Emit(GameEvent.TurnStarted(playerId));
        }

        private bool IsPlayerTurn(int playerId)
        {
            if (State.IsGameOver)
                return false;

            if (State.Phase != GamePhase.Turn)
                return false;

            if (State.CurrentPlayerId != playerId)
                return false;

            PlayerState player =
                State.FindPlayer(playerId);

            return player != null &&
                   player.Health > 0;
        }

        private int FindNextLivingPlayer(int currentPlayerId)
        {
            if (State.Players.Count == 0)
                return -1;

            int currentIndex = -1;

            for (int i = 0; i < State.Players.Count; i++)
            {
                if (State.Players[i].Id == currentPlayerId)
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex < 0)
                return -1;

            for (int offset = 1;
                 offset <= State.Players.Count;
                 offset++)
            {
                int index =
                    (currentIndex + offset) %
                    State.Players.Count;

                PlayerState player =
                    State.Players[index];

                if (player.Health > 0)
                    return player.Id;
            }

            return -1;
        }

        private bool CheckGameOver()
        {
            if (State.IsGameOver)
                return true;

            PlayerState human =
                State.FindPlayer(0);

            if (human == null || human.Health <= 0)
            {
                EndMatch(null);
                return true;
            }

            int livingPlayers = 0;
            int lastLivingPlayerId = -1;

            for (int i = 0; i < State.Players.Count; i++)
            {
                PlayerState player =
                    State.Players[i];

                if (player.Health > 0)
                {
                    livingPlayers++;
                    lastLivingPlayerId = player.Id;
                }
            }

            if (livingPlayers <= 1)
            {
                EndMatch(lastLivingPlayerId);
                return true;
            }

            return false;
        }

        private void EndMatch(int? winnerId)
        {
            if (State.IsGameOver)
                return;

            State.WinnerId = winnerId;
            State.IsGameOver = true;
            State.Phase = GamePhase.GameOver;

            Emit(GameEvent.MatchEnded(winnerId));
        }

        private void AddDefaultPlayers()
        {
            State.AddPlayer(
                new PlayerState(
                    0,
                    "You",
                    true));

            State.AddPlayer(
                new PlayerState(
                    1,
                    "AI-1",
                    false));

            State.AddPlayer(
                new PlayerState(
                    2,
                    "AI-2",
                    false));

            State.AddPlayer(
                new PlayerState(
                    3,
                    "AI-3",
                    false));
        }

        private void Emit(GameEvent gameEvent)
        {
            State.AddEvent(gameEvent);

            EventProduced?.Invoke(gameEvent);
        }

        private void Reject(
            int playerId,
            string message)
        {
            Emit(
                GameEvent.ActionRejected(
                    playerId,
                    message));
        }
    }
}
