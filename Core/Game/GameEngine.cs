using CardGame.Core.Players;

namespace CardGame.Core.Game
{
    public sealed class GameEngine
    {
        public GameState State { get; } = new GameState();

        public void StartMatch()
        {
            State.Reset();
            State.AddPlayer(new PlayerState(0, "You", true));
            State.AddPlayer(new PlayerState(1, "AI-1", false));
            State.AddPlayer(new PlayerState(2, "AI-2", false));
            State.AddPlayer(new PlayerState(3, "AI-3", false));
            State.Phase = GamePhase.PlayerTurn;
            State.CurrentPlayerId = 0;
            State.TurnNumber = 1;
            State.AddLog("Match started. Your turn.");
        }

        public bool TryPlayerAction(GameActionType action)
        {
            if (State.Phase != GamePhase.PlayerTurn || State.CurrentPlayerId != 0) return false;
            Apply(new GameAction(action, 0));
            if (CheckGameOver()) return true;
            RunAiTurns();
            return true;
        }

        private void RunAiTurns()
        {
            State.Phase = GamePhase.AiTurn;
            for (int i = 1; i < State.Players.Count; i++)
            {
                var ai = State.Players[i];
                if (ai.Health <= 0) continue;
                GameActionType action = ai.Health <= 5 ? GameActionType.Heal : (State.TurnNumber + i) % 3 == 0 ? GameActionType.Guard : GameActionType.Attack;
                Apply(new GameAction(action, ai.Id));
                if (CheckGameOver()) return;
            }
            State.TurnNumber++;
            State.CurrentPlayerId = 0;
            State.Phase = GamePhase.PlayerTurn;
            State.AddLog("Your turn.");
        }

        private void Apply(GameAction action)
        {
            GameRules.Resolve(State, action);
        }

        private bool CheckGameOver()
        {
            var human = GameRules.Find(State, 0);
            if (human == null || human.Health <= 0)
            {
                State.WinnerId = -1;
                State.Phase = GamePhase.GameOver;
                State.AddLog("You lost the match.");
                return true;
            }

            if (GameRules.CountLiving(State) == 1)
            {
                State.WinnerId = 0;
                State.Phase = GamePhase.GameOver;
                State.AddLog("You won the match.");
                return true;
            }
            return false;
        }
    }
}
