using CardGame.Core.Players;

namespace CardGame.Core.Game
{
    /// <summary>
    /// Contains the deterministic rules of the game.
    ///
    /// GameRules does not control game flow and does not emit events.
    /// It validates and resolves GameAction instances against GameState.
    ///
    /// GameEngine is responsible for:
    /// - receiving actions
    /// - enforcing turn flow
    /// - emitting GameEvents
    /// - deciding when a match starts/ends
    ///
    /// GameRules is responsible for:
    /// - validating action-specific rules
    /// - mutating the game state when an action is valid
    /// </summary>
    public static class GameRules
    {
        /// <summary>
        /// Resolves an action against the supplied state.
        ///
        /// Returns true when the action was valid and applied.
        /// Returns false when the action was rejected by the rules.
        ///
        /// GameRules itself does not create GameEvents.
        /// </summary>
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

        /// <summary>
        /// Checks whether an action can be performed without changing state.
        ///
        /// This is useful for UI, AI and input validation.
        /// </summary>
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
                    return CanDrawCard(state, actor, action);

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
            /*
             * The current project does not yet have a card/deck model
             * attached to GameState or PlayerState.
             *
             * Therefore DrawCard cannot mutate a real card collection yet.
             *
             * This method intentionally returns false until the card
             * model is introduced.
             */
            return false;
        }

        private static bool ResolvePlayCard(
            GameState state,
            PlayerState actor,
            GameAction action)
        {
            /*
             * The current project does not yet have:
             *
             * - PlayerState.Hand
             * - DeckState
             * - CardState/CardDefinition integration
             *
             * Therefore PlayCard cannot safely be resolved yet.
             *
             * Do not mutate health or other state here based only on
             * CardId. Card effects belong to the card/rules layer.
             */
            return false;
        }

        private static bool CanDrawCard(
            GameState state,
            PlayerState actor,
            GameAction action)
        {
            if (actor == null)
                return false;

            /*
             * Card/deck state is not available yet.
             * Keep this false until the card model is connected.
             */
            return false;
        }

        private static bool CanPlayCard(
            GameState state,
            PlayerState actor,
            GameAction action)
        {
            if (actor == null)
                return false;

            if (!action.CardId.HasValue)
                return false;

            /*
             * Card ownership / hand validation will be added when
             * PlayerState receives a hand/card collection.
             */
            return false;
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

        /// <summary>
        /// Finds a player by id.
        /// </summary>
        public static PlayerState Find(
            GameState state,
            int playerId)
        {
            if (state == null)
                return null;

            return state.FindPlayer(playerId);
        }

        /// <summary>
        /// Finds the next living opponent.
        ///
        /// The search starts after the supplied actor and wraps around
        /// the player list.
        /// </summary>
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
                    (actorIndex + offset) % state.Players.Count;

                PlayerState player = state.Players[index];

                if (player.Id != actorId && player.Health > 0)
                    return player;
            }

            return null;
        }

        /// <summary>
        /// Counts all living players.
        /// </summary>
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

        /// <summary>
        /// Determines whether a player is still alive.
        /// </summary>
        public static bool IsAlive(
            GameState state,
            int playerId)
        {
            PlayerState player = Find(state, playerId);

            return player != null && player.Health > 0;
        }

        /// <summary>
        /// Determines whether the supplied player is the only
        /// living player remaining.
        /// </summary>
        public static bool IsLastLivingPlayer(
            GameState state,
            int playerId)
        {
            if (!IsAlive(state, playerId))
                return false;

            if (CountLiving(state) != 1)
                return false;

            return true;
        }
    }
}
