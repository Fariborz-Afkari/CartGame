using UnityEngine;
using CardGame.Core.Game;
using CardGame.Core.Players;
using CardGame.Presentation.Game;

namespace CardGame.Presentation.UI
{
    public sealed class GameUi : MonoBehaviour
    {
        private GamePresenter presenter;
        private Vector2 scroll;

        public void Initialize(GamePresenter value) => presenter = value;

        private void OnGUI()
        {
            if (presenter == null) return;
            GUI.skin.label.fontSize = 18;
            GUI.skin.button.fontSize = 18;
            GUI.skin.box.fontSize = 18;

            GUILayout.BeginArea(new Rect(24, 18, Screen.width - 48, Screen.height - 36));
            GUILayout.Label("Reusable 2D Card Game — Offline Skeleton", GUI.skin.box);
            GUILayout.Label("Coins: " + presenter.Economy.Coins + "    Status: " + presenter.Status);

            DrawPlayers();

            GUILayout.Space(10);
            if (presenter.Engine.State.Phase == GamePhase.Lobby || presenter.Engine.State.Phase == GamePhase.GameOver)
            {
                string label = presenter.Engine.State.Phase == GamePhase.GameOver ? "Play Again (1 Coin)" : "Start Match (1 Coin)";
                if (GUILayout.Button(label, GUILayout.Height(48))) presenter.StartMatch();
            }
            else
            {
                GUILayout.Label("Your hand:");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Attack\n3 damage", GUILayout.Height(70))) presenter.PlayerAction(GameActionType.Attack);
                if (GUILayout.Button("Heal\n2 HP", GUILayout.Height(70))) presenter.PlayerAction(GameActionType.Heal);
                if (GUILayout.Button("Guard\n50% damage", GUILayout.Height(70))) presenter.PlayerAction(GameActionType.Guard);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8);
            if (GUILayout.Button("Mock IAP — Buy 10 Coins", GUILayout.Height(42))) presenter.BuyCoins();
            GUILayout.Label("Real Unity IAP can later implement IIapService without changing Core game rules.");

            GUILayout.Label("Recent events:");
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(150));
            foreach (var line in presenter.Engine.State.Log) GUILayout.Label("• " + line);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawPlayers()
        {
            if (presenter.Engine.State.Players.Count == 0)
            {
                GUILayout.Label("No active match. Start one to play.");
                return;
            }

            for (int i = 0; i < presenter.Engine.State.Players.Count; i++)
            {
                PlayerState p = presenter.Engine.State.Players[i];
                string guard = p.Guarding ? " [Guarding]" : string.Empty;
                GUILayout.Label(p.Name + ": " + p.Health + "/" + p.MaxHealth + guard);
            }
        }
    }
}
