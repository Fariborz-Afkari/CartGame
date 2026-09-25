using CardGame.Core.Game;

namespace CardGame.Core.AI
{
    public sealed class BasicAiStrategy : IAiStrategy
    {
        public GameActionType ChooseAction(GameState state, int playerId)
        {
            var player = GameRules.Find(state, playerId);
            if (player != null && player.Health <= 5) return GameActionType.Heal;
            return GameActionType.Attack;
        }
    }
}
