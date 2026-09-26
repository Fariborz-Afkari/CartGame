using System.Collections.Generic;
using CardGame.Core.Cards;

namespace CardGame.Games.SimpleCardGame
{
    public static class SimpleCardGameRules
    {
        public const int CardsPerDefinition = 8;
        public const int StartingHandSize = 3;

        public static readonly CardDefinition Attack =
            new CardDefinition(
                "attack",
                "Attack",
                CardEffect.Damage,
                3);

        public static readonly CardDefinition Heal =
            new CardDefinition(
                "heal",
                "Heal",
                CardEffect.Heal,
                2);

        public static readonly CardDefinition Guard =
            new CardDefinition(
                "guard",
                "Guard",
                CardEffect.Guard,
                0);

        /// <summary>
        /// Creates a fresh deck for one match.
        ///
        /// The concrete game owns the composition of its deck;
        /// Core only knows how to store, shuffle and draw cards.
        /// </summary>
        public static IEnumerable<Card> CreateDeck()
        {
            int instanceId = 0;

            for (int i = 0; i < CardsPerDefinition; i++)
                yield return new Card(instanceId++, Attack);

            for (int i = 0; i < CardsPerDefinition; i++)
                yield return new Card(instanceId++, Heal);

            for (int i = 0; i < CardsPerDefinition; i++)
                yield return new Card(instanceId++, Guard);
        }
    }
}
