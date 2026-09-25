namespace CardGame.Core.Game
{
    public enum GameEventType
    {
        MatchStarted,
        ActionResolved,
        MatchEnded,
        CoinConsumed
    }

    public readonly struct GameEvent
    {
        public GameEventType Type { get; }
        public string Message { get; }

        public GameEvent(GameEventType type, string message)
        {
            Type = type;
            Message = message;
        }
    }
}
