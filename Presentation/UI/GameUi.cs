using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardGame.Presentation.Game;

namespace CardGame.Presentation.UI
{
    public sealed class GameUi : MonoBehaviour
    {
        [Header("Presenter")]
        [SerializeField]
        private GamePresenter _presenter;

        [Header("Hand")]
        [SerializeField]
        private Transform _handContainer;

        [SerializeField]
        private Button _cardButtonPrefab;

        [Header("Actions")]
        [SerializeField]
        private Button _startButton;

        [SerializeField]
        private Button _endTurnButton;

        [Header("Status")]
        [SerializeField]
        private TMP_Text _statusText;

        [Header("Target Selection")]
        [SerializeField]
        private GameObject _targetPanel;

        [SerializeField]
        private Transform _targetContainer;

        [SerializeField]
        private Button _targetButtonPrefab;

        private int? _selectedCardId;
        [SerializeField] private TMP_Text _coinsText;
        [SerializeField] private Button _buyCoinsButton;

        private void Awake()
        {
            if (_presenter == null)
                _presenter = new GamePresenter();

            _startButton.onClick.AddListener(
                OnDrawClicked);

            _endTurnButton.onClick.AddListener(
                OnEndTurnClicked);
            if (_buyCoinsButton != null)
            {
                _buyCoinsButton.onClick.AddListener(OnBuyCoinsClicked);
            }
            HideTargetPanel();
        }

        private void OnEnable()
        {
            if (_presenter != null)
                _presenter.Changed += Refresh;
        }

        private void Start()
        {
            StartGame();
        }

        private void OnDestroy()
        {
            if (_presenter != null)
            {
                _presenter.Changed -= Refresh;
                _presenter.Dispose();
            }

            _startButton.onClick.RemoveListener(
                OnDrawClicked);

            _endTurnButton.onClick.RemoveListener(
                OnEndTurnClicked);
            if (_buyCoinsButton != null)
            {
                _buyCoinsButton.onClick.RemoveListener(OnBuyCoinsClicked);
            }
        }

        // --------------------------------------------------
        // Match
        // --------------------------------------------------

        public void StartGame()
        {
            _presenter.StartMatch();
        }

        // --------------------------------------------------
        // Refresh
        // --------------------------------------------------

        private void Refresh()
        {
            RefreshHand();
            RefreshStatus();
            RefreshActions();
            RefreshEconomy();
        }

        private void RefreshHand()
        {
            ClearContainer(
                _handContainer);

            var cards =
                _presenter.GetPlayerHand();

            for (int i = 0;
                 i < cards.Count;
                 i++)
            {
                CreateCardButton(cards[i]);
            }
        }

        private void RefreshStatus()
        {
            if (_statusText == null)
                return;

            _statusText.text =
                _presenter.Status;
        }

        private void RefreshActions()
        {
            bool canPlay =
                _presenter.IsPlayerTurn &&
                !_presenter.IsGameOver;

            _startButton.interactable =
                canPlay;

            _endTurnButton.interactable =
                canPlay;
        }
        private void RefreshEconomy()
        {
            if (_coinsText != null)
            {
                _coinsText.text = "Coins: " + _presenter.Coins;
            }

            if (_startButton != null)
            {
                _startButton.interactable = _presenter.CanStartMatch;
            }

            if (_buyCoinsButton != null)
            {
                _buyCoinsButton.interactable = true;
            }
        }

        // --------------------------------------------------
        // Hand
        // --------------------------------------------------

        private void CreateCardButton(
            GamePresenter.CardViewData card)
        {
            Button button =
                Instantiate(
                    _cardButtonPrefab,
                    _handContainer);

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>();

            if (text != null)
            {
                text.text =
                    card.Value > 0
                        ? $"{card.Name} ({card.Value})"
                        : card.Name;
            }

            int cardId =
                card.InstanceId;

            button.onClick.AddListener(
                () => OnCardClicked(cardId));
        }

        private void OnCardClicked(
            int cardId)
        {
            if (!_presenter.IsPlayerTurn)
                return;

            GamePresenter.CardInteraction interaction =
                _presenter.GetCardInteraction(
                    cardId);

            if (interaction.RequiresTarget)
            {
                _selectedCardId =
                    interaction.CardId;

                ShowTargetPanel(
                    interaction.Targets);

                return;
            }

            PlaySelectedCard();
        }

        // --------------------------------------------------
        // Target selection
        // --------------------------------------------------

        private void ShowTargetPanel(
            System.Collections.Generic.IReadOnlyList<
                GamePresenter.TargetViewData> targets)
        {
            ClearContainer(
                _targetContainer);

            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                CreateTargetButton(
                    targets[i]);
            }

            _targetPanel.SetActive(true);
        }

        private void CreateTargetButton(
            GamePresenter.TargetViewData target)
        {
            Button button =
                Instantiate(
                    _targetButtonPrefab,
                    _targetContainer);

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>();

            if (text != null)
                text.text = target.PlayerName;

            int playerId =
                target.PlayerId;

            button.onClick.AddListener(
                () => OnTargetSelected(playerId));
        }

        private void OnTargetSelected(
            int targetPlayerId)
        {
            if (!_selectedCardId.HasValue)
                return;

            PlaySelectedCard(
                targetPlayerId);
        }

        private void HideTargetPanel()
        {
            _selectedCardId = null;

            if (_targetPanel != null)
                _targetPanel.SetActive(false);
        }

        // --------------------------------------------------
        // Card play
        // --------------------------------------------------

        private void PlaySelectedCard(
            int? targetPlayerId = null)
        {
            if (!_selectedCardId.HasValue)
                return;

            int cardId =
                _selectedCardId.Value;

            HideTargetPanel();

            _presenter.PlayCard(
                cardId,
                targetPlayerId);
        }

        // --------------------------------------------------
        // Actions
        // --------------------------------------------------

        private void OnDrawClicked()
        {
            if (!_presenter.IsPlayerTurn)
                return;

            _presenter.DrawCard();
        }

        private void OnEndTurnClicked()
        {
            if (!_presenter.IsPlayerTurn)
                return;

            _presenter.EndTurn();
        }

        private void OnBuyCoinsClicked()
        {
            _presenter.BuyCoins();
        }
        // --------------------------------------------------
        // Utility
        // --------------------------------------------------

        private void ClearContainer(
            Transform container)
        {
            if (container == null)
                return;

            for (int i = container.childCount - 1;
                 i >= 0;
                 i--)
            {
                Destroy(
                    container.GetChild(i).gameObject);
            }
        }
    }
}
