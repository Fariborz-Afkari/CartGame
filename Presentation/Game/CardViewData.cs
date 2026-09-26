namespace CardGame.Presentation.Game
{
    public sealed class CardViewData
    {
        public int InstanceId { get; private set; }

        public string Name { get; private set; }

        public int Value { get; private set; }

        public CardViewData(
            int instanceId,
            string name,
            int value)
        {
            InstanceId = instanceId;
            Name = name;
            Value = value;
        }
    }
}