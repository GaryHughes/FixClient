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

        if (FragmentMessages)
        {
            lock (_writer)
            {
                Write(_writer, message);
            }
        }
        else
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream, Encoding.Latin1, true))
            {
                Write(writer, message);
            }

            lock (_writer)
            {
                _writer.Write(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        OnMessageWritten(message);
    }

    static void Write(BinaryWriter writer, Message message)
    {
        int checkSumTag = FIX_5_0SP2.Fields.CheckSum.Tag;

        // Each field is encoded into one pooled buffer, grown as needed, rather than into new strings
        // and byte arrays for every field.
        byte[] buffer = ArrayPool<byte>.Shared.Rent(InitialBufferSize);

        try
        {
            foreach (Field field in message.Fields)
            {
                if (field.Tag == checkSumTag)
                    continue;

                WriteField(writer, ref buffer, field.Tag, field.Value, field.Data);
            }

            WriteField(writer, ref buffer, checkSumTag, message.CheckSum ?? string.Empty, data: false);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    const int InitialBufferSize = 256;
    const int MaxTagBytes = 11;

    static void WriteField(BinaryWriter writer, ref byte[] buffer, int tag, string value, bool data)
    {
        // The tag, '=', the value and SOH. Text is one byte per character in Latin-1 and base64 decodes
        // to fewer bytes than it has characters.
        int maximum = MaxTagBytes + value.Length + 2;

        if (buffer.Length < maximum)
        {
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = ArrayPool<byte>.Shared.Rent(maximum);
        }

        Span<byte> span = buffer;

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

        writer.Write(span[..length]);
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

