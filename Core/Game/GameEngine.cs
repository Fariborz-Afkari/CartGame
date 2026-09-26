using System;
using CardGame.Core.Players;

namespace CardGame.Core.Game
{
    /// <summary>
    /// Coordinates game flow.
    ///
    /// GameEngine receives GameAction instances and produces GameEvent instances.
    /// It does not expose the old action-based API.
    /// </summary>
    public sealed class GameEngine
    {
        public GameState State { get; }

        /// <summary>
        /// Raised whenever the engine produces a game event.
        /// </summary>
        public event Action<GameEvent> EventProduced;

        public GameEngine(string gameId, int startingPlayerId = 0)
        {
            State = new GameState(gameId, startingPlayerId);
        }

        /// <summary>
        /// Starts a new match.
        /// </summary>
        public void StartMatch()
        {
            State.Reset();

            State.AddPlayer(new PlayerState(0, "You", true));
            State.AddPlayer(new PlayerState(1, "AI-1", false));
            State.AddPlayer(new PlayerState(2, "AI-2", false));
            State.AddPlayer(new PlayerState(3, "AI-3", false));

            State.Phase = GamePhase.TurnStart;
            State.CurrentPlayerId = 0;
            State.TurnNumber = 1;

            Emit(GameEvent.MatchStarted("Match started."));
            StartTurn(0);
        }

        /// <summary>
        /// Submits an action to the game engine.
        ///
        /// The engine validates whether the action is allowed,
        /// resolves it through GameRules, and emits resulting events.
        /// </summary>
        public bool SubmitAction(GameAction action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (State.IsGameOver)
            {
                Emit(GameEvent.ActionRejected(
                    action.PlayerId,
                    "The match has already ended."));

                return false;
            }

            if (!IsPlayerTurn(action.PlayerId))
            {
                Emit(GameEvent.ActionRejected(
                    action.PlayerId,
                    "It is not this player's turn."));

                return false;
            }

            switch (action.Type)
            {
                case GameActionType.DrawCard:
                case GameActionType.PlayCard:
                    return ResolveGameplayAction(action);

                case GameActionType.EndTurn:
                    return EndTurn(action.PlayerId);

                default:
                    Emit(GameEvent.ActionRejected(
                        action.PlayerId,
                        "Unsupported game action."));

                    return false;
            }
        }

        private bool ResolveGameplayAction(GameAction action)
        {
            GameRules.Resolve(State, action);

            // GameRules is responsible for changing the state.
            // GameEngine is responsible for emitting domain events
            // and advancing the game flow.

            switch (action.Type)
            {
                case GameActionType.DrawCard:
                    if (!action.CardId.HasValue)
                    {
                        Emit(GameEvent.ActionRejected(
                            action.PlayerId,
                            "DrawCard did not produce a card id."));

                        return false;
                    }

                    Emit(GameEvent.CardDrawn(
                        action.PlayerId,
                        action.CardId.Value));

                    return true;

                case GameActionType.PlayCard:
                    if (!action.CardId.HasValue)
                    {
                        Emit(GameEvent.ActionRejected(
                            action.PlayerId,
                            "PlayCard requires a card id."));

                        return false;
                    }

                    Emit(GameEvent.CardPlayed(
                        action.PlayerId,
                        action.CardId.Value,
                        action.TargetPlayerId));

                    return true;

                default:
                    return false;
            }
        }

        private bool EndTurn(int playerId)
        {
            Emit(GameEvent.TurnEnded(playerId));

            if (CheckGameOver())
                return true;

            int nextPlayerId = FindNextLivingPlayer(playerId);

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
            State.Phase = GamePhase.Turn;

            Emit(GameEvent.TurnStarted(playerId));
        }

        private bool IsPlayerTurn(int playerId)
        {
            if (State.Phase != GamePhase.Turn)
                return false;

            if (State.CurrentPlayerId != playerId)
                return false;

            PlayerState player = State.FindPlayer(playerId);

            return player != null && player.Health > 0;
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

            for (int offset = 1; offset <= State.Players.Count; offset++)
            {
                int index =
                    (currentIndex + offset) % State.Players.Count;

                PlayerState player = State.Players[index];

                if (player.Health > 0)
                    return player.Id;
            }

            return -1;
        }

        private bool CheckGameOver()
        {
            PlayerState human = State.FindPlayer(0);

            if (human == null || human.Health <= 0)
            {
                EndMatch(null);
                return true;
            }

            int livingPlayers = 0;
            int lastLivingPlayerId = -1;

            for (int i = 0; i < State.Players.Count; i++)
            {
                PlayerState player = State.Players[i];

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
            State.WinnerId = winnerId;
            State.IsGameOver = true;
            State.Phase = GamePhase.GameOver;

            Emit(GameEvent.MatchEnded(winnerId));
        }

        private void Emit(GameEvent gameEvent)
        {
            State.AddEvent(gameEvent);
            EventProduced?.Invoke(gameEvent);
        }
    }
}
