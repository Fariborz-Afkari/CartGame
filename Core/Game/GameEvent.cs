namespace CardGame.Core.Game
{
    /// <summary>
    /// Defines the types of events that can be produced by the game engine.
    ///
    /// A GameEvent represents something that has already happened.
    /// It is not a command and must not contain game logic.
    /// </summary>
    public enum GameEventType
    {
        /// <summary>
        /// A new match has started.
        /// </summary>
        MatchStarted = 0,

        /// <summary>
        /// A player's turn has started.
        /// </summary>
        TurnStarted = 1,

        /// <summary>
        /// A player has drawn a card.
        /// </summary>
        CardDrawn = 2,

        /// <summary>
        /// A player has played a card.
        /// </summary>
        CardPlayed = 3,

        /// <summary>
        /// A player's turn has ended.
        /// </summary>
        TurnEnded = 4,

        /// <summary>
        /// The match has ended.
        /// </summary>
        MatchEnded = 5,

        /// <summary>
        /// A coin was consumed by a player.
        /// </summary>
        CoinConsumed = 6,

        /// <summary>
        /// An action was rejected by the game engine.
        /// </summary>
        ActionRejected = 7
    }

    /// <summary>
    /// Represents an event produced by the game engine.
    ///
    /// GameEvent is immutable and contains information about
    /// an event that has already happened.
    ///
    /// Events can be consumed by Presentation, UI, audio,
    /// animation, logging or other external systems.
    /// </summary>
    public readonly struct GameEvent
    {
        /// <summary>
        /// Type of the event.
        /// </summary>
        public GameEventType Type { get; }

        /// <summary>
        /// Identifier of the player associated with the event.
        ///
        /// A value of -1 means that the event is not associated
        /// with a specific player.
        /// </summary>
        public int PlayerId { get; }

        /// <summary>
        /// Optional identifier of the card associated with the event.
        /// </summary>
        public int? CardId { get; }

        /// <summary>
        /// Optional identifier of the target player associated
        /// with the event.
        /// </summary>
        public int? TargetPlayerId { get; }

        /// <summary>
        /// Human-readable description of the event.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Creates a new game event.
        /// </summary>
        public GameEvent(
            GameEventType type,
            int playerId = -1,
            int? cardId = null,
            int? targetPlayerId = null,
            string message = null)
        {
            Type = type;
            PlayerId = playerId;
            CardId = cardId;
            TargetPlayerId = targetPlayerId;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// Creates a MatchStarted event.
        /// </summary>
        public static GameEvent MatchStarted(
            string message = "Match started.")
        {
            return new GameEvent(
                GameEventType.MatchStarted,
                message: message);
        }

        /// <summary>
        /// Creates a TurnStarted event.
        /// </summary>
        public static GameEvent TurnStarted(
            int playerId,
            string message = null)
        {
            return new GameEvent(
                GameEventType.TurnStarted,
                playerId: playerId,
                message: message ?? "Turn started.");
        }

        /// <summary>
        /// Creates a CardDrawn event.
        /// </summary>
        public static GameEvent CardDrawn(
            int playerId,
            int cardId,
            string message = null)
        {
            return new GameEvent(
                GameEventType.CardDrawn,
                playerId: playerId,
                cardId: cardId,
                message: message ?? "Card drawn.");
        }

        /// <summary>
        /// Creates a CardPlayed event.
        /// </summary>
        public static GameEvent CardPlayed(
            int playerId,
            int cardId,
            int? targetPlayerId = null,
            string message = null)
        {
            return new GameEvent(
                GameEventType.CardPlayed,
                playerId: playerId,
                cardId: cardId,
                targetPlayerId: targetPlayerId,
                message: message ?? "Card played.");
        }

        /// <summary>
        /// Creates a TurnEnded event.
        /// </summary>
        public static GameEvent TurnEnded(
            int playerId,
            string message = null)
        {
            return new GameEvent(
                GameEventType.TurnEnded,
                playerId: playerId,
                message: message ?? "Turn ended.");
        }

        /// <summary>
        /// Creates a MatchEnded event.
        /// </summary>
        public static GameEvent MatchEnded(
            int? winnerPlayerId = null,
            string message = null)
        {
            return new GameEvent(
                GameEventType.MatchEnded,
                playerId: winnerPlayerId ?? -1,
                message: message ?? "Match ended.");
        }

        /// <summary>
        /// Creates an ActionRejected event.
        /// </summary>
        public static GameEvent ActionRejected(
            int playerId,
            string message)
        {
            return new GameEvent(
                GameEventType.ActionRejected,
                playerId: playerId,
                message: message);
        }
        /// <summary>
        /// Creates a CoinConsumed event.
        /// </summary>
        public static GameEvent CoinConsumed(
            int playerId,
            string message = null)
        {
            return new GameEvent(
                GameEventType.CoinConsumed,
                playerId: playerId,
                message: message ?? "Coin consumed.");
        }
        public override string ToString()
        {
            return Message;
        }
    }
}
