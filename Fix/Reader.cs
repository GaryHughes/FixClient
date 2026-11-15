/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: Reader.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Text;
using static Fix.Dictionary;

namespace Fix;

public class Reader : IDisposable
{
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

    public event MessageDelegate? MessageRead;

    protected void OnMessageRead(Message message)
    {
        MessageRead?.Invoke(this, new MessageEvent(message));
    }

    #endregion

    public Reader(Stream stream)
    {
        _stream = stream;
        ValidateDataFields = true;
    }

    public bool ValidateDataFields { get; set; }

    public void Close()
    {
    }

    public Message Read()
    {
        var message = new Message();
        message.Fields.Clear();
        Read(message);
        return message;
    }

    public void DiscardLine()
    {
        try
        {
            char lastChar = ReadChar();
            for (; ; )
            {
                char newChar = ReadChar();
                if (lastChar == '\r' && newChar == '\n')
                    return;
                lastChar = newChar;
            }
        }
        catch (EndOfStreamException)
        {
        }
    }

    public Message? ReadLine()
    {
        Message? message = null;
        try
        {
            char direction = ReadChar();

            if (direction != '<' && direction != '>')
            {
                throw new Exception($"Invalid direction prefix, expected '<' or '>', received '{direction}'");
            }

            message = Read();
            message.Incoming = direction == '<';

            for (; ; )
            {
                char c = PeekChar();
                if (c == '\r' || c == '\n')
                {
                    ReadChar();
                }
                else
                {
                    break;
                }
            }
        }
        catch (EndOfStreamException)
        {
        }
        return message;
    }

    public void Read(Message message)
    {
        Field? previous = null;

        for (long index = 0; ; ++index)
        {
            _tag.Length = 0;
            _value.Length = 0;

            char token;

            for (; ; )
            {
                token = ReadChar();

                if (token == Field.ValueSeparator || token == Field.Separator)
                    break;

                _tag.Append(token);
            }

            VersionField? definition = null;

            if (ValidateDataFields &&
                FIX_5_0SP2.Fields.TryGetValue(int.Parse(_tag.ToString()), out definition) &&
                definition != null &&
                definition.DataType == FIX_5_0SP2.DataTypes.data.Name)
            {
                if (previous is null)
                {
                    throw new Exception($"Encountered a data type field at index {index} [{definition.Tag}] with no previous field");
                }

                if (!int.TryParse(previous.Value, out int dataLength))
                {
                    throw new Exception($"Encountered a data type field at index {index} [{definition.Tag}] but the previous field {previous} was not numeric");
                }

                byte[] bytes = new byte[dataLength];

                if (ReadChars(bytes, 0, bytes.Length) != bytes.Length)
                {
                    throw new EndOfStreamException();
                }

                _value.Append(Convert.ToBase64String(bytes));

                char trailing = ReadChar();

                if (trailing != Field.Separator)
                {
                    throw new Exception($"Read [{trailing}] when expecting SOH field separator after data field [{definition.Tag}]");
                }
            }
            else
            {
                try
                {
                    for (; ; )
                    {
                        token = ReadChar();

                        if (Convert.ToByte(token) == Field.Separator)
                            break;

                        _value.Append(token);
                    }
                }
                catch (EndOfStreamException)
                {
                }
            }

            var field = new Field(_tag.ToString(), _value.ToString())
            {
                Data = definition != null && definition.DataType == FIX_5_0SP2.DataTypes.data.Name
            };

            message.Fields.Add(field);

            if (field.Tag == FIX_5_0SP2.Fields.CheckSum.Tag)
                break;

            previous = field;
        }

        OnMessageRead(message);
    }

    #region IDisposable Members

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            Close();
            _disposed = true;
        }
    }

    bool _disposed;

    #endregion

    char ReadChar()
    {
        if (_peekChar.HasValue)
        {
            char c = _peekChar.Value;
            _peekChar = null;
            return c;
        }

        int result = _stream.ReadByte();

        if (result == -1)
            throw new EndOfStreamException();

        return (char)result;
    }

    int ReadChars(byte[] buffer, int offset, int count)
    {
        if (_peekChar.HasValue)
        {
            buffer[offset++] = (byte)_peekChar.Value;
            _peekChar = null;
            --count;
        }

        int result = _stream.Read(buffer, offset, count);

        if (result == -1)
            throw new EndOfStreamException();

        return result;
    }

    char PeekChar()
    {
        if (!_peekChar.HasValue)
        {
            _peekChar = ReadChar();
        }

        return _peekChar.Value;
    }

    char? _peekChar;
    readonly Stream _stream;
    readonly StringBuilder _tag = new(256);
    readonly StringBuilder _value = new(256);

}

