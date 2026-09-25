namespace CardGame.Core.Game
{
    /// <summary>
    /// Represents the current phase of a card game.
    /// </summary>
    public enum GamePhase
    {
        /// <summary>
        /// Game is being initialized.
        /// </summary>
        Setup = 0,

        /// <summary>
        /// A new turn is starting.
        /// </summary>
        TurnStart = 1,

        /// <summary>
        /// The active player can perform actions.
        /// </summary>
        Turn = 2,

        /// <summary>
        /// The game has ended.
        /// </summary>
        GameOver = 3
    }
}
