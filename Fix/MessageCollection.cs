namespace Fix;

public class MessageCollection : IEnumerable<Message>, ICloneable
{
    readonly List<Message> _messages = new();

    #region Events

    public class MessageEvent : EventArgs
    {
        public MessageEvent(Message message)
        {
            Message = message;
        }

        public Message Message { get; }
    }

    public delegate void MessageDelegate(object sender, MessageEvent e);
    public delegate void ResetDelegate(object sender);

    public event MessageDelegate? MessageAdded;
    public event ResetDelegate? Reset;

    protected void OnMessageAdded(Message message)
    {
        MessageAdded?.Invoke(this, new MessageEvent(message));
    }

    protected void OnReset()
    {
        Reset?.Invoke(this);
    }

    #endregion

    public void Add(Message message)
    {
        _messages.Add(message);
        OnMessageAdded(message);
    }

    public int Count => _messages.Count;

    public Message this[int index] => _messages[index];

    public void Clear()
    {
        _messages.Clear();
        OnReset();
    }

    public static async Task<MessageCollection> Parse(string path)
    {
        var result = new MessageCollection();
        var url = new Uri($"file://{Path.GetFullPath(path)}");
        await foreach (var message in Parser.Parse(url))
        {
            if (message is null)
            {
                break;
            }
            result.Add(message);
        }
        return result;
    }

    #region IEnumerable<Message>

    public IEnumerator<Message> GetEnumerator()
    {
        return _messages.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    public object Clone()
    {
        var clone = new MessageCollection();
        foreach (Message message in _messages)
            clone.Add(message);
        return clone;
    }
}

