namespace CardGame.Core.Cards
{
    public enum CardEffect
    {
        Damage,
        Heal,
        Guard
    }

    public sealed class CardDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public CardEffect Effect { get; }
        public int Value { get; }

        public CardDefinition(string id, string name, CardEffect effect, int value)
        {
            Id = id; Name = name; Effect = effect; Value = value;
        }
    }
}
