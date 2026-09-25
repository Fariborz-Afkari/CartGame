namespace CardGame.Core.Cards
{
    public sealed class Card
    {
        public CardDefinition Definition { get; }
        public Card(CardDefinition definition) { Definition = definition; }
    }
}
