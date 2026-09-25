namespace CardGame.Core.Game
{
    /// <summary>
    /// Represents the runtime state of a card game.
    ///
    /// This class stores state only.
    /// Game rules, AI, Unity and presentation logic
    /// must not be implemented here.
    /// </summary>
    public sealed class GameState
    {
        /// <summary>
        /// Unique identifier for this game instance.
        /// </summary>
        public string GameId { get; }
        /// <summary>
        /// Current phase of the game.
        /// </summary>
        public GamePhase Phase { get; internal set; }
        /// <summary>
        /// Indicates whether the game has finished.
        /// </summary>
        public bool IsGameOver { get; internal set; }

        /// <summary>
        /// Identifier of the player whose turn is currently active.
        /// </summary>
        public int CurrentPlayerId { get; internal set; }

        /// <summary>
        /// Number of turns that have been completed.
        /// </summary>
        public int TurnNumber { get; internal set; }

        public GameState(string gameId,int startingPlayerId)
        {
            GameId = gameId;
            IsGameOver = false;
            Phase = GamePhase.Setup;
            CurrentPlayerId = startingPlayerId;
            TurnNumber = 0;
        }
    }
}
