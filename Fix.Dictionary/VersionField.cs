using System.Collections.Generic;

namespace Fix;

public static partial class Dictionary
{
    public static bool IsValid(VersionField? field) =>
        field switch
        {
            VersionField valid => valid.IsValid,
            null => false
        };

    public class VersionField
    {
        public VersionField(int tag, string name, string dataType, string description, Pedigree pedigree, params FieldValue[] values)
        {
            Tag = tag;
            Name = name;
            Description = description;
            DataType = dataType;
            Pedigree = pedigree;
            foreach (var value in values)
            {
                Values.Add(value.Value, value);
            }
        }
        public static VersionField Dummy = new VersionField(0, string.Empty, string.Empty, string.Empty, new Pedigree { });
        public int Tag { get; }
        public string Name { get; }
        public string Description { get; }

        public bool IsValid => Tag != 0;

        // TODO - make this reference the actual type in the dictionary directly
        public string DataType { get; }

        public Pedigree Pedigree { get; }
        public OrderedDictionary<string, FieldValue> Values { get; } = new OrderedDictionary<string, FieldValue>();
    }
}

