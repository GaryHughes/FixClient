using System.Collections.Generic;
using System.Linq;

namespace Fix;

public static partial class Dictionary
{
    // This indices of this collection are 0 based, FIX field tags are 1 based. The code
    // generator inserts a null at index 0 so we can efficently do Field.Tag based lookups. 
    // There are also gaps in the sequence, particularly in more recent versions of the FIX 
    // standard so there may be large sequences of nulls in the collection. This has little 
    // effect on memory usage but lets us use natural FIX tag based indexing into the collection.
    // 
    // eg. var OrderQty = Fix.Dictionary.FIX_4_0.Fields[38];
    //
    public abstract class VersionFieldCollection : IEnumerable<VersionField>
    {
        // There are gaps in the sequence of tags and we populate those with dummy values so
        // don't call this property Count or Length as that is misleading.
        public int MaxTag => Fields.Length - 1;

        public VersionField this[int tag]
        {
            get
            {
                if (tag < 1 || tag > Fields.Length - 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(tag), tag, $"Tag value {tag} must be between 1 and {Fields.Length}");
                }
                return Fields[tag];
            }
        }

        public bool TryGetValue(int tag, out VersionField field)
        {
            if (tag < 1 || tag > Fields.Length - 1)
            {
                field = VersionField.Dummy;
                return false;
            }

            field = Fields[tag];

            return IsValid(field);
        }

        public bool Contains(int tag)
        {
            return TryGetValue(tag, out var _);
        }

        public VersionField this[string name]
        {
            get
            {
                if (!TryGetValue(name, out var field))
                {
                    throw new ArgumentOutOfRangeException(nameof(name), name, $"{name} is not a valid field name");
                }
                return field;
            }
        }


        public bool TryGetValue(string name, out VersionField field)
        {
            var value = (from candidate in Fields
                         where candidate?.Name.ToLower() == name.ToLower()
                         select candidate).FirstOrDefault();

            if (value is null)
            {
                field = VersionField.Dummy;
                return false;
            }

            field = value;

            return true;
        }

        public IEnumerator<VersionField> GetEnumerator()
        {
            foreach (var field in Fields)
            {
                yield return field;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public VersionField[] Fields
        {
            get
            {
                if (_fields is null)
                {
                    throw new NullReferenceException("VersionFieldCollection._fields is unexpectedly null");
                }
                return _fields;
            }
            protected set
            {
                _fields = value;
            }
        }

        protected VersionField[] _fields = new VersionField[] { };
    }
}

