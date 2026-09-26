using CardGame.Core.Cards;

namespace CardGame.Games.SimpleCardGame
{
    public static class SimpleCardGameRules
    {
        public static readonly CardDefinition Attack = new CardDefinition("attack", "Attack", CardEffect.Damage, 3);
        public static readonly CardDefinition Heal = new CardDefinition("heal", "Heal", CardEffect.Heal, 2);
        public static readonly CardDefinition Guard = new CardDefinition("guard", "Guard", CardEffect.Guard, 0);
    }
}
