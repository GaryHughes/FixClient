namespace Fix;

public partial class Dictionary
{
    public sealed record FieldValue(int Tag, string Name, string Value, string Description, Fix.Dictionary.Pedigree Pedigree)
    {
        public string Name { get; private set; } = Name;

        public override int GetHashCode()
        {
            return HashCode.Combine(Tag, Value);
        }

        public bool Equals(FieldValue? other)
        {
            if (other is FieldValue right)
            {
                return Tag == right.Tag && Value == right.Value;
            }

            return false;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                if (FIX_5_0SP2.Fields.TryGetValue(Tag, out var fieldDfinition) &&
                    fieldDfinition.Values.TryGetValue(Value, out var valueDefinition))
                {
                    Name = valueDefinition.Name;
                }
            }

            return Name;
        }
    }
}
