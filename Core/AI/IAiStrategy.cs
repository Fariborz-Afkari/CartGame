using CardGame.Core.Game;

namespace CardGame.Core.AI
{
    public interface IAiStrategy
    {
        GameActionType ChooseAction(GameState state, int playerId);
    }
}
