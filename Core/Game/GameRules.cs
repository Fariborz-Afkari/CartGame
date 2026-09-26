using CardGame.Core.Cards;
using CardGame.Core.Game;
using CardGame.Core.Players;
using static UnityEngine.GraphicsBuffer;

namespace CardGame.Core.Game
{
    /// <summary>
    /// Contains the deterministic rules of the game.
    ///
    /// GameRules does not control game flow and does not emit events.
    /// It validates and resolves GameAction instances against GameState.
    /// </summary>
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

            PlayerState actor = Find(state, action.PlayerId);

            if (actor == null || actor.Health <= 0)
                return false;

            if (state.CurrentPlayerId != actor.Id)
                return false;

            switch (action.Type)
            {
                case GameActionType.DrawCard:
                    return ResolveDrawCard(state, actor, action);

                case GameActionType.PlayCard:
                    return ResolvePlayCard(state, actor, action);

                case GameActionType.EndTurn:
                    return CanEndTurn(state, actor);

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

            PlayerState actor = Find(state, action.PlayerId);

            if (actor == null || actor.Health <= 0)
                return false;

            if (state.CurrentPlayerId != actor.Id)
                return false;

            switch (action.Type)
            {
                case GameActionType.DrawCard:
                    return CanDrawCard(state, actor);

                case GameActionType.PlayCard:
                    return CanPlayCard(state, actor, action);

                case GameActionType.EndTurn:
                    return CanEndTurn(state, actor);

                default:
                    return false;
            }
        }

        private static bool ResolveDrawCard(
            GameState state,
            PlayerState actor,
            GameAction action)
        {
            if (!CanDrawCard(state, actor))
                return false;

            Card card = state.Deck.Draw();

            if (card == null)
                return false;

            actor.Hand.Add(card);

            return true;
        }

        private static bool ResolvePlayCard(
            GameState state,
            PlayerState actor,
            GameAction action)
        {
            if (!CanPlayCard(state, actor, action))
                return false;

            Card card = actor.Hand.Find(action.CardId.Value);

            if (card == null || card.Definition == null)
                return false;

            bool resolved = ResolveCardEffect(
                state,
                actor,
                card,
                action);

            if (!resolved)
                return false;

            /*
             * The card is consumed only after its effect
             * has been successfully resolved.
             */
            return actor.Hand.Remove(card.InstanceId);
        }

        private static bool ResolveCardEffect(
            GameState state,
            PlayerState actor,
            Card card,
            GameAction action)
        {
            CardDefinition definition = card.Definition;

            switch (definition.Effect)
            {
                case CardEffect.Damage:
                    return ResolveDamage(
                        state,
                        actor,
                        definition.Value,
                        action.TargetPlayerId);

                case CardEffect.Heal:
                    return ResolveHeal(
                        actor,
                        definition.Value);

                case CardEffect.Guard:
                    return ResolveGuard(actor);

                default:
                    return false;
            }
        }

        private static bool ResolveDamage(
            GameState state,
            PlayerState actor,
            int amount,
            int? targetPlayerId)
        {
            if (amount <= 0)
                return false;

            PlayerState target;

            if (targetPlayerId.HasValue)
            {
                target = Find(
                    state,
                    targetPlayerId.Value);

                if (!IsValidOpponent(actor, target))
                    return false;
            }
            else
            {
                target = FindNextLivingOpponent(
                    state,
                    actor.Id);

                if (target == null)
                    return false;
            }

            target.Damage(amount);

            return true;
        }

        private static bool ResolveHeal(
            PlayerState actor,
            int amount)
        {
            if (amount <= 0)
                return false;

            actor.Heal(amount);

            return true;
        }

        private static bool ResolveGuard(
            PlayerState actor)
        {
            actor.Guarding = true;

            return true;
        }

        private static bool CanDrawCard(
            GameState state,
            PlayerState actor)
        {
            if (state == null || actor == null)
                return false;

            return !state.Deck.IsEmpty;
        }

        private static bool CanPlayCard(
            GameState state,
            PlayerState actor,
            GameAction action)
        {
            if (state == null || actor == null || action == null)
                return false;

            if (!action.CardId.HasValue)
                return false;

            Card card = actor.Hand.Find(
                action.CardId.Value);

            if (card == null || card.Definition == null)
                return false;

            CardDefinition definition = card.Definition;

            switch (definition.Effect)
            {
                case CardEffect.Damage:
                    if (definition.Value <= 0)
                        return false;

                    /*
                     * A missing target means:
                     * "attack the next living opponent".
                     */
                    if (!action.TargetPlayerId.HasValue)
                        return FindNextLivingOpponent(
                            state,
                            actor.Id) != null;

                    PlayerState damageTarget =
                        Find(
                            state,
                            action.TargetPlayerId.Value);

                    return IsValidOpponent(
                        actor,
                        damageTarget);

                case CardEffect.Heal:
                    /*
                     * Heal is self-targeted in the current
                     * simple card game.
                     */
                    return definition.Value > 0;

                case CardEffect.Guard:
                    /*
                     * Guard is self-targeted.
                     */
                    return true;

                default:
                    return false;
            }
        }

        private static bool IsValidOpponent(
            PlayerState actor,
            PlayerState target)
        {
            if (actor == null || target == null)
                return false;

            if (target.Id == actor.Id)
                return false;

            return target.Health > 0;
        }

        private static bool CanEndTurn(
            GameState state,
            PlayerState actor)
        {
            if (actor == null)
                return false;

            return actor.Health > 0 &&
                   state.CurrentPlayerId == actor.Id;
        }

        public static PlayerState Find(
            GameState state,
            int playerId)
        {
            if (state == null)
                return null;

            return state.FindPlayer(playerId);
        }

        public static PlayerState FindNextLivingOpponent(
            GameState state,
            int actorId)
        {
            if (state == null || state.Players.Count == 0)
                return null;

            int actorIndex = -1;

            for (int i = 0; i < state.Players.Count; i++)
            {
                if (state.Players[i].Id == actorId)
                {
                    actorIndex = i;
                    break;
                }
            }

            if (actorIndex < 0)
                return null;

            for (int offset = 1;
                 offset <= state.Players.Count;
                 offset++)
            {
                int index =
                    (actorIndex + offset) %
                    state.Players.Count;

                PlayerState player = state.Players[index];

                if (player.Id != actorId &&
                    player.Health > 0)
                {
                    return player;
                }
            }

            return null;
        }

        public static int CountLiving(GameState state)
        {
            if (state == null)
                return 0;

            int count = 0;

            for (int i = 0; i < state.Players.Count; i++)
            {
                if (state.Players[i].Health > 0)
                    count++;
            }

            return count;
        }

        public static bool IsAlive(
            GameState state,
            int playerId)
        {
            PlayerState player =
                Find(state, playerId);

            return player != null &&
                   player.Health > 0;
        }

        public static bool IsLastLivingPlayer(
            GameState state,
            int playerId)
        {
            if (!IsAlive(state, playerId))
                return false;

            return CountLiving(state) == 1;
        }
    }
}
