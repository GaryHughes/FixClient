namespace Fix;

public partial class Dictionary
{
    public abstract class Message
    {
        protected Message(string msgType, string name, string description, Pedigree pedigree, MessageFieldCollection fields)
        {
            MsgType = msgType;
            Name = name;
            Description = description;
            Pedigree = pedigree;
            Fields = fields;
        }

        public string MsgType { get; }
        public string Name { get; }
        public string Description { get; }
        public Pedigree Pedigree { get; }
        public MessageFieldCollection Fields { get; }
    }
}

