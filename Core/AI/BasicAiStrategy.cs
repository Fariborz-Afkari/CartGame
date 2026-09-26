using CardGame.Core.Cards;
using CardGame.Core.Game;
using CardGame.Core.Players;

namespace CardGame.Core.AI
{
    public sealed class BasicAiStrategy : IAIStrategy
    {
        private readonly int _healThreshold;

        public BasicAiStrategy(
            int healThreshold = 6)
        {
            _healThreshold = healThreshold;
        }

        public GameAction DecideAction(
            GameState state,
            PlayerState actor)
        {
            if (state == null ||
                actor == null ||
                actor.Health <= 0)
            {
                return null;
            }

            // اگر Health پایین است و Heal داریم،
            // ابتدا Heal را انتخاب می‌کنیم.
            Card heal =
                FindCard(
                    actor,
                    CardEffect.Heal);

            if (heal != null &&
                actor.Health <= _healThreshold)
            {
                return GameAction.PlayCard(
                    actor.Id,
                    heal.InstanceId);
            }

            // در حالت عادی Attack را ترجیح می‌دهیم.
            Card attack =
                FindCard(
                    actor,
                    CardEffect.Damage);

            if (attack != null)
            {
                PlayerState target =
                    FindLivingOpponent(
                        state,
                        actor.Id);

                if (target != null)
                {
                    return GameAction.PlayCard(
                        actor.Id,
                        attack.InstanceId,
                        target.Id);
                }
            }

            // اگر Health نسبتاً پایین است و Guard داریم،
            // از Guard استفاده می‌کنیم.
            Card guard =
                FindCard(
                    actor,
                    CardEffect.Guard);

            if (guard != null &&
                actor.Health <= _healThreshold + 2)
            {
                return GameAction.PlayCard(
                    actor.Id,
                    guard.InstanceId);
            }

            // اگر کارت قابل بازی نداریم، Draw.
            if (state.Deck != null &&
                !state.Deck.IsEmpty)
            {
                return GameAction.DrawCard(
                    actor.Id);
            }

            // آخرین گزینه: پایان نوبت.
            return GameAction.EndTurn(
                actor.Id);
        }

        private Card FindCard(
            PlayerState player,
            CardEffect effect)
        {
            for (int i = 0;
                 i < player.Hand.Count;
                 i++)
            {
                Card card =
                    player.Hand.Cards[i];

                if (card.Definition != null &&
                    card.Definition.Effect == effect)
                {
                    return card;
                }
            }

            return null;
        }

        private PlayerState FindLivingOpponent(
            GameState state,
            int playerId)
        {
            for (int i = 0;
                 i < state.Players.Count;
                 i++)
            {
                PlayerState player =
                    state.Players[i];

                if (player.Id != playerId &&
                    player.Health > 0)
                {
                    return player;
                }
            }

            return null;
        }
    }
}
