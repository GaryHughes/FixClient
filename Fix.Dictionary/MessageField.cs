namespace Fix;

public static partial class Dictionary
{
    public class MessageField
    {
        VersionField field;

        public MessageField(VersionField field, bool required, int depth)
        {
            this.field = field;
            Required = required;
            Depth = depth;
        }

        public int Tag => @field.Tag;
        public string Name => @field.Name;
        public string Description => @field.Description;
        public bool Required { get; }
        public int Depth { get; }

    }
}
