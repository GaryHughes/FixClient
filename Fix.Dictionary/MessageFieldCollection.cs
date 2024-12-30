using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Fix.Common;

namespace Fix;

public static partial class Dictionary
{
    public class MessageFieldCollection : IEnumerable<MessageField>
    {
        internal MessageFieldCollection(params MessageField[] fields)
        {
            foreach (var field in fields)
            {
                _fields.Add(field.Tag, field);
            }
        }

        public bool TryGetValue(int tag, [MaybeNullWhen(false)] out MessageField result)
        {
            return _fields.TryGetValue(tag, out result);
        }

        public int Count => _fields.Count;
        public MessageField this[int index] => _fields.GetAt(index).Value;

        public virtual IEnumerator<MessageField> GetEnumerator() => _fields.Values.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        readonly OrderedDictionary<int, MessageField> _fields = new OrderedDictionary<int, MessageField>();

        public override string ToString() => Count.ToString();

    }
}

