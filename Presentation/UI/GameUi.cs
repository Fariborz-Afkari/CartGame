using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CardGame.Core.Cards;
using CardGame.Core.Game;

namespace CardGame.Presentation.Game
{
    public sealed class GameUi : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GamePresenter _presenter;

        [SerializeField]
        private Transform _handContainer;

        [SerializeField]
        private GameObject _cardButtonPrefab;

        [SerializeField]
        private Button _drawButton;

        [SerializeField]
        private Button _endTurnButton;

        [SerializeField]
        private Text _statusText;

        [Header("Target Selection")]
        [SerializeField]
        private GameObject _targetPanel;

        [SerializeField]
        private Transform _targetContainer;

        [SerializeField]
        private GameObject _targetButtonPrefab;

        private Card _selectedCard;

        private void Awake()
        {
            if (_presenter == null)
                _presenter = new GamePresenter();

            if (_drawButton != null)
                _drawButton.onClick.AddListener(OnDrawCardClicked);

            if (_endTurnButton != null)
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);

            HideTargetPanel();
        }

        public void StartGame()
        {
            _presenter.StartMatch();

            Refresh();
        }

        public void Refresh()
        {
            RefreshHand();
            RefreshStatus();
        }

        private void RefreshHand()
        {
            ClearContainer(_handContainer);

            IReadOnlyList<Card> hand =
                _presenter.GetPlayerHand();

            for (int i = 0; i < hand.Count; i++)
            {
                CreateCardButton(hand[i]);
            }
        }

        private void CreateCardButton(Card card)
        {
            if (_cardButtonPrefab == null ||
                _handContainer == null)
            {
                return;
            }

            GameObject buttonObject =
                Instantiate(
                    _cardButtonPrefab,
                    _handContainer);

            Button button =
                buttonObject.GetComponent<Button>();

            Text label =
                buttonObject.GetComponentInChildren<Text>();

            if (label != null)
            {
                label.text =
                    card.Definition.Name +
                    "\n" +
                    "Value: " +
                    card.Definition.Value;
            }

            if (button != null)
            {
                button.onClick.AddListener(
                    () => OnCardClicked(card));
            }
        }

        private void OnCardClicked(Card card)
        {
            if (card == null)
                return;

            _selectedCard = card;

            switch (card.Definition.Effect)
            {
                case CardEffect.Damage:
                    ShowTargetPanel();
                    break;

                case CardEffect.Heal:
                case CardEffect.Guard:
                    PlaySelectedCard(null);
                    break;

                default:
                    _selectedCard = null;
                    break;
            }
        }

        private void ShowTargetPanel()
        {
            if (_targetPanel != null)
                _targetPanel.SetActive(true);

            ClearContainer(_targetContainer);

            for (int i = 0;
                 i < _presenter.Engine.State.Players.Count;
                 i++)
            {
                var player =
                    _presenter.Engine.State.Players[i];

                if (player.Id == 0)
                    continue;

                if (player.Health <= 0)
                    continue;

                CreateTargetButton(
                    player.Id,
                    player.Name);
            }
        }

        private void CreateTargetButton(
            int playerId,
            string playerName)
        {
            if (_targetButtonPrefab == null ||
                _targetContainer == null)
            {
                return;
            }

            GameObject buttonObject =
                Instantiate(
                    _targetButtonPrefab,
                    _targetContainer);

            Button button =
                buttonObject.GetComponent<Button>();

            Text label =
                buttonObject.GetComponentInChildren<Text>();

            if (label != null)
                label.text = playerName;

            if (button != null)
            {
                button.onClick.AddListener(
                    () => OnTargetSelected(playerId));
            }
        }

        private void OnTargetSelected(
            int targetPlayerId)
        {
            PlaySelectedCard(targetPlayerId);
        }

        private void PlaySelectedCard(
            int? targetPlayerId)
        {
            if (_selectedCard == null)
                return;

            int cardId =
                _selectedCard.InstanceId;

            HideTargetPanel();

            bool success =
                _presenter.PlayCard(
                    cardId,
                    targetPlayerId);

            _selectedCard = null;

            if (success)
                Refresh();
            else
                RefreshStatus();
        }

        private void OnDrawCardClicked()
        {
            if (_presenter.DrawCard())
                Refresh();
            else
                RefreshStatus();
        }

        private void OnEndTurnClicked()
        {
            _presenter.EndTurn();

            Refresh();
        }

        private void RefreshStatus()
        {
            if (_statusText != null)
                _statusText.text =
                    _presenter.Status;
        }

        private void HideTargetPanel()
        {
            if (_targetPanel != null)
                _targetPanel.SetActive(false);
        }

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

        private void OnDestroy()
        {
            if (_drawButton != null)
                _drawButton.onClick.RemoveListener(
                    OnDrawCardClicked);

            if (_endTurnButton != null)
                _endTurnButton.onClick.RemoveListener(
                    OnEndTurnClicked);
        }
    }
}
