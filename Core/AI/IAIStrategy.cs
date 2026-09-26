using CardGame.Core.Game;
using CardGame.Core.Players;

namespace CardGame.Core.AI
{
    public interface IAIStrategy
    {
        GameAction DecideAction(
            GameState state,
            PlayerState actor);
    }
}
