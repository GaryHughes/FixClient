using System.Buffers;
using System.Globalization;
using System.Text;
using static Fix.Dictionary;

namespace Fix;

public class Writer : IDisposable
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

    public event MessageDelegate? MessageWriting;
    public event MessageDelegate? MessageWritten;

    protected void OnMessageWriting(Message message)
    {
        MessageWriting?.Invoke(this, new MessageEvent((Message)message.Clone()));
    }

    protected void OnMessageWritten(Message message)
    {
        MessageWritten?.Invoke(this, new MessageEvent((Message)message.Clone()));
    }

    #endregion

    public Writer(Stream stream, bool leaveOpen)
    {
        _writer = new BinaryWriter(stream, Encoding.Latin1, leaveOpen);
    }

    // When true each field is written to the stream as it is encoded rather than the whole message
    // being collected first and written at once. This matches the Session default.
    public bool FragmentMessages { get; set; } = true;

    public void Close()
    {
        _writer?.Close();
    }

    void WriteMessage(Message message)
    {
        message.Fields.Set(FIX_5_0SP2.Fields.BodyLength.Tag, message.ComputeBodyLength());
        // Remove any existing checksum, in the case of resends it might already be present and
        // we need to ensure it is the last field.
        message.Fields.Remove(FIX_5_0SP2.Fields.CheckSum);
        message.Fields.Add(FIX_5_0SP2.Fields.CheckSum.Tag, message.ComputeCheckSum());

        OnMessageWriting(message);

        // Fields are encoded into a pooled buffer, grown as needed, rather than into new strings and
        // byte arrays for every field or a MemoryStream for every message.
        byte[] buffer = ArrayPool<byte>.Shared.Rent(InitialBufferSize);

        try
        {
            if (FragmentMessages)
            {
                lock (_writer)
                {
                    Encode(message, ref buffer, _writer);
                }
            }
            else
            {
                int length = Encode(message, ref buffer, fragments: null);

                lock (_writer)
                {
                    _writer.Write(buffer, 0, length);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        OnMessageWritten(message);
    }

    // Encodes the message into buffer. When fragments is given each field is written to it as soon as
    // it is encoded and the buffer reused for the next field. Otherwise the whole message accumulates
    // in the buffer and its length is returned.
    static int Encode(Message message, ref byte[] buffer, BinaryWriter? fragments)
    {
        int checkSumTag = FIX_5_0SP2.Fields.CheckSum.Tag;
        int length = 0;

        foreach (Field field in message.Fields)
        {
            if (field.Tag == checkSumTag)
                continue;

            length = EncodeField(ref buffer, length, field.Tag, field.Value, field.Data);

            if (fragments is not null)
            {
                fragments.Write(buffer, 0, length);
                length = 0;
            }
        }

        length = EncodeField(ref buffer, length, checkSumTag, message.CheckSum ?? string.Empty, data: false);

        if (fragments is not null)
        {
            fragments.Write(buffer, 0, length);
            length = 0;
        }

        return length;
    }

    const int InitialBufferSize = 256;
    const int MaxTagBytes = 11;

    // Encodes the field at offset in buffer, growing the buffer if needed, and returns the offset just
    // past it.
    static int EncodeField(ref byte[] buffer, int offset, int tag, string value, bool data)
    {
        // The tag, '=', the value and SOH. Text is one byte per character in Latin-1 and base64 decodes
        // to fewer bytes than it has characters.
        int maximum = offset + MaxTagBytes + value.Length + 2;

        if (buffer.Length < maximum)
        {
            byte[] larger = ArrayPool<byte>.Shared.Rent(Math.Max(maximum, buffer.Length * 2));
            buffer.AsSpan(0, offset).CopyTo(larger);
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = larger;
        }

        Span<byte> span = buffer.AsSpan(offset);

        tag.TryFormat(span, out int length, default, CultureInfo.InvariantCulture);
        span[length++] = Field.ValueSeparator;

        if (data)
        {
            if (!Convert.TryFromBase64String(value, span[length..], out int written))
            {
                // Not valid base64; let FromBase64String throw the same FormatException as before.
                written = Convert.FromBase64String(value).Length;
            }
            length += written;
        }
        else
        {
            // Characters outside Latin-1 are written as '?'.
            length += Encoding.Latin1.GetBytes(value, span[length..]);
        }

        span[length++] = Field.Separator;

        return offset + length;
    }

    public void Write(Message message)
    {
        WriteMessage(message);
        _writer.Flush();
    }

    public void WriteLine(Message message)
    {
        _writer.Write(Encoding.Latin1.GetBytes(message.Incoming ? "<" : ">"));
        WriteMessage(message);
        _writer.Write(Encoding.Latin1.GetBytes("\r\n"));
        _writer.Flush();
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

    readonly BinaryWriter _writer;
}

