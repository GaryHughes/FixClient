using System.Collections.Frozen;

namespace Fix;

public partial class Dictionary
{
    public class VersionMessageCollection : IEnumerable<Message>
    {
        public int Length => Messages.Length;

        // Looked up for every message sent, received or displayed, so index by MsgType rather than
        // scanning the list, which has over a hundred messages in the larger versions.
        public Message? this[string msgType] => _messagesByMsgType.TryGetValue(msgType, out var message) ? message : null;

        public IEnumerator<Message> GetEnumerator()
        {
            foreach (var message in Messages)
            {
                yield return message;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public Message[] Messages
        {
            get
            {
                if (_messages is null)
                {
                    throw new NullReferenceException("VersionMessageCollection._messages is unexpectedly null");
                }
                return _messages;
            }
            protected set
            {
                _messages = value;

                // If a MsgType were ever defined twice keep the first, as the previous linear search did.
                var byMsgType = new Dictionary<string, Message>();
                foreach (var message in value)
                {
                    byMsgType.TryAdd(message.MsgType, message);
                }
                _messagesByMsgType = byMsgType.ToFrozenDictionary();
            }
        }

        private Message[] _messages = new Message[] { };
        private FrozenDictionary<string, Message> _messagesByMsgType = FrozenDictionary<string, Message>.Empty;
    }
}
