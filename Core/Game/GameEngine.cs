using System;
using CardGame.Core.Players;
using CardGame.Core.Cards;

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
    /// GameEngine does not contain card-specific rules.
    /// </summary>
    public sealed class GameEngine
    {
        private readonly int _startingPlayerId;

        public GameState State { get; }

        /// <summary>
        /// Raised whenever a new game event is produced.
        /// </summary>
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
        /// Starts a new match using the default sample players.
        ///
        /// Player creation is currently kept here because the project
        /// does not yet have a separate match/player setup layer.
        /// </summary>
        public void StartMatch()
        {
            State.Reset();

            AddDefaultPlayers();

            if (State.FindPlayer(_startingPlayerId) == null)
            {
                throw new InvalidOperationException(
                    $"Starting player {_startingPlayerId} does not exist.");
            }

            State.Phase = GamePhase.TurnStart;
            State.IsGameOver = false;
            State.CurrentPlayerId = _startingPlayerId;
            State.TurnNumber = 1;
            State.WinnerId = null;

            Emit(GameEvent.MatchStarted());

            StartTurn(_startingPlayerId);
        }

        /// <summary>
        /// Submits an action requested by a player or AI.
        ///
        /// Returns true when the action was accepted and processed.
        /// Returns false when the action was rejected.
        /// </summary>
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

        /// <summary>
        /// Handles a draw-card request.
        ///
        /// The current Core model does not yet contain a Deck/Hand system,
        /// therefore no card can be drawn yet.
        /// </summary>
        private bool ResolveDrawCard(GameAction action) { 
            PlayerState actor = State.FindPlayer(action.PlayerId); 
            if (actor == null) { 
                Reject(action.PlayerId, "The player does not exist."); 
                return false; 
            } 
            int handCountBefore = actor.Hand.Count; 
            if (!GameRules.Resolve(State, action)) {
                Reject(action.PlayerId, "The card cannot be drawn."); 
                return false; 
            } /* * GameRules has already moved the real card: * 
               * * State.Deck -> actor.Hand * 
               * * The hand contains the newly drawn card at the end. */ 
            if (actor.Hand.Count <= handCountBefore) { 
                Reject(action.PlayerId, "The card draw did not produce a card."); 
                return false; 
            } 
            Card drawnCard = actor.Hand.Cards[actor.Hand.Count - 1]; 
            Emit(GameEvent.CardDrawn(action.PlayerId, drawnCard.InstanceId)); 
            return true; 
        }

        /// <summary>
        /// Handles a play-card request.
        /// </summary>
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

        /// <summary>
        /// Ends the current player's turn and advances to the next
        /// living player.
        /// </summary>
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

        /// <summary>
        /// Starts a player's turn.
        /// </summary>
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

        /// <summary>
        /// Determines whether the specified player is allowed to act.
        /// </summary>
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

        /// <summary>
        /// Finds the next living player in circular order.
        /// </summary>
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

        /// <summary>
        /// Checks whether the match has reached a terminal state.
        /// </summary>
        private bool CheckGameOver()
        {
            if (State.IsGameOver)
                return true;

            PlayerState human =
                State.FindPlayer(0);

            /*
             * The current sample game has player 0 as the human player.
             * This will later be replaced by a more generic win-condition
             * system when game-specific rules are introduced.
             */
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

        /// <summary>
        /// Ends the match and publishes the final event.
        /// </summary>
        private void EndMatch(int? winnerId)
        {
            if (State.IsGameOver)
                return;

            State.WinnerId = winnerId;
            State.IsGameOver = true;
            State.Phase = GamePhase.GameOver;

            Emit(GameEvent.MatchEnded(winnerId));
        }

        /// <summary>
        /// Adds the current sample game's players.
        ///
        /// This is temporary infrastructure. Later this should be moved
        /// to a match configuration / player factory.
        /// </summary>
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

        /// <summary>
        /// Records an event in GameState and notifies listeners.
        /// </summary>
        private void Emit(GameEvent gameEvent)
        {
            State.AddEvent(gameEvent);

            EventProduced?.Invoke(gameEvent);
        }

        /// <summary>
        /// Creates and publishes an ActionRejected event.
        /// </summary>
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
