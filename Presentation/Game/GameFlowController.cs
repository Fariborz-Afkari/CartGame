using CardGame.Core.AI;
using CardGame.Core.Game;
using CardGame.Core.Players;

namespace CardGame.Presentation.Game
{
    /// <summary>
    /// Coordinates turn flow between the presenter and the game engine.
    /// The presenter exposes UI-facing operations; this class handles
    /// automatic AI turns after the human player ends a turn.
    /// </summary>
    public sealed class GameFlowController
    {
        private readonly GameEngine _engine;
        private readonly IAIStrategy _aiStrategy;

        public GameFlowController(
            GameEngine engine,
            IAIStrategy aiStrategy)
        {
            _engine = engine;
            _aiStrategy = aiStrategy;
        }

        public void RunAiTurns()
        {
            while (!_engine.State.IsGameOver)
            {
                PlayerState current =
                    _engine.State.FindPlayer(
                        _engine.State.CurrentPlayerId);

                if (current == null ||
                    current.IsHuman ||
                    current.Health <= 0)
                {
                    return;
                }

                if (!RunAiTurn(current))
                    return;
            }
        }

        private bool RunAiTurn(
            PlayerState player)
        {
            const int maxActions = 3;

            for (int i = 0;
                 i < maxActions;
                 i++)
            {
                if (_engine.State.IsGameOver)
                    return true;

                if (_engine.State.CurrentPlayerId != player.Id)
                    return true;

                GameAction action =
                    _aiStrategy.DecideAction(
                        _engine.State,
                        player);

                if (action == null)
                    return false;

                if (!_engine.SubmitAction(action))
                    return false;

                if (action.Type ==
                    GameActionType.EndTurn)
                {
                    return true;
                }
            }

            // Safety fallback: an AI must never block the match by
            // failing to end its turn after the action limit.
            if (!_engine.State.IsGameOver &&
                _engine.State.CurrentPlayerId == player.Id)
            {
                _engine.SubmitAction(
                    GameAction.EndTurn(player.Id));
            }

            return true;
        }
    }
}
