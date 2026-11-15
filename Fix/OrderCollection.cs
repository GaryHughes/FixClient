using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Fix;

public class OrderCollection : IEnumerable<Order>
{
    public void Clear()
    {
        _keyIndex.Clear();
        _sendingTimeIndex.Clear();
    }

    public int Count => _keyIndex.Count;

    public void Add(Order order)
    {
        _keyIndex.Add(order.Key, order);

        var key = new SendingTimeKey
        {
            SendingTime = order.SendingTime,
            Index = _nextIndex++
        };

        _sendingTimeIndex.Add(key, order);
    }

    public Order this[int index] => _sendingTimeIndex.Values[index];

    public bool Contains(Order order) => _keyIndex.ContainsKey(order.Key);

    public Order? Find(string key)
    {
        _keyIndex.TryGetValue(key, out var order);
        return order;
    }

    public bool Remove(string key)
    {
        return _keyIndex.Remove(key);
        // TODO - sendingTimeIndex
    }

    public void ReplaceKey(string existing, string replacement)
    {
        if (_keyIndex.ContainsKey(replacement))
        {
            throw new ArgumentException($"replacement key {replacement} is already in use", nameof(replacement));
        }

        if (!TryGetValue(existing, out var result))
        {
            throw new ArgumentException($"existing key {existing} can not be found", nameof(existing));
        }

        Remove(existing);

        if (result is Order order)
        {
            Add(order);
        }
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out Order? result)
    {
        return _keyIndex.TryGetValue(key, out result);
    }

    public IEnumerable<Order> GetRange(DateTime from, DateTime to)
    {
        var keys = GetKeysForRange(from, to);

        if (keys.Length == 0)
        {
            yield break;
        }

        foreach (var key in keys)
        {
            yield return _sendingTimeIndex[key];
        }
    }

    public void RemoveRange(DateTime from, DateTime to)
    {
        foreach (var key in GetKeysForRange(from, to))
        {
            _keyIndex.Remove(_sendingTimeIndex[key].Key);
            _sendingTimeIndex.Remove(key);
        }
    }

    SendingTimeKey[] GetKeysForRange(DateTime from, DateTime to)
    {
        var keys = _sendingTimeIndex.Keys.ToArray();
        // Array.BinarySearch returns
        // The index of the value if it is found.
        // A negative value if the value is not found.
        //  - The bitwise complement of the index of the first element that is larger than the value.
        //  - If there is no larger value it will the bitwise complement of the size of the array.  
        var fromKey = new SendingTimeKey
        {
            SendingTime = from,
            Index = 0 // Find the first key with this SendingTime
        };

        var lower = Array.BinarySearch(keys, fromKey, new KeyComparer());

        if (lower == ~keys.Length)
        {
            return Array.Empty<SendingTimeKey>();
        }

        if (lower < 0)
        {
            lower = ~lower;
        }

        var toKey = new SendingTimeKey
        {
            SendingTime = to,
            Index = int.MaxValue // Find the last key with this SendingTime
        };

        var upper = Array.BinarySearch(keys, lower, keys.Length - lower, toKey, new KeyComparer());

        if (upper < 0)
        {
            upper = ~upper;
        }

        var result = new SendingTimeKey[upper - lower];
        Array.Copy(keys, lower, result, 0, upper - lower);
        return result;
    }

    #region IEnumerable<Order>

    public IEnumerator<Order> GetEnumerator() => _sendingTimeIndex.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    readonly Dictionary<string, Order> _keyIndex = new();

    // Use a compound with a unique int value so we can have orders with duplicate SendingTime.
    struct SendingTimeKey : IComparable<SendingTimeKey>
    {
        public DateTime SendingTime;
        public int Index;
        public int CompareTo(SendingTimeKey other)
        {
            var result = SendingTime.CompareTo(other.SendingTime);

            if (result == 0)
            {
                return Index.CompareTo(other.Index);
            }

            return result;
        }
    }

    class KeyComparer : IComparer
    {
        public int Compare(object? x, object? y)
        {
            static int CompareSendingTime(SendingTimeKey left, SendingTimeKey right)
            {
                var result = left.SendingTime.CompareTo(right.SendingTime);
                if (result == 0)
                {
                    return left.Index.CompareTo(right.Index);
                }
                return result;
            }

            return (x, y) switch
            {
                (null, null) => 0,
                (object, null) => 1,
                (null, object) => -1,
                (SendingTimeKey left, SendingTimeKey right) values => CompareSendingTime(left, right),
                (not null, not null) => throw new ArgumentException($"Cannot compare arguments that are not of type {nameof(SendingTimeKey)}")
            };
        }
    }

    int _nextIndex;
    readonly SortedList<SendingTimeKey, Order> _sendingTimeIndex = new();

}
