using System.Collections.Generic;
using System.Linq;

namespace Fix;

public partial class Dictionary
{
    public class VersionMessageCollection : IEnumerable<Message>
    {
        public int Length => Messages.Length;

        public Message? this[string msgType]
        {
            get
            {
                return (from message in Messages
                        where message.MsgType == msgType
                        select message).FirstOrDefault();
            }
        }

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
            }
        }

        private Message[] _messages = new Message[] { };
    }
}
