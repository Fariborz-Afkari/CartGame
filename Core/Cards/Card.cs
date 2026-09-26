namespace CardGame.Core.Cards
{
    /// <summary>
    /// Represents a concrete card instance.
    ///
    /// Card contains the definition of the card but does not contain
    /// gameplay logic. Gameplay rules are handled by GameRules.
    /// </summary>
    public sealed class Card
    {
        public int InstanceId { get; }

        public CardDefinition Definition { get; }

        public Card(
            int instanceId,
            CardDefinition definition)
        {
            if (definition == null)
                throw new System.ArgumentNullException(
                    nameof(definition));

            if (instanceId < 0)
                throw new System.ArgumentOutOfRangeException(
                    nameof(instanceId));

            InstanceId = instanceId;
            Definition = definition;
        }
    }
}
