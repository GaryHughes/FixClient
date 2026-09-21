/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: Writer.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.IO;
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

    public bool FragmentMessages { get; set; }

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
        foreach (Field field in message.Fields)
        {
            if (field.Tag == FIX_5_0SP2.Fields.CheckSum.Tag)
                continue;

            if (field.Data)
            {
                byte[] bytes = Convert.FromBase64String(field.Value);
                writer.Write(Encoding.Latin1.GetBytes(string.Format("{0}=", field.Tag)));
                writer.Write(bytes);
                writer.Write(Encoding.Latin1.GetBytes("\x01"));
            }
            else
            {
                writer.Write(Encoding.Latin1.GetBytes($"{field.Tag}={field.Value}\x01"));
            }
        }

        writer.Write(Encoding.Latin1.GetBytes($"{(int)FIX_5_0SP2.Fields.CheckSum.Tag}={message.CheckSum}\x01"));
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

