using System.Collections.Generic;

namespace CardGame.Presentation.Game
{
    public sealed class GameViewModel
    {
        public int Coins { get; set; }

        public bool CanStartMatch { get; set; }

        public bool IsPlayerTurn { get; set; }

        public bool IsGameOver { get; set; }

        public string CurrentPlayerName { get; set; }

        public string Status { get; set; }

        public IReadOnlyList<GamePresenter.CardViewData> Hand { get; set; }
    }
}