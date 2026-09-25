namespace CardGame.Core.Game
{
    /// <summary>
    /// Represents an action requested by a player or an AI.
    ///
    /// GameAction contains intent only.
    /// Validation and execution are handled by the game engine and rules.
    /// </summary>
    public sealed class GameAction
    {
        /// <summary>
        /// Type of action being requested.
        /// </summary>
        public GameActionType Type { get; }

        /// <summary>
        /// Identifier of the player requesting the action.
        /// </summary>
        public int PlayerId { get; }

        /// <summary>
        /// Optional identifier of a card involved in the action.
        /// </summary>
        public int? CardId { get; }

        /// <summary>
        /// Optional target player identifier.
        /// </summary>
        public int? TargetPlayerId { get; }

        private GameAction(
            GameActionType type,
            int playerId,
            int? cardId = null,
            int? targetPlayerId = null)
        {
            Type = type;
            PlayerId = playerId;
            CardId = cardId;
            TargetPlayerId = targetPlayerId;
        }

        public static GameAction DrawCard(int playerId)
        {
            return new GameAction(
                GameActionType.DrawCard,
                playerId);
        }

        public static GameAction PlayCard(
            int playerId,
            int cardId,
            int? targetPlayerId = null)
        {
            return new GameAction(
                GameActionType.PlayCard,
                playerId,
                cardId,
                targetPlayerId);
        }

        public static GameAction EndTurn(int playerId)
        {
            return new GameAction(
                GameActionType.EndTurn,
                playerId);
        }
    }

    /// <summary>
    /// Defines the types of actions supported by the core game layer.
    /// </summary>
    public enum GameActionType
    {
        DrawCard = 0,
        PlayCard = 1,
        EndTurn = 2
    }
}
