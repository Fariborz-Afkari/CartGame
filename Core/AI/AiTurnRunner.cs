using CardGame.Core.Game;
using CardGame.Core.Players;

namespace CardGame.Core.AI
{
    public sealed class AiTurnRunner
    {
        private readonly GameEngine _engine;
        private readonly IAIStrategy _strategy;

        public AiTurnRunner(
            GameEngine engine,
            IAIStrategy strategy)
        {
            _engine = engine;
            _strategy = strategy;
        }

        public bool RunCurrentTurn()
        {
            if (_engine.State.IsGameOver)
                return false;

            int playerId =
                _engine.State.CurrentPlayerId;

            PlayerState player =
                _engine.State.FindPlayer(playerId);

            if (player == null ||
                player.IsHuman ||
                player.Health <= 0)
            {
                return false;
            }

            return RunTurn(player);
        }

        private bool RunTurn(
            PlayerState player)
        {
            // حداکثر تعداد Action برای جلوگیری
            // از loop ناخواسته.
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
                    _strategy.DecideAction(
                        _engine.State,
                        player);

                if (action == null)
                    return false;

                bool accepted =
                    _engine.SubmitAction(action);

                if (!accepted)
                    return false;

                // EndTurn باعث انتقال نوبت می‌شود.
                if (action.Type ==
                    GameActionType.EndTurn)
                {
                    return true;
                }
            }

            // اگر AI در maxActions هنوز EndTurn نکرد،
            // برای جلوگیری از گیر کردن بازی نوبت را می‌بندیم.
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