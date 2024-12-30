/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: SessionTestsBase.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using static Fix.Dictionary;

namespace FixTests;

public class SessionTestsBase<TSession> where TSession : Fix.Session, new()
{
    const string Host = "127.0.0.1";
    int Port = 20101;
    TcpListener? _listener;
    TcpClient? _client;
    Socket? _socket;
    BlockingCollection<Fix.Message>? _initiatorIncomingMessages;
    BlockingCollection<Fix.Message>? _acceptorIncomingMessages;
    BlockingCollection<Fix.Message>? _acceptorOutgoingMessages;
    protected TSession Initiator = new();
    protected TSession Acceptor = new();
    BlockingCollection<Fix.State>? _initatorStates;
    BlockingCollection<Fix.State>? _acceptorStates;
    ManualResetEventSlim? _connectionEstablished;

#if DEBUG
    protected const int Timeout = int.MaxValue;
#else
    protected const int Timeout = 5000;
#endif

    void AcceptTcpClientCallback(IAsyncResult ar)
    {
        if (ar.AsyncState is not TcpListener listener)
        {
            Assert.Fail("AcceptTcpClientCallback AsyncState is not the expected TcpListener");
            return;
        }
        _socket = listener.EndAcceptSocket(ar);
        _socket.NoDelay = true;
        Acceptor = new TSession
        {
            Stream = new Fix.NetworkStream(_socket, true),
            LogonBehaviour = Fix.Behaviour.Acceptor
        };
        Acceptor.Error += Acceptor_Error;
        Acceptor.MessageReceived += (sender, ev) =>
        {
            Acceptor.Messages.Add(ev.Message);
            _acceptorIncomingMessages?.Add(ev.Message);
        };
        Acceptor.MessageSent += (sender, ev) =>
        {
            Acceptor.Messages.Add(ev.Message);
            _acceptorOutgoingMessages?.Add(ev.Message);
        };
        Acceptor.StateChanged += (sender, ev) => _acceptorStates?.Add(ev.State);
        _connectionEstablished?.Set();
    }

    protected void Initialize()
    {
        var random = new Random(Environment.TickCount);
        Port = random.Next(1024, UInt16.MaxValue);
        _connectionEstablished = new ManualResetEventSlim(false);
        _initiatorIncomingMessages = new BlockingCollection<Fix.Message>();
        _acceptorIncomingMessages = new BlockingCollection<Fix.Message>();
        _acceptorOutgoingMessages = new BlockingCollection<Fix.Message>();
        _initatorStates = new BlockingCollection<Fix.State>();
        _acceptorStates = new BlockingCollection<Fix.State>();

        if (Fix.Network.GetLocalAddress(Host) is not System.Net.IPAddress address)
        {
            Assert.Fail("Could not resolve address for {Host}");
            return;
        }

        _listener = new TcpListener(address, Port);
        _listener.Start();
        _listener.BeginAcceptSocket(AcceptTcpClientCallback, _listener);
        _client = new TcpClient { NoDelay = true };
        _client.Connect(Fix.Network.GetAddress(Host), Port);
        Initiator = new TSession
        {
            Stream = _client.GetStream(),
            LogonBehaviour = Fix.Behaviour.Initiator
        };
        Initiator.Error += Initiator_Error;
        Initiator.MessageReceived += (sender, ev) =>
        {
            Initiator.Messages.Add(ev.Message);
            _initiatorIncomingMessages.Add(ev.Message);
        };
        Initiator.MessageSent += (sender, ev) => Initiator.Messages.Add(ev.Message);
        Initiator.StateChanged += (sender, ev) => _initatorStates.Add(ev.State);
        Assert.IsTrue(_connectionEstablished.Wait(Timeout));
    }

    void Acceptor_Error(object sender, Fix.Session.LogEvent ev)
    {
        Console.WriteLine("Acceptor error: " + ev.Message);
    }

    void Initiator_Error(object sender, Fix.Session.LogEvent ev)
    {
        Console.WriteLine("Initiator error: " + ev.Message);
    }

    protected void Reconnect()
    {
        Acceptor = new TSession();
        _connectionEstablished?.Reset();
        _listener?.BeginAcceptSocket(AcceptTcpClientCallback, _listener);
        _client = new TcpClient { NoDelay = true };
        _client.Connect(Fix.Network.GetAddress(Host), Port);
        Assert.IsTrue(_connectionEstablished?.Wait(Timeout) ?? false);
        if (Initiator is TSession)
        {
            Initiator.Stream = _client.GetStream();
        }
    }

    protected void Cleanup()
    {
        _listener?.Stop();
        _client?.Close();
        Acceptor?.Close();
        Initiator?.Close();
    }

    protected void StandardLogonSequence()
    {
        if (Initiator is null)
        {
            return;
        }

        // Always specify the comp id's using the initiator values because we have tests that
        // don't set them in the acceptor.
        ReceiveAtAcceptor(FIX_5_0SP2.Messages.Logon, new[]
        {
            new Fix.Field(FIX_5_0SP2.Fields.SenderCompID, Initiator.SenderCompId),
            new Fix.Field(FIX_5_0SP2.Fields.TargetCompID, Initiator.TargetCompId)
        });

        SentFromAcceptor(FIX_5_0SP2.Messages.Logon, new[]
        {
            new Fix.Field(FIX_5_0SP2.Fields.SenderCompID, Initiator.TargetCompId),
            new Fix.Field(FIX_5_0SP2.Fields.TargetCompID, Initiator.SenderCompId)
        });

        ReceiveAtInitiator(FIX_5_0SP2.Messages.Logon, new[]
        {
            new Fix.Field(FIX_5_0SP2.Fields.SenderCompID, Initiator.TargetCompId),
            new Fix.Field(FIX_5_0SP2.Fields.TargetCompID, Initiator.SenderCompId)
        });

        AcceptorStateChange(Fix.State.LoggingOn);
        InitiatorStateChange(Fix.State.LoggingOn);

        SentFromAcceptor(FIX_5_0SP2.Messages.TestRequest, new[]
        {
            new Fix.Field(FIX_5_0SP2.Fields.SenderCompID, Initiator.TargetCompId),
            new Fix.Field(FIX_5_0SP2.Fields.TargetCompID, Initiator.SenderCompId)
        });

        ReceiveAtInitiator(FIX_5_0SP2.Messages.TestRequest, new[]
        {
            new Fix.Field(FIX_5_0SP2.Fields.SenderCompID, Initiator.TargetCompId),
            new Fix.Field(FIX_5_0SP2.Fields.TargetCompID, Initiator.SenderCompId)
        });

        ReceiveAtAcceptor(FIX_5_0SP2.Messages.TestRequest, new[]
        {
            new Fix.Field(FIX_5_0SP2.Fields.SenderCompID, Initiator.SenderCompId),
            new Fix.Field(FIX_5_0SP2.Fields.TargetCompID, Initiator.TargetCompId)
        });

        SentFromAcceptor(FIX_5_0SP2.Messages.Heartbeat, new[]
        {
            new Fix.Field(FIX_5_0SP2.Fields.SenderCompID, Initiator.TargetCompId),
            new Fix.Field(FIX_5_0SP2.Fields.TargetCompID, Initiator.SenderCompId)
        });

        ReceiveAtInitiator(FIX_5_0SP2.Messages.Heartbeat, new[]
        {
            new Fix.Field(FIX_5_0SP2.Fields.SenderCompID, Initiator.TargetCompId),
            new Fix.Field(FIX_5_0SP2.Fields.TargetCompID, Initiator.SenderCompId)
        });

        ReceiveAtAcceptor(FIX_5_0SP2.Messages.Heartbeat, new[]
        {
            new Fix.Field(FIX_5_0SP2.Fields.SenderCompID, Initiator.SenderCompId),
            new Fix.Field(FIX_5_0SP2.Fields.TargetCompID, Initiator.TargetCompId)
        });
    }

    protected void AcceptorStateChange(Fix.State expected)
    {
        if (_acceptorStates is null)
        {
            return;
        }
        Assert.IsTrue(_acceptorStates.TryTake(out Fix.State actual, Timeout), $"Timeout waiting for acceptor state change {expected}");
        Assert.AreEqual(expected, actual);
    }

    protected void InitiatorStateChange(Fix.State expected)
    {
        if (_initatorStates is null)
        {
            return;
        }
        Assert.IsTrue(_initatorStates.TryTake(out Fix.State actual, Timeout), $"Timeout waiting for initiator state change {expected}");
        Assert.AreEqual(expected, actual);
    }

    protected Fix.Message ReceiveAtInitiator(Fix.Dictionary.Message definition, IEnumerable<Fix.Field>? expectedFields = null)
    {
        return Expect(_initiatorIncomingMessages, definition, expectedFields);
    }

    protected Fix.Message ReceiveAtAcceptor(Fix.Dictionary.Message definition, IEnumerable<Fix.Field>? expectedFields = null)
    {
        return Expect(_acceptorIncomingMessages, definition, expectedFields);
    }

    static string MsgTypeName(string msgType)
    {
        var definition = FIX_5_0SP2.Messages[msgType];
        if (definition == null)
        {
            return msgType;
        }
        return definition.Name;
    }

    static Fix.Message Expect(BlockingCollection<Fix.Message>? messages, Fix.Dictionary.Message definition, IEnumerable<Fix.Field>? expectedFields)
    {
        if (messages is not BlockingCollection<Fix.Message>)
        {
            throw new Exception("messages is not the expected type")
;
        }

        Assert.IsTrue(messages.TryTake(out Fix.Message? message, Timeout), $"Timed out waiting for MsgType={definition.Name}");

        if (message is null)
        {
            throw new Exception("message is null");
        }

        Assert.AreEqual(definition.MsgType, message.MsgType, $"Found MsgType={MsgTypeName(message.MsgType)} when we expected MsgType={definition.Name}\n{message}");

        if (expectedFields == null)
            return message;

        var fields = message.Fields.GetEnumerator();

        foreach (var expected in expectedFields)
        {
            Fix.Field actual;
            for (; ; )
            {
                Assert.IsTrue(fields.MoveNext(), $"Expected field {expected} is not present or is not in the expected position");
                actual = fields.Current;
                if (actual.Tag == expected.Tag)
                    break;
            }

            Assert.AreEqual(expected.Value, actual.Value, $"{expected.Tag} = {actual.Value} when expecting {expected.Value}");
        }

        return message;
    }

    protected void SentFromAcceptor(Fix.Dictionary.Message definition, IEnumerable<Fix.Field>? fields = null)
    {
        Expect(_acceptorOutgoingMessages, definition, fields);
    }

    protected void SendFromAcceptor(Fix.Dictionary.Message definition, IEnumerable<Fix.Field>? fields = null)
    {
        var message = new Fix.Message { MsgType = definition.MsgType };

        if (fields != null)
        {
            foreach (Fix.Field field in fields)
            {
                message.Fields.Add(field.Tag, field.Value);
            }
        }

        Acceptor?.Send(message);
    }

    protected void SendFromInitiator(Fix.Dictionary.Message definition, IEnumerable<Fix.Field>? fields = null)
    {
        var message = new Fix.Message { MsgType = definition.MsgType };

        if (fields != null)
        {
            foreach (Fix.Field field in fields)
            {
                if (field.Tag == FIX_5_0SP2.Fields.MsgSeqNum.Tag)
                {
                    message.Fields.Set(field.Tag, field.Value);
                }
                else
                {
                    message.Fields.Add(field.Tag, field.Value);
                }
            }
        }

        Initiator?.Send(message);
    }
}

