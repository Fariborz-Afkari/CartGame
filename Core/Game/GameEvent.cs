namespace CardGame.Core.Game
{
    /// <summary>
    /// Defines the types of events that can occur during a game.
    ///
    /// Game events describe something that has already happened.
    /// They are not commands and must not contain game logic.
    /// </summary>
    public enum GameEventType
    {
        MatchStarted = 0,
        ActionResolved = 1,
        MatchEnded = 2,
        CoinConsumed = 3
    }

    /// <summary>
    /// Represents an event produced by the game engine.
    ///
    /// GameEvent is immutable and contains only information
    /// that can be consumed by other layers such as Presentation,
    /// UI, logging or future audio/visual systems.
    /// </summary>
    public readonly struct GameEvent
    {
        /// <summary>
        /// Type of the event.
        /// </summary>
        public GameEventType Type { get; }

        /// <summary>
        /// Human-readable message associated with the event.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Creates a new game event.
        /// </summary>
        public GameEvent(
            GameEventType type,
            string message)
        {
            Type = type;
            Message = message;
        }

        /// <summary>
        /// Creates a MatchStarted event.
        /// </summary>
        public static GameEvent MatchStarted(string message = "Match started.")
        {
            return new GameEvent(
                GameEventType.MatchStarted,
                message);
        }

        /// <summary>
        /// Creates an ActionResolved event.
        /// </summary>
        public static GameEvent ActionResolved(string message)
        {
            return new GameEvent(
                GameEventType.ActionResolved,
                message);
        }

        /// <summary>
        /// Creates a MatchEnded event.
        /// </summary>
        public static GameEvent MatchEnded(string message = "Match ended.")
        {
            return new GameEvent(
                GameEventType.MatchEnded,
                message);
        }

        /// <summary>
        /// Creates a CoinConsumed event.
        /// </summary>
        public static GameEvent CoinConsumed(string message = "Coin consumed.")
        {
            return new GameEvent(
                GameEventType.CoinConsumed,
                message);
        }
    }
}
