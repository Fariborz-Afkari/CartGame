using CardGame.Core.Players;

namespace CardGame.Core.Game
{
    public static class GameRules
    {
        public const int AttackDamage = 3;
        public const int HealAmount = 2;

        public static void Resolve(GameState state, GameAction action)
        {
            PlayerState actor = Find(state, action.ActorId);
            if (actor == null || actor.Health <= 0) return;

            switch (action.Type)
            {
                case GameActionType.Attack:
                    PlayerState target = FindNextLivingOpponent(state, actor.Id);
                    if (target != null)
                    {
                        target.Damage(AttackDamage);
                        state.AddLog(actor.Name + " attacks " + target.Name + " for " + AttackDamage + ".");
                    }
                    break;
                case GameActionType.Heal:
                    actor.Heal(HealAmount);
                    state.AddLog(actor.Name + " heals for " + HealAmount + ".");
                    break;
                case GameActionType.Guard:
                    actor.Guarding = true;
                    state.AddLog(actor.Name + " guards.");
                    break;
            }
        }

        public static PlayerState Find(GameState state, int id)
        {
            for (int i = 0; i < state.Players.Count; i++)
                if (state.Players[i].Id == id) return state.Players[i];
            return null;
        }

        public static PlayerState FindNextLivingOpponent(GameState state, int actorId)
        {
            for (int i = 0; i < state.Players.Count; i++)
            {
                var p = state.Players[i];
                if (p.Id != actorId && p.Health > 0) return p;
            }
            return null;
        }

        public static int CountLiving(GameState state)
        {
            int count = 0;
            for (int i = 0; i < state.Players.Count; i++)
                if (state.Players[i].Health > 0) count++;
            return count;
        }
    }
}
