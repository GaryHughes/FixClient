/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: Session.Transport.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
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
                _reader = new Reader(_stream)
                {
                    ValidateDataFields = ValidateDataFields
                };
                if (_writer != null)
                {
                    _writer.MessageWriting -= WriterMessageWriting;
                }
                _writer = new Writer(_stream, true);
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

            _writer?.Close();
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

            if (BeginString.BeginString == "FIXT_1_1" &&
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

