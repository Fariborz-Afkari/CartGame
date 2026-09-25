using UnityEngine;
using CardGame.Presentation.Game;
using CardGame.Presentation.UI;

namespace CardGame.Bootstrap
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private GamePresenter presenter;

        private void Awake()
        {
            presenter = new GamePresenter();
            var uiObject = new GameObject("RuntimeUI");
            var ui = uiObject.AddComponent<GameUi>();
            ui.Initialize(presenter);
        }
    }
}
