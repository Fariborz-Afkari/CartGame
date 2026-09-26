using CardGame.Core.Cards;
using CardGame.Core.Players;

namespace CardGame.Core.Game
{
    public static class GameRules
    {
        public static bool Resolve(
            GameState state,
            GameAction action)
        {
            if (state == null || action == null)
                return false;

            if (state.IsGameOver)
                return false;

            PlayerState actor =
                Find(state, action.PlayerId);

            if (actor == null)
                return false;

            if (!IsAlive(actor))
                return false;

            if (state.CurrentPlayerId != actor.Id)
                return false;

            switch (action.Type)
            {
                case GameActionType.DrawCard:
                    return ResolveDrawCard(
                        state,
                        action);

                case GameActionType.PlayCard:
                    return ResolvePlayCard(
                        state,
                        action);

                case GameActionType.EndTurn:
                    return CanEndTurn(
                        state,
                        action);

                default:
                    return false;
            }
        }

        public static bool CanResolve(
            GameState state,
            GameAction action)
        {
            if (state == null || action == null)
                return false;

            if (state.IsGameOver)
                return false;

            PlayerState actor =
                Find(state, action.PlayerId);

            if (actor == null)
                return false;

            if (!IsAlive(actor))
                return false;

            if (state.CurrentPlayerId != actor.Id)
                return false;

            switch (action.Type)
            {
                case GameActionType.DrawCard:
                    return CanDrawCard(state);

                case GameActionType.PlayCard:
                    return CanPlayCard(
                        state,
                        action);

                case GameActionType.EndTurn:
                    return CanEndTurn(
                        state,
                        action);

                default:
                    return false;
            }
        }

        private static bool ResolveDrawCard(
            GameState state,
            GameAction action)
        {
            if (!CanDrawCard(state))
                return false;

            PlayerState actor =
                Find(state, action.PlayerId);

            if (actor == null)
                return false;

            Card card =
                state.Deck.Draw();

            if (card == null)
                return false;

            actor.Hand.Add(card);

            return true;
        }

        private static bool ResolvePlayCard(
            GameState state,
            GameAction action)
        {
            if (!CanPlayCard(state, action))
                return false;

            PlayerState actor =
                Find(state, action.PlayerId);

            if (actor == null)
                return false;

            Card card =
                actor.Hand.Find(
                    action.CardId.Value);

            if (card == null)
                return false;

            if (!ResolveCardEffect(
                    state,
                    actor,
                    card,
                    action.TargetPlayerId))
            {
                return false;
            }

            return actor.Hand.Remove(
                card.InstanceId);
        }

        private static bool ResolveCardEffect(
            GameState state,
            PlayerState actor,
            Card card,
            int? targetPlayerId)
        {
            if (card == null ||
                card.Definition == null)
            {
                return false;
            }

            switch (card.Definition.Effect)
            {
                case CardEffect.Damage:
                    {
                        if (card.Definition.Value <= 0)
                            return false;

                        PlayerState target;

                        if (targetPlayerId.HasValue)
                        {
                            target =
                                Find(
                                    state,
                                    targetPlayerId.Value);

                            if (!IsValidOpponent(
                                    actor,
                                    target))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            target =
                                FindNextLivingOpponent(
                                    state,
                                    actor.Id);

                            if (target == null)
                                return false;
                        }

                        target.Damage(
                            card.Definition.Value);

                        return true;
                    }

                case CardEffect.Heal:
                    {
                        if (card.Definition.Value <= 0)
                            return false;

                        actor.Heal(
                            card.Definition.Value);

                        return true;
                    }

                case CardEffect.Guard:
                    {
                        actor.Guarding = true;
                        return true;
                    }

                default:
                    return false;
            }
        }

        private static bool CanDrawCard(
            GameState state)
        {
            return state.Deck != null &&
                   !state.Deck.IsEmpty;
        }

        private static bool CanPlayCard(
            GameState state,
            GameAction action)
        {
            if (!action.CardId.HasValue)
                return false;

            PlayerState actor =
                Find(state, action.PlayerId);

            if (actor == null)
                return false;

            Card card =
                actor.Hand.Find(
                    action.CardId.Value);

            if (card == null ||
                card.Definition == null)
            {
                return false;
            }

            switch (card.Definition.Effect)
            {
                case CardEffect.Damage:
                    {
                        if (card.Definition.Value <= 0)
                            return false;

                        if (action.TargetPlayerId.HasValue)
                        {
                            PlayerState target =
                                Find(
                                    state,
                                    action.TargetPlayerId.Value);

                            return IsValidOpponent(
                                actor,
                                target);
                        }

                        return
                            FindNextLivingOpponent(
                                state,
                                actor.Id) != null;
                    }

                case CardEffect.Heal:
                    return card.Definition.Value > 0;

                case CardEffect.Guard:
                    return true;

                default:
                    return false;
            }
        }

        private static bool CanEndTurn(
            GameState state,
            GameAction action)
        {
            PlayerState actor =
                Find(state, action.PlayerId);

            return actor != null &&
                   IsAlive(actor) &&
                   state.CurrentPlayerId == actor.Id;
        }

        private static bool IsValidOpponent(
            PlayerState actor,
            PlayerState target)
        {
            if (actor == null ||
                target == null)
            {
                return false;
            }

            if (actor.Id == target.Id)
                return false;

            return IsAlive(target);
        }

        public static PlayerState Find(
            GameState state,
            int playerId)
        {
            if (state == null)
                return null;

            for (int i = 0;
                 i < state.Players.Count;
                 i++)
            {
                if (state.Players[i].Id == playerId)
                    return state.Players[i];
            }

            return null;
        }

        private static PlayerState FindNextLivingOpponent(
            GameState state,
            int playerId)
        {
            if (state == null)
                return null;

            PlayerState current =
                Find(state, playerId);

            if (current == null)
                return null;

            for (int i = 1;
                 i <= state.Players.Count;
                 i++)
            {
                int index =
                    (current.Id + i) %
                    state.Players.Count;

                PlayerState candidate =
                    state.Players[index];

                if (candidate.Id != playerId &&
                    IsAlive(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool IsAlive(
            PlayerState player)
        {
            return player != null &&
                   player.Health > 0;
        }
    }
}
