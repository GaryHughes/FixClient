using System.ComponentModel;
using System.Net.Sockets;
using static Fix.Dictionary;

namespace Fix;

public partial class Session
{
    [Browsable(false)]
    public bool Connected
    {
        get { return _stream != null && _stream.CanRead && _stream.CanWrite; }
    }

    [Browsable(false)]
    public Stream? Stream
    {
        get { return _stream; }
        set
        {
            _stream = value;
            if (_stream is Stream)
            {
                State = State.Connected;
                // Reader consumes the stream a byte at a time. A NetworkStream does no buffering of its
                // own, so without this every byte read was a separate socket receive.
                _reader = new Reader(new BufferedStream(_stream))
                {
                    ValidateDataFields = ValidateDataFields
                };
                if (_writer != null)
                {
                    _writer.MessageWriting -= WriterMessageWriting;
                }
                // Writer writes each field to the stream separately when fragmenting messages, and a
                // NetworkStream does no buffering of its own, so without this every field was a
                // separate socket send. Writer flushes after every message.
                _writer = new Writer(new BufferedStream(_stream), true);
                _writer.MessageWriting += WriterMessageWriting;
            }
        }
    }

    void WriterMessageWriting(object sender, Writer.MessageEvent ev)
    {
        OnMessageSending(ev.Message);
    }

    public virtual void Open()
    {
        Logon();
        Task.Run(() =>
        {
            try
            {
                while (_reader != null)
                {
                    Message message = _reader.Read();
                    if (message == null)
                        break;
                    Receive(message);
                }
            }
            catch (EndOfStreamException ex)
            {
                OnError(ex.Message);
                Close();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
                Close();
            }
        });
    }

    readonly object _syncRoot = new();

    public virtual void Close()
    {
        lock (_syncRoot)
        {
            if (State == State.Disconnected)
                return;

            _testRequestTimer?.Dispose();
            _testRequestTimer = null;

            StopDefibrillator();

            _reader?.Close();
            _reader = null;

            try
            {
                // Flushes anything still buffered. If the connection has already failed that
                // throws, and the data can't be sent anyway, so carry on closing.
                _writer?.Close();
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException)
            {
            }
            _writer = null;

            var networkStream = _stream as NetworkStream;
            networkStream?.Socket.Shutdown(SocketShutdown.Send);
            _stream?.Close();
            _stream = null;

            State = State.Disconnected;
        }
    }

    public void Send(Message message, bool setSeqNum = true)
    {
        lock (_syncRoot)
        {
            PerformSend(message, setSeqNum);
        }
    }

    void PerformSend(Message message, bool setSeqNum = true)
    {
        try
        {
            if (message.MsgType == FIX_5_0SP2.Messages.TestRequest.MsgType)
            {
                if (message.Fields.Find(FIX_5_0SP2.Fields.TestReqID) is Field testReqId)
                {
                    ExpectedTestRequestId = testReqId.Value;
                }
            }
            else if (message.MsgType == FIX_5_0SP2.Messages.Logon.MsgType && message.ResetSeqNumFlag)
            {
                State = State.Resetting;
                OutgoingSeqNum = 1;
                IncomingSeqNum = 1;
            }

            message.Incoming = false;
            message.Fields.Set(FIX_5_0SP2.Fields.BeginString, BeginString.BeginString);

            if (BeginString.BeginString == Versions.FIXT_1_1.BeginString &&
                message.MsgType != FIX_5_0SP2.Messages.Logon.MsgType)
            {
                // Remove unpopulated optional header fields.
                if (message.Fields.Find(FIX_5_0SP2.Fields.ApplVerID) is Field ApplVerID && string.IsNullOrEmpty(ApplVerID.Value))
                {
                    message.Fields.Remove(ApplVerID.Tag);
                }

                if (message.Fields.Find(FIX_5_0SP2.Fields.CstmApplVerID) is Field CstmApplVerID && string.IsNullOrEmpty(CstmApplVerID.Value))
                {
                    message.Fields.Remove(CstmApplVerID.Tag);
                }

                if (message.Fields.Find(FIX_5_0SP2.Fields.ApplExtID) is Field ApplExtID && string.IsNullOrEmpty(ApplExtID.Value))
                {
                    message.Fields.Remove(ApplExtID.Tag);
                }
            }

            message.Fields.Set(FIX_5_0SP2.Fields.SenderCompID, SenderCompId);
            message.Fields.Set(FIX_5_0SP2.Fields.TargetCompID, TargetCompId);

            if (setSeqNum)
            {
                message.Fields.Set(FIX_5_0SP2.Fields.MsgSeqNum, AllocateOutgoingSeqNum());
            }

            message.Fields.Set(FIX_5_0SP2.Fields.SendingTime, Field.TimestampString(OutgoingTimestampFormat));

            if (_writer != null)
            {
                _writer.FragmentMessages = FragmentMessages;
                _writer.Write(message);
            }

            OnMessageSent(message);
        }
        catch (EndOfStreamException ex)
        {
            OnError(ex.Message);
            Close();
        }
        catch (Exception ex)
        {
            OnError(ex.Message);
            Close();
        }
    }

    Stream? _stream;
    Reader? _reader;
    Writer? _writer;
}

